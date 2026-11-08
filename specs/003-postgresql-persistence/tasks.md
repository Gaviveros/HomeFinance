# Tareas: Persistencia en PostgreSQL

**Entrada**: Documentos de diseño en `/specs/003-postgresql-persistence/`

**Prerrequisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md` y `contracts/api.yaml`.

**Pruebas**: Incluye pruebas unitarias y de integración porque la especificación exige validar explícitamente la persistencia, las migraciones, las restricciones y el aislamiento usando PostgreSQL.

**Organización**: Las tareas están agrupadas por historia para entregar y validar incrementos coherentes.

## Formato

- `[P]`: puede ejecutarse en paralelo con otras tareas independientes de archivos distintos.
- `[US1]`, `[US2]`, `[US3]`: historia de usuario correspondiente.
- Las tareas de configuración y prerrequisitos no tienen etiqueta de historia.
- Cada tarea identifica los archivos que debe cambiar o crear.

## Fase 1: Preparación

**Propósito**: preparar el SDK, las dependencias y las pruebas PostgreSQL aisladas.

- [ ] T001 Instalar .NET SDK 10 y cambiar el framework de destino a `net10.0` en `backend/src/HomeFinance.Api/HomeFinance.Api.csproj`, `backend/src/HomeFinance.Application/HomeFinance.Application.csproj`, `backend/src/HomeFinance.Domain/HomeFinance.Domain.csproj`, `backend/src/HomeFinance.Infrastructure/HomeFinance.Infrastructure.csproj` y `backend/tests/HomeFinance.IntegrationTests/HomeFinance.IntegrationTests.csproj`.
- [ ] T002 Agregar versiones compatibles de EF Core 10, Npgsql EF Core y herramientas de diseño de EF Core en `backend/src/HomeFinance.Infrastructure/HomeFinance.Infrastructure.csproj`; agregar la integración de autenticación JWT en `backend/src/HomeFinance.Api/HomeFinance.Api.csproj` y Testcontainers para PostgreSQL en `backend/tests/HomeFinance.IntegrationTests/HomeFinance.IntegrationTests.csproj`.
- [ ] T003 [P] Configurar nombres de opciones externas para cadena de conexión y clave de firma, sin valores secretos, en `backend/src/HomeFinance.Api/appsettings.json` y `backend/src/HomeFinance.Api/appsettings.Development.json`; revisar el `.gitignore` raíz para excluir archivos de secretos locales.
- [ ] T004 [P] Crear el fixture de PostgreSQL desechable y su ciclo de vida por ejecución en `backend/tests/HomeFinance.IntegrationTests/PostgresFixture.cs`, sin leer configuración de producción.

---

## Fase 2: Fundamentos compartidos

**Propósito**: preparar el modelo relacional inicial y la conexión que requieren las tres historias.

- [ ] T005 Crear el contexto de persistencia y sus colecciones de `User` y `Household` en `backend/src/HomeFinance.Infrastructure/PersistenceDbContext.cs`.
- [ ] T006 [P] Agregar `NormalizedEmail` a `User` en `backend/src/HomeFinance.Domain/Entities/User.cs` y mapear la entidad en `backend/src/HomeFinance.Infrastructure/Configurations/UserConfiguration.cs`: `Id` como clave primaria UUID; `Email` obligatorio; `NormalizedEmail` obligatorio y único, derivado del correo sin espacios periféricos y en minúsculas; `PasswordHash` obligatorio y con capacidad suficiente para las versiones del hasher; `CreatedAt` como instante UTC.
- [ ] T007 [P] Mapear `Household` en `backend/src/HomeFinance.Infrastructure/Configurations/HouseholdConfiguration.cs`: `Id` como clave primaria UUID; `Name` obligatorio y no vacío después de quitar espacios periféricos; `OwnerId` obligatorio con FK a `User.Id` y borrado restringido; `Currency` obligatorio con valor inicial `ARS`; `TimeZone` obligatorio con valor inicial `America/Argentina/Cordoba`; `CreatedAt` como instante UTC; incluir índice por titular.
- [ ] T008 Registrar el contexto PostgreSQL y el manejo de errores de conexión en `backend/src/HomeFinance.Api/Program.cs`, con opciones enlazadas desde configuración externa y ciclo de vida por solicitud.
- [ ] T009 Crear la migración inicial que materializa las tablas de usuario y hogar, la unicidad del correo normalizado y la FK obligatoria del titular en `backend/src/HomeFinance.Infrastructure/Migrations/`.
- [ ] T010 Implementar el middleware o filtro central de errores de almacenamiento en `backend/src/HomeFinance.Api/Program.cs` y `backend/src/HomeFinance.Api/Middleware/ApiExceptionHandler.cs`, de modo que errores de conexión no se registren con secretos ni produzcan respuestas de éxito.

**Punto de control**: el esquema puede construirse en una base vacía, la conexión es externa y las pruebas disponen de PostgreSQL aislado.

---

## Fase 3: Historia 1 — Conservar cuentas e iniciar sesión (Prioridad: P1) — MVP

**Objetivo**: registrar y autenticar usuarios persistidos, con correo único y hashes de contraseña adaptativos.

**Prueba independiente**: registrar una cuenta, cerrar y volver a crear el host de API contra la misma base, autenticar la cuenta, rechazar un correo duplicado incluso con otra capitalización y confirmar que las respuestas y la base no contienen la contraseña en claro.

### Pruebas de la historia 1

- [ ] T011 [P] [US1] Agregar pruebas de integración de registro, correo duplicado con mayúsculas/minúsculas, reinicio del host e inicio de sesión en `backend/tests/HomeFinance.IntegrationTests/AuthPersistenceTests.cs`.
- [ ] T012 [P] [US1] Agregar pruebas unitarias para el hash adaptativo y la verificación/actualización del hash SHA-256 heredado después de un login válido en `backend/tests/HomeFinance.IntegrationTests/PasswordHashMigrationTests.cs`.

### Implementación de la historia 1

- [ ] T013 [US1] Crear el normalizador común de correo en `backend/src/HomeFinance.Application/Services/EmailNormalizer.cs` para recortar espacios periféricos y convertir a minúsculas, y usarlo al crear o buscar cuentas.
- [ ] T014 [US1] Agregar `Update(User user)` a `IUserRepository` en `backend/src/HomeFinance.Domain/Repositories/IUserRepository.cs` e implementar `Add`, `GetByEmail`, `GetById` y `Update` en `backend/src/HomeFinance.Infrastructure/Repositories/PostgresUserRepository.cs`, usando `NormalizedEmail` para búsquedas y guardando cambios del hash.
- [ ] T015 [US1] Actualizar `AuthService` en `backend/src/HomeFinance.Application/Services/AuthService.cs` para usar un hasher adaptativo con sal; durante el login aceptar el SHA-256 heredado solo si la contraseña coincide y persistir inmediatamente su reemplazo por el nuevo hash.
- [ ] T016 [P] [US1] Agregar el servicio de emisión de tokens JWT con identificador de usuario, expiración y clave obtenida de configuración externa en `backend/src/HomeFinance.Api/Authentication/AccessTokenService.cs`.
- [ ] T017 [US1] Actualizar `AuthController` en `backend/src/HomeFinance.Api/Controllers/AuthController.cs` para mantener los contratos de registro/login de `specs/003-postgresql-persistence/contracts/api.yaml`, devolver el token solo al autenticar correctamente y traducir correo duplicado y falta de almacenamiento a respuestas explícitas sin detalles internos.
- [ ] T018 [US1] Registrar repositorio y servicios de autenticación con ciclo de vida por solicitud, configurar validación de tokens y política de errores en `backend/src/HomeFinance.Api/Program.cs`.
- [ ] T019 [US1] Actualizar la respuesta tipada del login y conservar el token solo en memoria de la aplicación Angular en `frontend/src/app/app.ts`.

**Punto de control**: el registro y login sobreviven a la recreación del host y los hashes antiguos se actualizan sin interrumpir el acceso.

---

## Fase 4: Historia 2 — Conservar y proteger hogares (Prioridad: P1)

**Objetivo**: persistir hogares y configuración, y restringir cada lectura o modificación al titular autenticado.

**Prueba independiente**: con usuarios autenticados distintos, crear y cambiar un hogar, reiniciar el host, verificar su titular y valores persistidos, y rechazar la suplantación o el acceso cruzado.

### Pruebas de la historia 2

- [ ] T020 [P] [US2] Agregar pruebas de integración para crear hogar con moneda `ARS` y zona `America/Argentina/Cordoba`, actualizar y recuperar configuración tras recrear el host en `backend/tests/HomeFinance.IntegrationTests/HouseholdPersistenceTests.cs`.
- [ ] T021 [P] [US2] Agregar pruebas de autorización para credencial ausente, credencial de otro usuario y encabezado `X-User-Id` suplantado en `backend/tests/HomeFinance.IntegrationTests/HouseholdAuthorizationTests.cs`.

### Implementación de la historia 2

- [ ] T022 [US2] Agregar `Update(Household household)` a `IHouseholdRepository` en `backend/src/HomeFinance.Domain/Repositories/IHouseholdRepository.cs` e implementar `Add`, `GetById`, `GetForUser` y `Update` en `backend/src/HomeFinance.Infrastructure/Repositories/PostgresHouseholdRepository.cs`, confirmando las modificaciones de configuración.
- [ ] T023 [US2] Actualizar `HouseholdService` en `backend/src/HomeFinance.Application/Services/HouseholdService.cs` para persistir la actualización antes de responder éxito y limitar operaciones al `OwnerId` autenticado.
- [ ] T024 [US2] Proteger `HouseholdsController` en `backend/src/HomeFinance.Api/Controllers/HouseholdsController.cs` con autenticación y autorización por titular; quitar `X-User-Id` y `UserId` del cuerpo como fuentes de identidad y evitar revelar hogares ajenos.
- [ ] T025 [US2] Registrar el repositorio PostgreSQL de hogares y servicios con ciclo de vida por solicitud en `backend/src/HomeFinance.Api/Program.cs`.
- [ ] T026 [US2] Actualizar `frontend/src/app/app.ts`, `frontend/src/app/app.config.ts` y `frontend/src/app/app.html` para enviar el token de autenticación en operaciones de hogar, dejar de enviar `X-User-Id` o `userId` en el cuerpo como identidad y no persistir el token en almacenamiento del navegador.

**Punto de control**: titularidad y configuración sobreviven al reinicio; una identidad inválida o ajena no puede consultar ni modificar el hogar.

---

## Fase 5: Historia 3 — Versionar y validar el esquema (Prioridad: P2)

**Objetivo**: demostrar que el esquema puede instalarse y evolucionar con integridad en PostgreSQL sin depender de datos de producción.

**Prueba independiente**: crear una base PostgreSQL efímera, aplicar todas las migraciones desde cero, comprobar unicidad y FK, ejecutar los flujos persistentes y verificar el comportamiento ante datos preexistentes y fallos de conexión.

### Pruebas de la historia 3

- [ ] T027 [P] [US3] Agregar pruebas PostgreSQL para aplicación de migraciones desde base vacía, preservación de registros en una actualización compatible, restricción única concurrente de correo y FK de titular en `backend/tests/HomeFinance.IntegrationTests/SchemaMigrationTests.cs`.
- [ ] T028 [P] [US3] Agregar pruebas de integración que verifican que el host no informa éxito ante conexión caída y que las pruebas no leen ni modifican datos de producción en `backend/tests/HomeFinance.IntegrationTests/DatabaseFailureTests.cs`.
- [ ] T029 [P] [US3] Agregar pruebas del límite entre hogares que validan listado por titular y rechazo de lectura/modificación cruzada en `backend/tests/HomeFinance.IntegrationTests/HouseholdIsolationTests.cs`.

### Implementación de la historia 3

- [ ] T030 [US3] Agregar una vía reproducible para generar y aplicar migraciones revisadas fuera del arranque de la API en `backend/scripts/apply-migrations.ps1`, usando configuración externa y sin incluir secretos.
- [ ] T031 [US3] Documentar en `specs/003-postgresql-persistence/quickstart.md` los comandos y resultados esperados para PostgreSQL efímero, migración limpia, actualización compatible, restricciones, conexión fallida y aislamiento de producción.
- [ ] T032 [US3] Actualizar `specs/003-postgresql-persistence/contracts/api.yaml` para que las respuestas de error y autorización coincidan con el comportamiento final implementado en registro, login y hogares.

**Punto de control**: las migraciones son reproducibles y todas las verificaciones de integridad y persistencia se ejecutan sin datos reales.

---

## Fase 6: Ajustes finales y validación transversal

**Propósito**: comprobar los criterios de aceptación integrados sin introducir funcionalidades fuera de alcance.

- [ ] T033 [P] Revisar `backend/src/HomeFinance.Api/appsettings.json`, `backend/src/HomeFinance.Api/appsettings.Development.json` y los archivos de configuración de pruebas para confirmar que no contienen credenciales reales ni claves de firma.
- [ ] T034 Ejecutar `dotnet restore .\HomeFinance.sln`, `dotnet build .\HomeFinance.sln --no-restore` y `dotnet test .\HomeFinance.sln --no-build` según `specs/003-postgresql-persistence/quickstart.md`, y corregir cualquier regresión en los archivos de backend y pruebas afectados.
- [ ] T035 Revisar `backend/src/HomeFinance.Api/Controllers/AuthController.cs`, `backend/src/HomeFinance.Api/Controllers/HouseholdsController.cs` y `frontend/src/app/app.ts` para confirmar que no se usa un identificador de usuario enviado por el cliente como prueba de autenticación.

---

## Dependencias y orden de ejecución

### Dependencias por fase

- **Preparación (Fase 1)**: inmediata; la instalación de .NET 10 y dependencias es prerrequisito para compilar.
- **Fundamentos (Fase 2)**: depende de la preparación y bloquea las historias.
- **Historia 1 (Fase 3)**: depende de los fundamentos.
- **Historia 2 (Fase 4)**: depende de los fundamentos y de la identidad/token de US1 para probar hogares con titular autenticado; puede iniciar sus pruebas/modelos cuando exista el contexto compartido.
- **Historia 3 (Fase 5)**: depende de las migraciones y de los flujos de persistencia de US1 y US2 que valida.
- **Ajustes finales (Fase 6)**: depende de las historias que se decida entregar.

### Grafo de historias

```text
Preparación
    ↓
