# Tasks: Acceso y configuración inicial del hogar

**Input**: Design documents from `/specs/002-household-access-setup/`

**Prerequisitos**: plan.md (obligatorio), spec.md (obligatorio para historias de usuario), research.md, data-model.md, contracts/

**Pruebas**: Las tareas de prueba se incluyen porque la especificación define escenarios de validación independientes para cada historia.

**Organización**: Las tareas están agrupadas por historia de usuario para permitir implementación y validación independiente de cada una.

## Formato: `[ID] [P?] [Story] Descripción`

- **[P]**: Puede ejecutarse en paralelo (archivos distintos, sin dependencias)
- **[Story]**: La historia de usuario a la que pertenece la tarea (por ejemplo, US1, US2, US3)
- Incluir rutas exactas de archivo en la descripción

## Fase 1: Configuración (infraestructura compartida)

**Propósito**: Inicialización del proyecto y estructura base

- [ ] T001 [P] Crear la estructura base del backend y frontend en backend/src, backend/tests, frontend/src y frontend/tests
- [ ] T002 [P] Inicializar la solución .NET 10 y el proyecto Angular para HomeFinance
- [ ] T003 [P] Configurar herramientas de análisis, formato y configuración de entorno en backend/.editorconfig, backend/Directory.Build.props y frontend/package.json

---

## Fase 2: Base fundacional (prerrequisitos bloqueantes)

**Propósito**: Infraestructura principal que DEBE completarse antes de cualquier historia de usuario

**⚠️ CRÍTICO**: ninguna historia puede comenzar antes de completar esta fase

- [ ] T004 Configurar PostgreSQL, EF Core y el DbContext base en backend/src/HomeFinance.Infrastructure/Persistence/HomeFinanceDbContext.cs
- [ ] T005 [P] Configurar la autenticación y autorización base en backend/src/HomeFinance.Api/Extensions/AuthenticationExtensions.cs
- [ ] T006 [P] Configurar middleware, manejo de errores y logging en backend/src/HomeFinance.Api/Middleware/ErrorHandlingMiddleware.cs
- [ ] T007 [P] Configurar gestión de variables de entorno y secretos en backend/src/HomeFinance.Api/appsettings.Development.json y backend/src/HomeFinance.Api/appsettings.json
- [ ] T008 Crear contratos y entidades base compartidas para Usuario, Hogar y Configuración en backend/src/HomeFinance.Domain/Users/User.cs y backend/src/HomeFinance.Domain/Households/Household.cs
- [ ] T009 [P] Preparar la infraestructura de pruebas con xUnit, pruebas de integración y Playwright para backend/tests y frontend/tests

**Punto de control**: la base está lista para que comiencen las historias de usuario en paralelo

---

## Fase 3: Historia de usuario 1 - Registro del usuario (Prioridad: P1) 🎯 MVP

**Objetivo**: permitir que un usuario nuevo cree una cuenta segura con correo y contraseña válidos.

**Prueba independiente**: un usuario nuevo puede registrarse correctamente y el sistema rechaza correos duplicados o información incompleta.

### Pruebas para la historia de usuario 1

- [ ] T010 [P] [US1] Crear prueba de integración para registro exitoso y rechazo de datos inválidos en backend/tests/HomeFinance.IntegrationTests/Auth/RegisterUserTests.cs
- [ ] T011 [P] [US1] Crear prueba de contrato para el endpoint de registro en backend/tests/HomeFinance.Api.Tests/Contracts/RegisterEndpointTests.cs

### Implementación para la historia de usuario 1

- [ ] T012 [P] [US1] Crear la entidad Usuario y la validación de correo y contraseña en backend/src/HomeFinance.Domain/Users/User.cs
- [ ] T013 [P] [US1] Implementar el repositorio y el hashing seguro de contraseñas en backend/src/HomeFinance.Infrastructure/Users/UserRepository.cs
- [ ] T014 [US1] Implementar el servicio de registro en backend/src/HomeFinance.Application/Users/RegisterUserCommand.cs
- [ ] T015 [US1] Implementar el endpoint POST /api/auth/register en backend/src/HomeFinance.Api/Controllers/AuthController.cs
- [ ] T016 [US1] Añadir validaciones, mensajes de error y respuestas HTTP correctas en backend/src/HomeFinance.Api/Models/Requests/RegisterUserRequest.cs
- [ ] T017 [US1] Validar el flujo de registro con una prueba de extremo a extremo en frontend/tests/e2e/auth/register.spec.ts

