# Sistema de Ventas - API Backend

API REST para gestión de ventas, productos e inventario, con autenticación JWT y control de acceso por roles.

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

La base de datos `SistemaVentasDb` se crea sola, se aplican las migraciones y se crean los roles iniciales (`Admin` y `Analista`).

Para detener: `docker compose down` (conserva los datos). Para empezar de cero: `docker compose down -v`.

## Autenticación

La API usa tokens JWT:

1. Crea un usuario con `POST /api/auth/registrar`.
2. Inicia sesión con `POST /api/auth/login` y copia el token de la respuesta.
3. Envía el token en el header `Authorization: Bearer <token>` (en Swagger, con el botón **Authorize**).

Hay dos roles: **Admin** (acceso total) y **Analista** (lectura de productos, ventas y reportes; puede registrar ventas).

## Endpoints principales

| Método | Ruta | Descripción | Acceso |
|---|---|---|---|
| POST | `/api/auth/registrar` | Registra un usuario con rol `Admin` o `Analista` | Público |
| POST | `/api/auth/login` | Devuelve un token JWT (válido 2 horas) | Público |
| GET | `/api/auth/whoami` | Muestra los claims del token actual | Autenticado |
| GET | `/api/productos` | Lista los productos | Admin, Analista |
| GET | `/api/productos/{id}` | Detalle de un producto | Admin, Analista |
| POST | `/api/productos` | Crea un producto | Admin |
| PUT | `/api/productos/{id}` | Actualiza un producto | Admin |
| DELETE | `/api/productos/{id}` | Elimina un producto | Admin |
| GET | `/api/ventas` | Lista las ventas con su detalle, de la más reciente a la más antigua | Admin, Analista |
| POST | `/api/ventas` | Registra una venta y descuenta el stock | Admin, Analista |
| GET | `/api/reportes/ingresos` | Ingresos por día o mes (`desde`, `hasta`, `agrupar=dia\|mes`) | Admin, Analista |
| GET | `/api/reportes/mas-vendidos` | Top de productos por unidades, con ingresos y ganancia (`top`, `desde`, `hasta`) | Admin, Analista |
| POST | `/api/importacion/productos` | Importa o actualiza productos desde un archivo `.csv` o `.json` | Admin |

En `ApiDatos/ApiDatos.http` hay ejemplos de peticiones listos para probar (con la extensión REST Client de VS Code o con Visual Studio).

### Reglas de negocio destacadas

- Las ventas se guardan en una **transacción**: si algún producto no existe o no tiene stock suficiente, no se guarda nada.
- El **precio unitario lo define el backend** a partir del producto, no el cliente.
- La importación crea los productos nuevos y actualiza los existentes por **SKU** (único). Devuelve cuántos creó, cuántos actualizó y los errores por fila.
- Formato CSV de importación (con encabezado): `Nombre,Sku,PrecioCosto,PrecioVenta,StockActual,StockMinimo`.

## Estructura

- `ApiDatos/`: proyecto de la API (controladores, modelos, DTOs, contexto de datos, migraciones).
- `Dockerfile`: imagen de la API (build multi-stage).
- `docker-compose.yaml`: API + SQL Server con healthcheck.
- `.env.example`: plantilla de variables de entorno.

## Estado del proyecto

## Estado del proyecto

En desarrollo.

**Implementado**

- [x] Autenticación JWT con roles (`Admin` y `Analista`)
- [x] CRUD de productos con control de acceso por rol
- [x] Registro de ventas transaccional con validación de stock
- [x] Reportes de ingresos y productos más vendidos
- [x] Importación masiva de productos desde CSV/JSON
- [x] Docker Compose con migraciones automáticas

**Próximos pasos**

- [ ] Pruebas unitarias y de integración
- [ ] DTOs en los endpoints de productos
- [ ] Paginación y filtros en los listados