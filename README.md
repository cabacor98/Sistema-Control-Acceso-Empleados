# SGCAE — Sistema de Gestión y Control de Acceso de Empleados

Caso de estudio: API REST para el control de acceso físico de empleados en una organización, construida con **.NET 10**, **SQL Server (LocalDB)** y arquitectura **Clean Architecture + DDD + CQRS**.

## Alcance funcional

- **RF-01** Registrar, consultar, actualizar y desactivar empleados.
- **RF-02** Asignar credenciales de acceso (código alfanumérico único); bloquear, reactivar y revocar.
- **RF-03** Registrar solicitudes de acceso (entrada/salida) con reglas de negocio; los intentos rechazados quedan trazados con su motivo.
- **RF-04** Historial de movimientos con paginación y filtros.
- **RF-06** Administrar puntos de acceso: crear, listar, habilitar e inhabilitar.

## Arquitectura

```
Sistema-Control-Acceso-Empleados/
├── src/
│   ├── SGCAE.Domain/            # Dominio puro: entidades, value objects, reglas
│   ├── SGCAE.Application/       # Casos de uso CQRS (MediatR) y validación (FluentValidation)
│   ├── SGCAE.Infrastructure/    # EF Core 10 + SQL Server: repos, migraciones, seed
│   └── SGCAE.Api/               # Controllers REST, OpenAPI, Scalar, ProblemDetails
├── tests/
│   └── SGCAE.UnitTests/         # Pruebas unitarias de dominio y casos de uso (xUnit)
└── SistemaControlAccesoEmpleados.slnx
```

Las dependencias apuntan hacia adentro: `Domain` no referencia nada; `Application` solo referencia `Domain`; `Infrastructure` implementa repositorios y `IUnitOfWork` definidos por las capas internas; `Api` compone todo.

**DDD**: los agregados `Empleado`, `Credencial`, `PuntoAcceso` y `RegistroAcceso` encapsulan sus invariantes; los value objects `CorreoElectronico` (regex + normalización a minúsculas) y `CodigoCredencial` (6-20 alfanumérico + normalización a mayúsculas); el servicio de dominio `ValidadorDeAcceso` centraliza las reglas de autorización.

**CQRS**: comandos y consultas viajan por MediatR. Los comandos usan repositorios de escritura + `IUnitOfWork`; las consultas usan `ISgcaeReadRepository` (proyecciones no-tracking).

## Reglas de negocio implementadas

- Solo empleados con estado **Activo** pueden obtener credenciales y registrar accesos.
- Desactivar un empleado revoca sus credenciales activas.
- Una credencial pasa por `Activa → Bloqueada → Reactivada`; `Revocada` es definitivo.
- Solo credenciales **Activas** autorizan accesos; una credencial bloqueada genera rechazo trazado.
- Un punto inhabilitado rechaza cualquier solicitud.
- Una **salida** requiere una **entrada** previa abierta del mismo empleado.
- El correo y el código de credencial son únicos (índices `UX_Empleados_Email`, `UX_Credenciales_Codigo`) y se normalizan (minúsculas / mayúsculas).

Motivos de rechazo trazables en RF-03: `EmpleadoInactivo`, `CredencialNoRegistrada`, `CredencialNoActiva`, `PuntoAccesoNoHabilitado`, `SalidaSinEntradaPrevia`.

## Endpoints (RF ↔ API)