**Punto de control**: la historia de usuario 1 queda funcional y validable de forma independiente

---

## Fase 4: Historia de usuario 2 - Autenticación del usuario (Prioridad: P1)

**Objetivo**: permitir iniciar y cerrar sesión de forma segura para acceder al entorno protegido.

**Prueba independiente**: un usuario registrado puede autenticarse con credenciales válidas, falla con credenciales inválidas y su sesión se cierra correctamente.

### Pruebas para la historia de usuario 2

- [ ] T018 [P] [US2] Crear prueba de integración para login exitoso y fallido en backend/tests/HomeFinance.IntegrationTests/Auth/LoginUserTests.cs
- [ ] T019 [P] [US2] Crear prueba de contrato para el endpoint de inicio y cierre de sesión en backend/tests/HomeFinance.Api.Tests/Contracts/AuthSessionTests.cs

### Implementación para la historia de usuario 2

- [ ] T020 [P] [US2] Crear la entidad de sesión y el modelo de token en backend/src/HomeFinance.Domain/Sessions/UserSession.cs
- [ ] T021 [P] [US2] Implementar el servicio de autenticación y validación de credenciales en backend/src/HomeFinance.Application/Auth/AuthenticateUserCommand.cs
- [ ] T022 [US2] Implementar generación y persistencia de sesión / token en backend/src/HomeFinance.Infrastructure/Auth/JwtTokenService.cs
- [ ] T023 [US2] Implementar los endpoints POST /api/auth/login y POST /api/auth/logout en backend/src/HomeFinance.Api/Controllers/AuthController.cs
- [ ] T024 [US2] Aplicar control de acceso por sesión y protección de rutas en backend/src/HomeFinance.Api/Extensions/AuthorizationExtensions.cs
- [ ] T025 [US2] Validar la denegación de acceso a rutas protegidas con usuarios no autenticados en backend/tests/HomeFinance.IntegrationTests/Auth/ProtectedRoutesTests.cs

**Punto de control**: las historias de usuario 1 y 2 funcionan de manera independiente

---

## Fase 5: Historia de usuario 3 - Creación del hogar (Prioridad: P1)

**Objetivo**: permitir a un usuario autenticado crear un hogar con titular válido y asociado al creador.

**Prueba independiente**: un usuario autenticado puede crear un hogar de forma exitosa y el sistema rechaza la operación cuando no existe titular válido.

### Pruebas para la historia de usuario 3

- [ ] T026 [P] [US3] Crear prueba de integración para creación de hogar y rechazo sin titular en backend/tests/HomeFinance.IntegrationTests/Households/CreateHouseholdTests.cs
- [ ] T027 [P] [US3] Crear prueba de contrato para el endpoint de creación del hogar en backend/tests/HomeFinance.Api.Tests/Contracts/HouseholdEndpointTests.cs

### Implementación para la historia de usuario 3

- [ ] T028 [P] [US3] Crear la entidad Hogar y la validación del titular en backend/src/HomeFinance.Domain/Households/Household.cs
- [ ] T029 [P] [US3] Implementar el repositorio de hogares y persistencia en backend/src/HomeFinance.Infrastructure/Households/HouseholdRepository.cs
- [ ] T030 [US3] Implementar el caso de uso CreateHousehold en backend/src/HomeFinance.Application/Households/CreateHouseholdCommand.cs
- [ ] T031 [US3] Implementar el endpoint POST /api/households en backend/src/HomeFinance.Api/Controllers/HouseholdsController.cs
- [ ] T032 [US3] Validar la asociación del usuario creador como titular del hogar y registrar errores de negocio en backend/src/HomeFinance.Application/Households/HouseholdValidationService.cs
- [ ] T033 [US3] Añadir validación visual de creación del hogar en frontend/src/app/features/household/create-household.component.ts

**Punto de control**: la creación del hogar queda funcional y validable sin depender de la configuración avanzada del hogar

---

## Fase 6: Historia de usuario 5 - Aislamiento de datos del hogar (Prioridad: P1)

