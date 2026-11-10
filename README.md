# MDRN_HATS

Base de una tienda de hats/moda con API ASP.NET Core 8, PostgreSQL y frontend React + Vite + TypeScript.

## Stack y estructura

- `backend/Api`: Web API, autenticación JWT, Swagger y endpoints HTTP.
- `backend/Application`: contratos, validadores e interfaces de servicios.
- `backend/Domain`: entidades y reglas de dominio.
- `backend/Infrastructure`: Identity, EF Core, PostgreSQL y servicios.
- `backend/Tests`: pruebas xUnit.
- `frontend`: aplicación React con React Router y Tailwind CSS.
- `scripts/backup-db.sh`: respaldo comprimido de PostgreSQL con rotación.

## Requisitos

- .NET 8 SDK
- Node.js 20.19+ o 22.12+
- Docker Compose v2 (para la base de datos local)

## Desarrollo local

1. Copia `.env.example` a `.env` y sustituye los valores de ejemplo por valores únicos. `JWT_KEY` debe tener al menos 32 bytes; `ADMIN_PASSWORD` debe cumplir la política de contraseñas de Identity.
2. Inicia PostgreSQL y la API: `docker compose up --build`.
3. En `frontend`, copia `.env.example` a `.env`, instala dependencias con `npm install` y ejecuta `npm run dev`.
4. La API queda en `http://localhost:8080`; Swagger está disponible en modo Development en `/swagger`.

La API aplica las migraciones al iniciar y crea los roles `Admin` y `Customer`. Si están definidas, `ADMIN_EMAIL` y `ADMIN_PASSWORD` se usan para crear el usuario Admin inicial. No guardes `.env` en Git.

Para compilar y probar el backend: `dotnet build MDRN_HATS.sln` y `dotnet test MDRN_HATS.sln`.
Para compilar el frontend: `cd frontend`, `npm install` y `npm run build`.

## Variables de entorno

| Variable | Uso |
| --- | --- |
| `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` | Base de datos del contenedor PostgreSQL |
| `ConnectionStrings__DefaultConnection` | Cadena de conexión de la API |
| `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE` | Firma y validación de tokens |
| `JWT_ACCESS_TOKEN_MINUTES`, `JWT_REFRESH_TOKEN_DAYS` | Duración de tokens |
| `ADMIN_EMAIL`, `ADMIN_PASSWORD` | Usuario inicial Admin |
| `CORS_ALLOWED_ORIGINS` | Orígenes frontend separados por coma |
| `API_PORT` | Puerto publicado para la API |
| `ASPNETCORE_ENVIRONMENT` | Entorno ASP.NET (`Development` habilita Swagger) |
| `VITE_API_URL` | URL base de la API para Vite |
| `PGHOST`, `PGPORT`, `PGDATABASE`, `PGUSER`, `PGPASSWORD` | Conexión del script de respaldo |
| `BACKUP_DIR`, `RETENTION_DAYS` | Destino de respaldos y días de retención |

Para crear un respaldo desde el host/NAS, exporta las variables `PG*` y `BACKUP_DIR`, y ejecuta `bash scripts/backup-db.sh` desde una tarea cron nocturna. El destino puede ser un directorio montado del NAS. Por defecto, se eliminan archivos de respaldo con más de 14 días.

## Alcance de esta fase

Incluye entidades y persistencia inicial, autenticación, CRUD de productos y pantallas placeholder. Catálogo completo, carrito, checkout y pagos quedan para fases posteriores.
