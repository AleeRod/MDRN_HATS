# Conexión y comandos de EF

Abre PowerShell en la raíz del repositorio y ejecuta:

```powershell
.\Connection\Run-Ef.ps1
```

Pega el connection string de un proyecto Supabase separado para pruebas cuando
se solicite. La entrada se oculta y solo se conserva en el proceso mientras
corre el script. `Database=postgres` por sí solo no convierte una base de
producción en una base de pruebas.

El modo predeterminado (`Check`) consulta el acceso y el estado de las
migraciones sin aplicarlas. Para generar el SQL idempotente en la carpeta
temporal de Windows:

```powershell
.\Connection\Run-Ef.ps1 -Action Script
```

Este helper no contiene una acción para aplicar migraciones. Nunca pegues
credenciales en archivos del repositorio ni uses una cadena de producción
para una prueba.