**Objetivo**: garantizar que cada usuario solo pueda acceder al hogar del que forma parte y rechazar intentos de acceso cruzado.

**Prueba independiente**: un usuario puede consultar su hogar y el sistema rechaza accesos a hogares ajenos.

### Pruebas para la historia de usuario 5

- [ ] T034 [P] [US5] Crear prueba de integración para aislamiento por hogar en backend/tests/HomeFinance.IntegrationTests/Households/HouseholdIsolationTests.cs
- [ ] T035 [P] [US5] Crear prueba de contrato para acceso autorizado y no autorizado en backend/tests/HomeFinance.Api.Tests/Contracts/HouseholdAccessTests.cs

### Implementación para la historia de usuario 5

- [ ] T036 [P] [US5] Crear la política de pertenencia y validación de acceso en backend/src/HomeFinance.Application/Households/HouseholdAccessService.cs
- [ ] T037 [US5] Implementar la verificación de titularidad en backend/src/HomeFinance.Domain/Households/HouseholdMembership.cs
- [ ] T038 [US5] Añadir comprobaciones de pertenencia antes de cada operación sensible en backend/src/HomeFinance.Api/Middleware/HouseholdAccessFilter.cs
- [ ] T039 [US5] Responder con errores de autorización y sin fugas de datos en backend/src/HomeFinance.Api/Responses/UnauthorizedHouseholdResponse.cs
- [ ] T040 [US5] Añadir pruebas end-to-end de acceso no autorizado en frontend/tests/e2e/household/household-access.spec.ts
- [ ] T041 [US5] Validar trazabilidad de errores y logging de accesos rechazados en backend/src/HomeFinance.Infrastructure/Logging/HouseholdAccessLogger.cs

**Punto de control**: el aislamiento por hogar queda asegurado antes de la configuración avanzada del entorno

---

## Fase 7: Historia de usuario 4 - Configuración inicial del hogar (Prioridad: P2)

**Objetivo**: configurar el nombre, la moneda y la zona horaria del hogar con valores por defecto correctos.

**Prueba independiente**: el administrador del hogar puede guardar valores básicos y el sistema usa ARS y America/Argentina/Cordoba cuando no están definidos.

### Pruebas para la historia de usuario 4

- [ ] T042 [P] [US4] Crear prueba de integración para configuración inicial del hogar en backend/tests/HomeFinance.IntegrationTests/Households/HouseholdSettingsTests.cs
- [ ] T043 [P] [US4] Crear prueba de contrato para GET/PUT de configuración en backend/tests/HomeFinance.Api.Tests/Contracts/HouseholdSettingsEndpointTests.cs

### Implementación para la historia de usuario 4

- [ ] T044 [P] [US4] Crear el modelo de configuración del hogar con validación de moneda y zona horaria en backend/src/HomeFinance.Domain/Households/HouseholdSettings.cs
- [ ] T045 [US4] Implementar el servicio de actualización de configuración en backend/src/HomeFinance.Application/Households/UpdateHouseholdSettingsCommand.cs
- [ ] T046 [US4] Implementar el endpoint GET/PUT /api/households/{id}/settings en backend/src/HomeFinance.Api/Controllers/HouseholdsController.cs
- [ ] T047 [US4] Aplicar los valores predeterminados ARS y America/Argentina/Cordoba al crear o actualizar el hogar en backend/src/HomeFinance.Application/Households/HouseholdDefaultsService.cs
- [ ] T048 [US4] Añadir formulario y flujo de UI en frontend/src/app/features/household/household-settings.component.ts y frontend/src/app/features/household/household-settings.component.html
- [ ] T049 [US4] Validar que la configuración se guarda sin perder la asociación del titular ni la privacidad del hogar en backend/tests/HomeFinance.IntegrationTests/Households/HouseholdSettingsPersistenceTests.cs

**Punto de control**: la configuración inicial del hogar queda validada y operativa

---

## Fase 8: Pulido y preocupaciones transversales

**Propósito**: mejoras que afectan a varias historias y validación global del feature