Fundamentos compartidos
    ├──→ US1: Cuenta y autenticación ──→ US2: Hogar y autorización ──→ US3: Migraciones y validación integral
    └─────────────────────────────────────────────────────────────────→ US3
```

### Oportunidades de paralelismo

- En Preparación: T003 puede ejecutarse en paralelo con T001 y T002; T004 depende de la referencia a Testcontainers de T002.
- En Fundamentos: T006 y T007 configuran entidades distintas y pueden paralelizarse tras crear T005.
- En US1: T011 y T012 son pruebas en archivos distintos; T013 prepara la normalización para T014 y T015; T016 puede implementarse en paralelo con la persistencia del usuario una vez fijada la configuración.
- En US2: T020 y T021 son suites separadas; T022 puede desarrollarse en paralelo con esas pruebas. T024 y T026 requieren el contrato de identidad acordado por US1.
- En US3: T027, T028 y T029 son archivos de prueba independientes una vez esté disponible el entorno de integración.

## Ejemplo de ejecución paralela: US1

```text
T011: Pruebas de integración de registro, unicidad y login tras reinicio en AuthPersistenceTests.cs
T012: Pruebas unitarias de hashes y transición SHA-256 en PasswordHashMigrationTests.cs
```

## Ejemplo de ejecución paralela: US2

```text
T020: Pruebas de persistencia y configuración del hogar en HouseholdPersistenceTests.cs
T021: Pruebas de autorización y suplantación en HouseholdAuthorizationTests.cs
```

## Estrategia de implementación

### MVP

1. Completar Preparación y Fundamentos.
2. Completar US1: alta persistente, login después de reinicio, email único y hash seguro.
3. Validar US1 de forma independiente con PostgreSQL real.
4. Continuar con US2 para ofrecer hogares seguros y persistentes; US1 solo no completa el objetivo principal del producto.

### Entrega incremental

1. Preparación y esquema compartido inicial.
2. US1 permite alta e inicio de sesión persistentes.
3. US2 añade hogares, configuración persistente y autorización por titular.
4. US3 acredita migraciones, integridad, pruebas aisladas y manejo de fallos.
5. Ejecutar validación transversal y revisar que los secretos estén ausentes.
