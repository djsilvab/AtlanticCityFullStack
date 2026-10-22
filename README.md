# Pedidos · API + SPA (Reto técnico Fullstack Senior)

Aplicación end-to-end de **autenticación JWT + CRUD de pedidos** con arquitectura por capas
(.NET 8) y SPA en React 18 + TypeScript + Vite.

- **Backend:** ASP.NET Core Web API (controllers) · EF Core 8 · SQL Server 2022 (Docker) · JWT Bearer · Serilog · Polly
- **Frontend:** React 18 · TypeScript estricto · Vite 7 · Tailwind CSS 4 · React Router 7 · Axios · React Hook Form + Zod
- **Tests:** xUnit + FluentAssertions + NSubstitute (unitarios) · `WebApplicationFactory` (integración contra SQL Server real) · Vitest + Testing Library (frontend)

---

## 1. Arquitectura

```
┌────────────────────────────┐        HTTPS/JSON + JWT        ┌──────────────────────────────┐        EF Core 8         ┌──────────────────┐
│  React SPA (Vite, :5173)   │  ────────────────────────────▶ │  Pedidos.Api (:5080)         │  ────────────────────▶ │  SQL Server     │
│  Tailwind · RHF + Zod      │  ◀──────────────────────────── │  Controllers + IException-   │  ◀──────────────────── │  2022 (Docker)  │
│  sessionStorage (D8)       │        ProblemDetails          │  Handler + Serilog           │                       │  :1433           │
└────────────────────────────┘                                └──────────────┬───────────────┘                       └──────────────────┘
                                                                             │
                                                        ┌────────────────────┴────────────────────┐
                                                        │                                         │
                                             ┌───────▼────────┐                          ┌───────▼────────┐
                                             │  Application   │                          │  Infrastructure│
                                             │  casos de uso  │◀─── puertos (interfaces) ─▶│  EF Core, JWT  │
                                             │  DTOs, reglas  │                          │  hashing, seed │
                                             └───────┬────────┘                          └───────┬────────┘
                                                     │                                          │
                                             ┌───────▼────────┐                                 │
                                             │     Domain     │◀────── entidades, invariantes ───┘
                                             │  sin dependencias│
                                             └────────────────┘
```

Regla de dependencias: `Api → Application, Infrastructure` · `Infrastructure → Application → Domain`.
**Domain no depende de nada.** Los *puertos* (interfaces) viven en Application; las implementaciones, en Infrastructure.

| Capa | Contenido |
|---|---|
| `src/Pedidos.Domain` | Entidades `Pedido` y `Usuario` con invariantes, enums `EstadoPedido`/`Rol`, excepciones de dominio |
| `src/Pedidos.Application` | Casos de uso (`PedidoService`, `AuthService`), DTOs, validadores FluentValidation, puertos (`IPedidoRepository`, `IUsuarioRepository`, `IUnitOfWork`, `ITokenService`, `IPasswordHasher`) |
| `src/Pedidos.Infrastructure` | `AppDbContext` + configuraciones EF, repositorios, `JwtTokenService`, hashing, seed, migraciones, pipeline de Polly |
| `src/Pedidos.Api` | Controllers, middleware, `IExceptionHandler`, DI, Swagger, CORS, rate limiting, health check |
| `frontend` | SPA React organized by feature (`auth`, `pedidos`) + componentes compartidos |

---

## 2. Requisitos previos

| Herramienta | Versión mínima | Notas |
|---|---|---|
| .NET SDK | **8.0** | El proyecto apunta a `net8.0` (fallback del enunciado: no hay SDK 9 en la máquina de desarrollo) |
| Node.js | 20.19+ / 22.12+ | Probado con Node 22.23 |
| Docker Desktop | Cualquier versión reciente | Solo para SQL Server 2022 |
| Git | 2.30+ | — |

---

## 3. Puesta en marcha (10 minutos)

### 3.1. Base de datos

```bash
# 1) Copia la plantilla de variables y ajusta la contraseña
cp .env.example .env      # en Windows: copy .env.example .env

# 2) Levanta SQL Server 2022 (lee la contraseña del .env)
docker compose up -d db

# 3) Comprueba que el contenedor está healthy
docker compose ps
```