- [ ] T050 [P] Revisar y actualizar la documentación operativa en docs/ y la guía de arranque en specs/002-household-access-setup/quickstart.md
- [ ] T051 [P] Ejecutar validación cruzada del flujo completo de onboarding en frontend/tests/e2e/onboarding/household-onboarding.spec.ts
- [ ] T052 Revisar seguridad, autorizaciones y trazabilidad global para usuarios, hogares y sesiones en backend/src/HomeFinance.Api/Startup.cs
- [ ] T053 [P] Ejecutar set de pruebas de regresión para precisión financiera en backend/tests/HomeFinance.IntegrationTests/Finance/DecimalPrecisionTests.cs
- [ ] T054 Validar que todo el feature cumpla con el checklist de requisitos y la constitución en specs/002-household-access-setup/checklists/requirements.md

---

## Dependencias y orden de ejecución

### Dependencias de fases

- **Fase 1**: sin dependencias, puede comenzar inmediatamente
- **Fase 2**: depende de la Fase 1; bloquea todas las historias de usuario
- **Fase 3+**: dependen de la Fase 2 y se pueden ejecutar en paralelo si hay capacidad de equipo
- **Fase 8**: depende de que todas las historias estén completas

### Dependencias por historia

- **US1 (Registro)**: puede comenzar después de la Fase 2; no depende de otras historias
- **US2 (Autenticación)**: puede comenzar después de la Fase 2 y usa la base de identidad creada en US1
- **US3 (Creación del hogar)**: puede comenzar después de la Fase 2 y requiere autenticación del usuario
- **US5 (Aislamiento por hogar)**: puede comenzar después de la Fase 2 y depende de la pertenencia al hogar
- **US4 (Configuración inicial)**: puede comenzar después de la Fase 2 y depende del hogar ya creado

### Oportunidades de paralelismo

- Todas las tareas marcadas con [P] de las Fases 1 y 2 pueden ejecutarse en paralelo
- Las pruebas de cada historia marcadas con [P] pueden ejecutarse en paralelo
- Los modelos de dominio de cada historia pueden desarrollarse en paralelo si no comparten archivos
- Las historias de usuario pueden trabajarse por equipos distintos una vez que la base fundacional termina

---

## Ejemplo de paralelismo: historia de usuario 1

```bash
# Lanzar pruebas para User Story 1 al mismo tiempo:
Task: "Crear prueba de integración para registro exitoso y rechazo de datos inválidos en backend/tests/HomeFinance.IntegrationTests/Auth/RegisterUserTests.cs"
Task: "Crear prueba de contrato para el endpoint de registro en backend/tests/HomeFinance.Api.Tests/Contracts/RegisterEndpointTests.cs"

# Lanzar modelos de la historia de usuario 1 al mismo tiempo:
Task: "Crear la entidad Usuario y la validación de correo y contraseña en backend/src/HomeFinance.Domain/Users/User.cs"
Task: "Implementar el repositorio y el hashing seguro de contraseñas en backend/src/HomeFinance.Infrastructure/Users/UserRepository.cs"
```

---

## Estrategia de implementación

### MVP primero (solo User Story 1)

1. Completar la Fase 1: Configuración
2. Completar la Fase 2: Base fundacional
3. Completar la Fase 3: US1
4. **DETENERSE y VALIDAR**: probar US1 de forma independiente
5. Preparar demo si corresponde

### Entrega incremental

1. Completar Configuración + Base fundacional
2. Añadir US1 → validar de forma independiente → demo
3. Añadir US2 → validar de forma independiente → demo
4. Añadir US3 → validar de forma independiente → demo
5. Añadir US5 → validar de forma independiente → demo
6. Añadir US4 → validar de forma independiente → demo
7. Ejecutar pulido y validación final

### Estrategia de equipo en paralelo

1. El equipo completa Configuración + Base fundacional conjuntamente
2. Una vez finalizada la base:
   - Desarrollador A: US1
   - Desarrollador B: US2
   - Desarrollador C: US3
   - Desarrollador D: US5/US4
3. Las historias se validan y se integran independientemente

---

## Notas

- Las tareas marcadas con [P] usan archivos o componentes distintos y no tienen dependencias directas
- La etiqueta [Story] permite trazabilidad por historia de usuario
- Cada historia debe poder completarse y probarse de forma independiente
- Las pruebas deben escribirse antes de la implementación cuando el flujo lo requiera
- Se recomienda validación por checkpoints después de cada historia completa
- Evitar tareas vagas, conflictos de archivos y dependencias entre historias que rompan la independencia
