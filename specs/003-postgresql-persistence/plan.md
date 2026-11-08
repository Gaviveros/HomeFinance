# Plan de implementación: Persistencia en PostgreSQL

**Branch**: `003-postgresql-persistence` (la rama Git activa sigue siendo `develop`) | **Date**: 2026-11-08 | **Spec**: [spec.md](./spec.md)

**Input**: Especificación funcional de `specs/003-postgresql-persistence/spec.md`

## Resumen

Reemplazar los repositorios en memoria por persistencia PostgreSQL en la capa de Infraestructura, conservando los flujos actuales de registro, inicio de sesión y hogares. El esquema versionado deberá proteger la unicidad del correo, la relación obligatoria entre cada hogar y su titular y la configuración del hogar. El alcance incluye fortalecer el hash de contraseñas con migración gradual al iniciar sesión y emitir un token JWT validado por el backend para autorizar rutas por titular. Repositorios y servicios que usan EF Core deben tener ciclo de vida por solicitud, y las operaciones de almacenamiento deben confirmar explícitamente los cambios. El cliente Angular usará el token en las operaciones de hogar; las pruebas deben usar una base PostgreSQL aislada.

## Contexto técnico

**Language/Version**: C# / .NET 10 para todos los proyectos backend, alineado con la constitución y la decisión del usuario. El repositorio actualmente apunta a .NET 9 y el entorno solo tiene instalado .NET SDK 9.0.306; instalar .NET 10 será un prerrequisito de implementación.

**Primary Dependencies**: ASP.NET Core Web API, Entity Framework Core 10, proveedor Npgsql compatible con EF Core 10, autenticación de tokens JWT de ASP.NET Core, `PasswordHasher<TUser>` mantenido por la plataforma, xUnit y Testcontainers para PostgreSQL.

**Storage**: PostgreSQL como almacenamiento persistente principal.

**Testing**: `dotnet test` sobre `HomeFinance.IntegrationTests`, con PostgreSQL efímero mediante Testcontainers para las pruebas de integración; las pruebas unitarias actuales continúan usando repositorios controlados en memoria cuando corresponda.

**Target Platform**: API web ASP.NET Core en .NET 10; clientes Angular actuales y clientes API futuros.

**Project Type**: Aplicación web monolítica modular; solución backend existente y frontend separado.

**Performance Goals**: La especificación no define objetivos numéricos de latencia, volumen ni disponibilidad. La validación se limita a completar correctamente los flujos funcionales aceptados.

**Constraints**: Mantener límites entre Dominio, Aplicación, Infraestructura y API; no almacenar secretos en el repositorio; proteger correos contra duplicados sin distinguir mayúsculas; derivar la identidad solo de un token JWT validado, nunca de `X-User-Id`; conservar hogares y titularidad; rechazar acceso no autenticado o entre hogares; actualizar hashes heredados tras autenticación válida; no guardar el token en almacenamiento persistente del navegador; no agregar funciones financieras ni colaboración entre hogares.

**Scale/Scope**: Persistencia del modelo actual de Usuario y Hogar; un titular obligatorio por hogar y cero o más hogares por usuario, sin una meta de volumen especificada.

## Revisión de la constitución

*GATE: Debe cumplirse antes de Phase 0 research y volver a verificarse tras Phase 1.*

- **Monolito modular y capas**: Cumple. Los cambios de persistencia pertenecen a `HomeFinance.Infrastructure`; las reglas y orquestación existentes permanecen en Dominio y Aplicación; API continúa siendo la frontera de acceso.
- **Ciclo de vida de persistencia**: Cumple si el contexto, los repositorios y servicios que lo consumen no se registran como singleton y cada escritura confirma su unidad de trabajo.
- **.NET 10 y API-first**: El plan se alinea con .NET 10. Los proyectos están actualmente en .NET 9 y el SDK 10 no está instalado; el usuario eligió actualizar los proyectos backend y tratar la instalación del SDK como prerrequisito, no como una excepción.
- **PostgreSQL y EF Core**: Cumple con PostgreSQL y EF Core como persistencia principal.
- **Aislamiento y seguridad**: El alcance actualizado exige validar una identidad autenticada y autorizar hogares por titular; un identificador enviado sin validar por el cliente no se acepta como identidad. El contrato usa tokens JWT para los clientes API.
- **Pruebas**: Incluye pruebas unitarias y de integración PostgreSQL para los flujos de persistencia, restricciones, transición de contraseñas y aislamiento.
- **Especificación previa**: Cumple; los requisitos y decisiones de seguridad se registraron antes del diseño.

**Resultado inicial**: Aprobado con prerrequisito operativo identificado (instalación del SDK .NET 10). No se propone una desviación de la constitución.

## Project Structure

### Documentación de esta funcionalidad

```text
specs/003-postgresql-persistence/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── api.yaml
└── checklists/
    └── requirements.md
```

### Código fuente (raíz del repositorio)

```text
HomeFinance.sln
backend/
├── src/
│   ├── HomeFinance.Api/
│   │   ├── Controllers/
│   │   ├── Program.cs
│   │   └── appsettings*.json
│   ├── HomeFinance.Application/
│   │   └── Services/
│   ├── HomeFinance.Domain/
│   │   ├── Entities/
│   │   └── Repositories/
│   └── HomeFinance.Infrastructure/
│       ├── Repositories/
│       ├── PersistenceDbContext.cs
│       ├── Configurations/
│       └── Migrations/
└── tests/
    └── HomeFinance.IntegrationTests/
frontend/
└── src/
    └── app/
        ├── app.ts
        └── app.config.ts
```

**Structure Decision**: Se conserva la solución existente y sus cuatro proyectos backend. No se agregan proyectos ni se accede a PostgreSQL desde el frontend. El contexto y las migraciones viven en Infraestructura; las pruebas PostgreSQL se agregan al proyecto de integración existente. El cliente Angular deja de enviar `X-User-Id` como prueba de identidad y usa el token devuelto por el inicio de sesión, conservándolo solo en memoria.

## Revisión de la constitución (post-diseño)

- Separación por capas y persistencia exclusivamente detrás de repositorios: cumple.
- Ciclo de vida por solicitud para contexto/repositorios/servicios y confirmación explícita de escritura: cumple.
- Esquema y migraciones bajo control de versiones; unicidad de correo y relación obligatoria con titular: cumple.
- Identidad autenticada y autorización por hogar en la API: cumple en el diseño; el contrato define autenticación mediante JWT y respuestas no divulgativas para accesos no autorizados.
- Compatibilidad de contraseña: el inicio de sesión existente verifica el formato heredado y lo reemplaza por el nuevo hash tras verificar la contraseña.
- Pruebas independientes de producción y sin secretos versionados: cumple.
- Compatibilidad .NET 10: prevista; la disponibilidad del SDK es una condición de ejecución, no una excepción de diseño.

**Resultado final**: Aprobado. No se detectan violaciones que requieran justificación ni complejidad adicional en la arquitectura.
