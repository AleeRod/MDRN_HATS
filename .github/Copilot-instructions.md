# Tienda de gorras
E-commerce: tienda pública + panel admin

## Stack
- Backend: .NET 8 Web API, EF Core, PostgreSQL (Supabase)
- Frontend: React + Vite + TypeScript + Tailwind (tienda y /admin en un solo proyecto)
- Auth: ASP.NET Identity + JWT + refresh tokens, roles Admin/Customer
- Deploy: Cloudflare Pages (front), Render Starter (API), Supabase (BD)
- Backups: pg_dump nocturno al NAS (OpenMediaVault)

## Estructura
- backend/Api, Application, Domain, Infrastructure, Tests
- frontend/src/store, admin, shared, routes
- scripts/backup-db.sh

## Reglas
- Arquitectura en capas: Api → Application → Domain; Infrastructure → Application
- Validar stock y precios SIEMPRE en el backend
- Guardar precio unitario en OrderItems al comprar
- Soft delete en productos (campo Active)
- Nunca manejar datos de tarjeta; solo checkout de la pasarela
- Secretos en variables de entorno, nunca en el repo
- Endpoints de admin con [Authorize(Roles = "Admin")]
- Mobile-first en toda la UI
- Commits en español, convencionales (feat:, fix:)
- Al terminar una tarea: dotnet build, dotnet test y npm run build

## Comandos
- Backend: `dotnet run --project backend/Api`
- Frontend: `npm run dev` (en /frontend)
- Todo local: `docker compose up`