> La contraseña de `sa` se define en `MSSQL_SA_PASSWORD` dentro de `.env` (ignorado por git).
> El API lee la cadena de conexión de `ConnectionStrings__Pedidos` en ese mismo archivo.

### 3.2. API

```bash
# Restaura y compila
dotnet build

# Arranca la API en http://localhost:5080
dotnet run --project src/Pedidos.Api
```

Al arrancar, la API:

1. aplica las migraciones de EF Core (con reintentos hasta que SQL Server esté disponible),
2. crea la base de datos y sus tablas si no existen,
3. en `Development` inserta los usuarios de prueba (seed idempotente).

Documentación interactiva: <http://localhost:5080/swagger> · Health check: <http://localhost:5080/health>

### 3.3. Frontend

```bash
cd frontend
npm install
npm run dev          # http://localhost:5173
```

La URL de la API se toma de `frontend/.env` (plantilla en `frontend/.env.example`):

```
VITE_API_URL=http://localhost:5080
```

Si no existe, el cliente usa `http://localhost:5080` por defecto.

### 3.4. Cierre

```bash
docker compose down          # para parar (conserva el volumen)
docker compose down -v       # para parar y borrar los datos
```

---

## 4. Credenciales de prueba

| Email | Contraseña | Rol | Puede |
|---|---|---|---|
| `user@email.com` | `123456` | `User` | Listar, ver, crear y editar pedidos |
| `admin@email.com` | `Admin123!` | `Admin` | Todo lo anterior **y eliminar** (eliminación lógica) |

---

## 5. Endpoints

| Método | Ruta | Auth | Respuestas |
|---|---|---|---|
| `POST` | `/auth/login` | Anónimo | 200 `{ token, expiresIn }` · 400 (errores por campo) · 401 (credenciales inválidas) · 429 (rate limit) |
| `GET` | `/api/pedidos` | Bearer | 200 lista de pedidos (orden por fecha desc) |
| `GET` | `/api/pedidos/paginado` | Bearer | 200 lista paginada (`pagina`, `pageSize`, `estado`, `cliente`) |
| `GET` | `/api/pedidos/{id}` | Bearer | 200 · 404 |
| `POST` | `/api/pedidos` | Bearer | 201 + `Location` · 400 · 409 (número duplicado) |
| `PUT` | `/api/pedidos/{id}` | Bearer | 200 · 400 · 404 · 409 |
| `DELETE` | `/api/pedidos/{id}` | Bearer (**Admin**) | 204 · 403 (rol insuficiente) · 404 |
| `GET` | `/health` | Anónimo | 200 con comprobación de base de datos |

Todos los errores usan **ProblemDetails (RFC 7807)** con `type`, `title`, `status`, `detail`, `instance`, `traceId`
y, en los errores de validación, `errors` por campo.

<details>
<summary>Ejemplo de error de validación (400)</summary>

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Uno o más errores de validación se han producido.",
  "status": 400,
  "detail": "Revisa los campos indicados e inténtalo de nuevo.",
  "instance": "/api/pedidos",
  "errors": {
    "numeroPedido": ["El número de pedido es obligatorio."],
    "total": ["El total debe ser mayor que cero."]
  },
  "traceId": "0HNPG0J38P6VS:00000001"
}
```

</details>

---

## 6. Tests

```bash
# Backend (unitarios + integración)
dotnet test

# Cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Frontend
cd frontend
npm test
npm run build      # comprobación de tipos + bundle de producción
```

| Suite | Contenido | Requisitos |
|---|---|---|
| `tests/Pedidos.UnitTests` | Invariantes del dominio, validadores, `AuthService` y `PedidoService` con dobles de prueba | Ninguno |
| `tests/Pedidos.IntegrationTests` | API real (`WebApplicationFactory`) contra SQL Server: login, 401/403/404/409, CRUD, rate limiting, health check, cabeceras de seguridad | Docker con `docker compose up -d db` |
| `frontend/src/test` | `ProtectedRoute` (redirección y restauración de sesión) y validación del formulario de pedido | Ninguno |

Los tests de integración usan una base **aislada** (`Pedidos_Tests`) y aplican las migraciones reales,
de modo que también validan el esquema y las migraciones.

---

## 7. Decisiones técnicas y trade-offs

| # | Decisión | Motivo / trade-off |
|---|---|---|
| D1 | **`net8.0`** (no .NET 9) | No hay SDK 9 instalado; 8 es LTS y cumple el enunciado |
| D2 | **SQL Server 2022 en Docker** | Reproducible con `docker compose`; el modelo es T-SQL |
| D3 | **Sin MediatR ni AutoMapper** | Menos dependencias, servicios legibles; el mapeo es manual y explícito |
| D4 | **Controllers** (no Minimal APIs) | Encaja con `[Authorize]` y con los atributos de rol del enunciado |
| D5 | **Eliminación lógica** (`EsEliminado` + query filter global) | El registro se conserva; `NumeroPedido` sigue siendo único también entre eliminados |
| D6 | **`DELETE` solo para `Admin`** | Da sentido a los roles; `User` puede listar, ver, crear y editar |
| D7 | **Resiliencia donde hay fallos transitorios**: Polly en la migración y `EnableRetryOnFailure` en EF | No hay llamadas HTTP salientes: un circuit breaker HTTP no aportaría nada |
| D8 | **Token JWT en `sessionStorage`** | Sencillez y ausencia de cookies; trade-off conocido: un XSS puede leer el token. La alternativa recomendada en producción es cookie `httpOnly` + `SameSite=Strict` + CSRF token |
| D9 | **Seed de usuarios en `Development`** | Facilita la evaluación; en otros entornos no se ejecuta |
| D10 | **Puertos 5080 / 5173 / 1433** | Estándar y sin conflictos |
| D11 | **Tests de integración contra SQL Server real** | Maxima fidelidad (índices únicos, `decimal(10,2)`, query filters); a cambio, requieren Docker |
| D12 | **`dotnet-ef` como local tool (8.0.32)** | El `dotnet ef` global (10.x) no es compatible con EF Core 8 |
| D13 | **FluentAssertions 7.x** | La 8.x exige licencia comercial; la 7.x es Apache-2.0 |
| D14 | **Clave JWT fuera del repo** | Si no está configurada, en `Development` se genera una clave efímera en memoria (warning en el log); en otros entornos el arranque falla indicando el comando exacto |
| D15 | **React 18 + Vite 7 + Tailwind 4 + Zod 4** | Versiones estables actuales y compatibles entre sí |
| D16 | **`Microsoft.Extensions.Identity.Core`** | Paquete mínimo que aporta `PasswordHasher<Usuario>` sin arrastrar todo Identity |
| D17 | **CORS con el middleware de ASP.NET Core** | Comportamiento estándar, preflight correcto y menos código propio |

### Modelo de datos

`Pedidos`: `Id` (identity) · `NumeroPedido` (nvarchar 50, **único**) · `Cliente` (nvarchar 150) · `Fecha` (datetime2) ·
`Total` (decimal 10,2) · `Estado` (nvarchar 20: `Registrado`, `EnProceso`, `Completado`, `Cancelado`) ·
`EsEliminado` (bit) · `FechaEliminacion` (datetime2, null).

`Usuarios`: `Id` (identity) · `Email` (nvarchar 200, **único**) · `PasswordHash` (nvarchar 500) · `Rol` (nvarchar 20) · `Activo` (bit).

Scripts SQL equivalentes a la migración: [`db/01-schema.sql`](db/01-schema.sql) y [`db/02-seed.sql`](db/02-seed.sql).

---

## 8. Resiliencia

| Punto | Mecanismo | Detalle |
|---|---|---|
| Arranque de la API | **Polly** (`ResiliencePipeline`) | 10 reintentos con backoff exponencial (2 s → 15 s máx.) y timeout de 60 s por intento. Espera a que el contenedor de SQL Server esté listo |
| Consultas a la base de datos | **EF Core `EnableRetryOnFailure`** | 5 reintentos, 10 s de delay máximo, para fallos transitorios de SQL Server |
| Migraciones | Contexto **sin** estrategia de reintentos | Las migraciones son transaccionales; reintentarlas dentro de la misma transacción no aporta nada |

**No hay circuit breaker HTTP** porque la aplicación **no realiza llamadas HTTP salientes**:
no consume third parties, ni microservicios, ni webhooks. Un circuit breaker solo tendría sentido
delante de dependencias externas susceptibles de caer; aquí la única dependencia externa es SQL Server,
y para ella se aplican reintentos con backoff, que es la estrategia adecuada para fallos transitorios
de conexión. Si en el futuro se incorporan llamadas salientes, el sitio natural para el circuit breaker
sería el cliente HTTP de esas integraciones (patrón `ResiliencePipeline` + `AddCircuitBreaker` de Polly 8).

Comprobado: con la base de datos apagada, la API registra los reintentos y se recupera sola cuando
SQL Server vuelve a estar disponible.

---

## 9. Seguridad

| Medida | Implementación |
|---|---|
| Contraseñas | `PasswordHasher<Usuario>` (PBKDF2 + sal); nunca en texto plano ni en logs |
| Tokens | HS256, clave ≥ 32 caracteres desde configuración, `Issuer`/`Audience`/`exp` validados, `ClockSkew` de 30 s |
| Claims | `sub` (id), `email`, `role`, `jti`, `exp` |
| Autorización | `[Authorize]` en todos los endpoints salvo `/auth/login` y `/health`; `DELETE` restringido al rol `Admin` |
| Login | Mensaje genérico (no revela si el email existe) y **rate limiting** de 5 intentos/minuto por IP → 429 |
| Errores | ProblemDetails sin stack traces; el `traceId` permite correlacionar con el log |
| CORS | Solo el origen del frontend (`Cors:AllowedOrigins`) |
| Cabeceras | `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` y HSTS fuera de `Development` |
| Secretos | `.env` (ignorado por git) o variables de entorno / *user-secrets*; en el repositorio no hay ninguna clave real |
| Logs | Serilog con plantillas de mensaje; no se registran contraseñas, tokens ni hashes |

---

## 10. Colección Postman

[`postman/Pedidos.postman_collection.json`](postman/Pedidos.postman_collection.json) incluye:

- la petición de login con un script que guarda el token en la variable `token`,
- el CRUD de pedidos con `{{token}}` en la cabecera `Authorization`,
- ejemplos de casos de error (401, 403, 404, 409, 429).

Importa la colección en Postman y ejecuta primero **Pedidos → Login**; el resto de peticiones
heredan el token automáticamente.

---

## 11. Estructura del repositorio

```
.
├── AGENTS.md                    # reglas de trabajo del proyecto
├── docker-compose.yml           # SQL Server 2022 (healthcheck + volumen)
├── .env.example                 # plantilla de variables (sin secretos)
├── db/                          # 01-schema.sql y 02-seed.sql
├── docs/                        # requisitos, planificación y decisiones
├── postman/                     # colección Postman
├── src/
│   ├── Pedidos.Domain/          # entidades, enums, excepciones (sin dependencias)
│   ├── Pedidos.Application/     # casos de uso, DTOs, validadores, puertos
│   ├── Pedidos.Infrastructure/  # EF Core, repositorios, JWT, hashing, seed, Polly
│   └── Pedidos.Api/             # controllers, middleware, DI, Program.cs
├── tests/
│   ├── Pedidos.UnitTests/
│   └── Pedidos.IntegrationTests/
└── frontend/                    # SPA React (Vite + Tailwind)
```

---

## 12. Comandos habituales

```bash
# Backend
dotnet build
dotnet test
dotnet run --project src/Pedidos.Api                       # http://localhost:5080
dotnet tool restore                                          # restaura dotnet-ef (local tool)
dotnet dotnet-ef migrations add <Nombre> -p src/Pedidos.Infrastructure -s src/Pedidos.Api -o Persistence/Migrations

# Frontend
cd frontend && npm install && npm run dev                   # http://localhost:5173
cd frontend && npm run build && npm test

# Base de datos
docker compose up -d db
docker compose logs -f db
docker compose down -v
```

---

## 13. Entrega

- Historial con **Conventional Commits** y una rama por fase (`feature/f1-…` … `feature/f5-…`).
- Etiqueta final: `v1.0.0` sobre `main`.
- Sin secretos, sin `bin/`, `obj/`, `node_modules/` ni artefactos de build versionados.
