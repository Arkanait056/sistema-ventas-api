# Sistema de Ventas - API Backend

API REST para gestión de ventas, productos e inventario, con autenticación JWT y roles.

> Proyecto en desarrollo.

## Tecnologías

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core (migraciones automáticas al iniciar)
- SQL Server 2022
- ASP.NET Core Identity + JWT (roles `Admin` y `Analista`)
- Swagger / OpenAPI
- Docker y Docker Compose

## Cómo ejecutarlo

Requisitos: [Docker Desktop](https://www.docker.com/products/docker-desktop/).

1. Clona el repositorio.
2. Copia `.env.example` como `.env` y ajusta los valores (`SA_PASSWORD` y `JWT_KEY`).
3. Levanta todo:

   ```bash
   docker compose up --build
   ```

4. Abre Swagger en <http://localhost:5000/swagger>.

La base de datos `SistemaVentasDb` se crea sola, se aplican las migraciones y se crean los roles iniciales.

Para detener: `docker compose down` (conserva los datos). Para empezar de cero: `docker compose down -v`.

## Estructura

- `ApiDatos/`: proyecto de la API (controladores, modelos, contexto de datos, migraciones).
- `Dockerfile`: imagen de la API (build multi-stage).
- `docker-compose.yml`: API + SQL Server con healthcheck.

## Autenticación

La API usa tokens JWT. Obtén un token con el endpoint de login y envíalo en el header
`Authorization: Bearer <token>` (en Swagger, con el botón **Authorize**).