| RF | Método y ruta | Descripción |
|----|---------------|-------------|
| RF-01 | `POST /api/empleados` | Registrar empleado |
| RF-01 | `GET /api/empleados` | Listar con paginación, búsqueda y filtro por estado |
| RF-01 | `GET /api/empleados/{id}` | Consultar empleado |
| RF-01 | `PUT /api/empleados/{id}` | Actualizar datos |
| RF-01 | `DELETE /api/empleados/{id}` | Desactivar empleado (revoca credenciales activas) |
| RF-02 | `POST /api/credenciales` | Asignar credencial a empleado |
| RF-02 | `GET /api/credenciales?estado=` | Listar por estado |
| RF-02 | `POST /api/credenciales/{id}/bloquear` | Bloquear credencial |
| RF-02 | `POST /api/credenciales/{id}/reactivar` | Reactivar credencial bloqueada |
| RF-02 | `POST /api/credenciales/{id}/revocar` | Revocar credencial (definitivo) |
| RF-03 | `POST /api/accesos/solicitudes` | Registrar solicitud de entrada/salida |
| RF-04 | `GET /api/accesos` | Historial con paginación y filtros |
| RF-06 | `POST /api/puntos-acceso` | Crear punto de acceso |
| RF-06 | `GET /api/puntos-acceso` | Listar puntos de acceso |
| RF-06 | `POST /api/puntos-acceso/{id}/habilitar` | Habilitar punto |
| RF-06 | `POST /api/puntos-acceso/{id}/inhabilitar` | Inhabilitar punto |

Errores con ProblemDetails (RFC 9457): `DomainException` → 400, `ValidacionException` → 400 con errores por campo, `NotFoundException` → 404, `ConflictoException` → 409.

## Cómo ejecutar

Requisitos: .NET SDK 10 y SQL Server LocalDB (incluido con Visual Studio).

```powershell
# Restaurar y compilar
dotnet build SistemaControlAccesoEmpleados.slnx

# Crear/actualizar la base de datos en LocalDB
dotnet ef database update --project src/SGCAE.Infrastructure --startup-project src/SGCAE.Api

# Arrancar la API
dotnet run --project src/SGCAE.Api

# Ejecutar las pruebas
dotnet test
```

Al arrancar, la aplicación aplica migraciones y siembra datos de demostración si la base está vacía. Documentación interactiva en `/scalar`; OpenAPI en `/openapi/v1.json`.

## Evidencia de funcionamiento (curl)

Entrada autorizada con credencial activa `CRED1077` en Recepción:

```json
{ "resultado": "Autorizado", "motivoRechazo": null }
```

Salida tras la entrada:

```json
{ "resultado": "Autorizado", "motivoRechazo": null }
```

Rechazo trazado tras bloquear la credencial:

```json
{ "resultado": "Rechazado", "motivoRechazo": "CredencialNoActiva" }
```

Conflicto por correo duplicado (con normalización de mayúsculas):

```json
{ "title": "Conflicto con el estado actual", "status": 409,
  "detail": "Ya existe un empleado registrado con el correo 'M.ORTEGA@Logistica-Demo.COM'." }
```

Validación de entrada:

```json
{ "status": 400,
  "errors": { "Email": ["'Email' no es una dirección de correo electrónico válida."] } }
```

Recurso inexistente:

```json
{ "title": "Recurso no encontrado", "status": 404,
  "detail": "El empleado con identificador '…' no existe." }
```

Código duplicado de credencial (minúsculas se normalizan a mayúsculas):

```json
{ "status": 409,
  "detail": "El código de credencial 'CRED1077' ya está asignado." }
```

## Datos sembrados (demo)

- **María Fernanda Ortega Ruiz** — `CRED1077` activa (email `m.ortega@logistica-demo.com`).
- **Andrés Méndez** — empleado activo sin credencial.
- **Camilo Restrepo** — empleado inactivo con credencial `CRED2056` revocada.
- **Luis Córdova** — empleado con credencial `CRED3312`.

Puntos de acceso sembrados: Recepción, Parqueadero, Bodega y Área Restringida A (habilitados), y Almacén Temporal (inhabilitado), con IDs deterministas `0aa9c3d4-7006-4d61-9e30-1d3a5b171a01…a05`.

## Stack

- .NET 10 (ASP.NET Core + controllers)
- EF Core 10 / SQL Server (LocalDB)
- MediatR 12 (CQRS) + FluentValidation 12
- OpenAPI nativo de ASP.NET Core + Scalar (UI en `/scalar`)
- xUnit (53 pruebas unitarias)
