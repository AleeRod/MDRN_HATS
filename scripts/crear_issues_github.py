#!/usr/bin/env python3
"""Crea los issues del backlog de MDRN HATS en GitHub y los agrega al Project.

Uso (ver pasos en la conversación):
  python crear_issues_github.py --owner AleeRod --repo MDRN_HATS --project 1 --dry-run
  python crear_issues_github.py --owner AleeRod --repo MDRN_HATS --project 1 --limit 12
  python crear_issues_github.py --owner AleeRod --repo MDRN_HATS --project 1

Requisitos: GitHub CLI (gh) con sesión iniciada y permiso "project":
  gh auth login
  gh auth refresh -s project
El script guarda su avance en progreso_issues.json: si se corta, vuelve a
ejecutarlo y continúa sin duplicar issues.
"""
import argparse, csv, json, os, re, subprocess, sys, time

TYPE_LABEL = {"Epic": "epica", "Story": "historia", "Task": "tarea"}
STATUS_MAP = {"To Do": "backlog", "Done": "done"}
PRIORITY_ORDER = ["Highest", "High", "Medium", "Low", "Lowest"]


def gh(*args):
    r = subprocess.run(["gh", *args], capture_output=True, text=True)
    if r.returncode:
        raise RuntimeError((r.stderr or r.stdout).strip())
    return r.stdout


def gql(query, **variables):
    args = ["api", "graphql", "-f", "query=" + query]
    for k, v in variables.items():
        args += ["-F" if isinstance(v, int) else "-f", f"{k}={v}"]
    return json.loads(gh(*args))["data"]


PROJECT_QUERY = """query($owner:String!,$num:Int!){user(login:$owner){projectV2(number:$num){id
fields(first:50){nodes{__typename ... on ProjectV2FieldCommon{id name}
... on ProjectV2SingleSelectField{options{id name}}
... on ProjectV2IterationField{configuration{iterations{id title startDate}
completedIterations{id title startDate}}}}}}}}"""


def load_project(owner, num):
    p = gql(PROJECT_QUERY, owner=owner, num=num)["user"]["projectV2"]
    fields = {f["name"]: f for f in p["fields"]["nodes"] if f.get("name")}
    return p["id"], fields


def pick_priority(options, priority):
    """Asigna Highest..Lowest a las opciones que tenga el campo (p. ej. P0/P1/P2)."""
    names = {o["name"].lower(): o["id"] for o in options}
    if priority.lower() in names:
        return names[priority.lower()]
    i = PRIORITY_ORDER.index(priority) if priority in PRIORITY_ORDER else 2
    idx = int(i * (len(options) - 1) / 4 + 0.5)
    return options[idx]["id"]


def pick_status(options, status):
    want = STATUS_MAP.get(status, "backlog")
    for o in options:
        if o["name"].lower() == want:
            return o["id"]
    return None


def pick_iteration(field, sprint):
    m = re.search(r"Sprint (\d+)", sprint or "")
    if not m:
        return None
    cfg = field["configuration"]
    its = sorted(cfg["iterations"] + cfg["completedIterations"], key=lambda x: x["startDate"])
    n = int(m.group(1)) - 1
    return its[n]["id"] if n < len(its) else None


