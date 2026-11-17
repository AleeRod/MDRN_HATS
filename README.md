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

## Desarrollo local

1. Guarda la configuración local de la API con .NET User Secrets. El proyecto ya tiene habilitado el almacenamiento de secretos fuera del repositorio:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=POOLER_HOST;Port=5432;Database=postgres;Username=POOLER_USER;Password=SUPABASE_DB_PASSWORD;SSL Mode=Require" --project backend\Api\MDRN_HATS.Api.csproj
   dotnet user-secrets set "Jwt:Key" "USA_UNA_CLAVE_ALEATORIA_DE_32_BYTES_O_MAS" --project backend\Api\MDRN_HATS.Api.csproj
   dotnet user-secrets set "Jwt:Issuer" "MDRN_HATS" --project backend\Api\MDRN_HATS.Api.csproj
   dotnet user-secrets set "Jwt:Audience" "MDRN_HATS" --project backend\Api\MDRN_HATS.Api.csproj
   dotnet user-secrets set "Cors:AllowedOrigins" "http://localhost:5173" --project backend\Api\MDRN_HATS.Api.csproj
   ```

   Sustituye los marcadores por los valores de **Session pooler** de Supabase y no publiques esos secretos. Para crear el usuario administrador inicial, configura también `Admin:Email` y `Admin:Password` con `dotnet user-secrets set`; la contraseña debe cumplir la política de Identity.
2. Inicia la API desde la raíz del repositorio: `dotnet run --project backend/Api`.
3. En `frontend`, copia `.env.example` a `.env`, instala dependencias con `npm install` y ejecuta `npm run dev`.
4. La API queda en `http://localhost:8080`; Swagger está disponible en modo Development en `/swagger`.

La API aplica las migraciones al iniciar y crea los roles `Admin` y `Customer`. Haz un respaldo antes de conectar una base que ya tenga datos. ASP.NET Core carga User Secrets automáticamente en Development; el `.env` de la raíz no se carga y se conserva sin cambios.

Para compilar y probar el backend: `dotnet build MDRN_HATS.sln` y `dotnet test MDRN_HATS.sln`.
Para compilar el frontend: `cd frontend`, `npm install` y `npm run build`.

## Variables de entorno

| Variable | Uso |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Cadena de conexión de la API |
| `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE` | Firma y validación de tokens |
| `JWT_ACCESS_TOKEN_MINUTES`, `JWT_REFRESH_TOKEN_DAYS` | Duración de tokens |
| `ADMIN_EMAIL`, `ADMIN_PASSWORD` | Usuario inicial Admin |
| `CORS_ALLOWED_ORIGINS` | Orígenes frontend separados por coma |
| `ASPNETCORE_ENVIRONMENT` | Entorno ASP.NET (`Development` habilita Swagger) |
| `VITE_API_URL` | URL base de la API para Vite |
| `PGHOST`, `PGPORT`, `PGDATABASE`, `PGUSER`, `PGPASSWORD` | Conexión del script de respaldo |
| `BACKUP_DIR`, `RETENTION_DAYS` | Destino de respaldos y días de retención |

Para crear un respaldo desde el host/NAS, exporta las variables `PG*` y `BACKUP_DIR`, y ejecuta `bash scripts/backup-db.sh` desde una tarea cron nocturna. El destino puede ser un directorio montado del NAS. Por defecto, se eliminan archivos de respaldo con más de 14 días.

## Alcance de esta fase

Incluye entidades y persistencia inicial, autenticación, CRUD de productos y pantallas placeholder. Catálogo completo, carrito, checkout y pagos quedan para fases posteriores.

Para el detalle de tablas, relaciones y migraciones, consulta [docs/BASE_DATOS.md](docs/BASE_DATOS.md).