def set_field(project_id, item_id, field_id, literal):
    gql(
        'mutation{updateProjectV2ItemFieldValue(input:{projectId:"%s",itemId:"%s",fieldId:"%s",value:%s})'
        "{projectV2Item{id}}}" % (project_id, item_id, field_id, literal)
    )


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--owner", required=True)
    ap.add_argument("--repo", required=True)
    ap.add_argument("--project", type=int, required=True, help="número del proyecto (está en su URL)")
    ap.add_argument("--csv", default="mdrn_hats_backlog_jira.csv")
    ap.add_argument("--limit", type=int, default=0, help="procesar solo las primeras N filas (prueba)")
    ap.add_argument("--dry-run", action="store_true")
    a = ap.parse_args()

    rows = list(csv.DictReader(open(a.csv, encoding="utf-8")))
    if a.limit:
        rows = rows[: a.limit]
    epics = {r["Issue Id"]: r["Summary"] for r in rows if r["Issue Type"] == "Epic"}
    repo = f"{a.owner}/{a.repo}"

    if a.dry_run:
        for r in rows:
            print(f'[{r["Issue Type"]:5}] {r["Summary"]}  | {r["Priority"]} | {r["Story Points"] or "-"} pts | {r["Sprint"] or "sin sprint"} | {r["Status"]}')
        print(f"\n{len(rows)} filas. Nada se creó (dry-run).")
        return

    project_id, fields = load_project(a.owner, a.project)
    for need in ("Status", "Priority", "Estimate", "Iteration"):
        if need not in fields:
            print(f"AVISO: el proyecto no tiene el campo '{need}'; se omitirá.")

    # Etiquetas
    labels = set()
    for r in rows:
        labels.add(TYPE_LABEL[r["Issue Type"]])
        if r["Labels"]:
            labels.add(r["Labels"])
        if r["Parent"] in epics:
            labels.add(("epica: " + epics[r["Parent"]])[:50])
    for name in sorted(labels):
        color = "d4af37" if name.startswith("epic") else "ededed"
        try:
            gh("label", "create", name, "--repo", repo, "--color", color, "--force")
        except RuntimeError as e:
            print("No se pudo crear la etiqueta", name, "->", e)

    path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "progreso_issues.json")
    prog = json.load(open(path)) if os.path.exists(path) else {}
    save = lambda: json.dump(prog, open(path, "w"), indent=1)

    for r in rows:
        cid, typ = r["Issue Id"], r["Issue Type"]
        st = prog.setdefault(cid, {})
        try:
            if "number" not in st:
                lbls = [TYPE_LABEL[typ]] + ([r["Labels"]] if r["Labels"] else [])
                if r["Parent"] in epics:
                    lbls.append(("epica: " + epics[r["Parent"]])[:50])
                title = ("[ÉPICA] " if typ == "Epic" else "") + r["Summary"]
                body = r["Description"]
                if r["Parent"] in epics:
                    body += f'\n\n---\nÉpica: {epics[r["Parent"]]}'
                args = ["api", f"repos/{repo}/issues", "--method", "POST", "-f", f"title={title}", "-f", f"body={body}"]
                for l in lbls:
                    args += ["-f", f"labels[]={l}"]
                d = json.loads(gh(*args))
                st.update(number=d["number"], id=d["id"], node=d["node_id"])
                save()
                time.sleep(1)  # evita límites de velocidad de GitHub
                print(f'Issue #{d["number"]}: {title}')

            # Sub-issue de su épica (si falla, queda la etiqueta de épica)
            if r["Parent"] in epics and not st.get("linked"):
                pe = prog.get(r["Parent"], {})
                if "number" in pe:
                    try:
                        gh("api", f'repos/{repo}/issues/{pe["number"]}/sub_issues', "--method", "POST", "-F", f'sub_issue_id={st["id"]}')
                        st["linked"] = True
                    except RuntimeError as e:
                        print(f'  (sin vincular como sub-issue: {e})')
                    save()

            # Agregar al proyecto y llenar campos
            if not st.get("fields_done"):
                if "item" not in st:
                    out = gql(
                        'mutation{addProjectV2ItemById(input:{projectId:"%s",contentId:"%s"}){item{id}}}' % (project_id, st["node"])
                    )
                    st["item"] = out["addProjectV2ItemById"]["item"]["id"]
                    save()
                it = st["item"]
                if "Status" in fields:
                    oid = pick_status(fields["Status"]["options"], r["Status"])
                    if oid:
                        set_field(project_id, it, fields["Status"]["id"], '{singleSelectOptionId:"%s"}' % oid)
                if "Priority" in fields and r["Priority"]:
                    oid = pick_priority(fields["Priority"]["options"], r["Priority"])
                    set_field(project_id, it, fields["Priority"]["id"], '{singleSelectOptionId:"%s"}' % oid)
                if "Estimate" in fields and r["Story Points"]:
                    set_field(project_id, it, fields["Estimate"]["id"], "{number:%s}" % int(r["Story Points"]))
                if "Iteration" in fields and r["Sprint"]:
                    iid = pick_iteration(fields["Iteration"], r["Sprint"])
                    if iid:
                        set_field(project_id, it, fields["Iteration"]["id"], '{iterationId:"%s"}' % iid)
                    else:
                        print(f'  AVISO: no hay iteración para "{r["Sprint"]}" (créalas en el proyecto)')
                if r["Status"] == "Done" and not st.get("closed"):
                    gh("api", f'repos/{repo}/issues/{st["number"]}', "--method", "PATCH", "-f", "state=closed", "-f", "state_reason=completed")
                    st["closed"] = True
                st["fields_done"] = True
                save()
        except Exception as e:  # sigue con la siguiente fila
            print(f'ERROR en fila {cid} ({r["Summary"][:40]}): {e}')
            save()

    print("\nListo. Revisa el proyecto en GitHub.")


if __name__ == "__main__":
    sys.exit(main())
