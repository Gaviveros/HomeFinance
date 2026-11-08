# Feature Specification: Persistencia en PostgreSQL

**Feature Branch**: `No creada`

**Created**: 2026-11-08

**Status**: Draft

**Input**: User description: Base de persistencia para conservar usuarios, autenticación y configuración de hogares, con migraciones reproducibles y validación de integridad. La cadena de conexión compartida en el mensaje se omite deliberadamente.

## Clarifications

### Session 2026-11-08

- Q: ¿La base de persistencia debe también mejorar la protección de contraseñas, sustituyendo el hash SHA-256 actual por un método adaptativo con sal, o debe conservar el método existente y limitarse a guardar sus hashes? → A: Mejorar el hash; mantener el flujo de autenticación existente.
- Q: Si ya hubiera cuentas con el hash anterior al desplegar el cambio, ¿cómo deberían seguir iniciando sesión sus titulares? → A: Aceptar el hash anterior al iniciar sesión y actualizarlo al nuevo método tras una autenticación correcta.
- Q: Al revisar el flujo actual encontré un límite importante: la API recibe `X-User-Id` del cliente y no valida una sesión o credencial autenticada; por tanto, un filtro de hogar basado solo en ese encabezado no garantiza aislamiento frente a suplantación. ¿Qué alcance debe tener el plan? → A: Incluir identidad autenticada y autorización de hogar en este alcance.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Conservar una cuenta entre sesiones (Priority: P1)

Como usuario registrado, quiero que mi cuenta permanezca disponible para poder iniciar sesión después de que la aplicación se reinicie.

**Why this priority**: La autenticación persistente es la base para que cualquier usuario pueda volver a utilizar HomeFinance.

**Independent Test**: Registrar una cuenta, reiniciar la aplicación y comprobar que el mismo usuario puede iniciar sesión sin volver a registrarse.

**Acceptance Scenarios**:

1. **Given** que el servicio está conectado a la base de datos configurada, **When** una persona se registra con un correo nuevo y una contraseña válida, **Then** se crea una cuenta persistente y la respuesta no expone la contraseña.
2. **Given** una cuenta registrada, **When** la aplicación se reinicia y su titular inicia sesión con sus credenciales válidas, **Then** se autentica la misma cuenta.
3. **Given** que ya existe una cuenta con ese correo, **When** se intenta registrar otra cuenta con el mismo correo, **Then** el sistema rechaza el duplicado y conserva la cuenta existente.
4. **Given** que la base de datos no está disponible, **When** se intenta registrar o autenticar una cuenta, **Then** el sistema no comunica un resultado exitoso que no haya podido confirmar.
5. **Given** una cuenta cuyo hash de contraseña usa el método anterior, **When** su titular inicia sesión correctamente, **Then** conserva el acceso y el sistema reemplaza el hash por uno generado con el nuevo método.

---

### User Story 2 - Conservar la configuración del hogar (Priority: P1)

Como propietario de un hogar, quiero conservar sus datos y configuración entre sesiones para continuar usando el mismo entorno.

**Why this priority**: Los hogares definen el contexto personal de la aplicación y dependen de la identidad persistente de su titular.

**Independent Test**: Registrar un usuario, crear su hogar, cambiar su configuración, reiniciar la aplicación y comprobar que el hogar, su titular y la configuración siguen disponibles.

**Acceptance Scenarios**:

1. **Given** un usuario existente, **When** crea un hogar, **Then** el hogar se conserva asociado a ese titular y mantiene sus valores iniciales de moneda y zona horaria.
2. **Given** un hogar existente, **When** se actualiza su configuración, **Then** los cambios permanecen disponibles después de reiniciar la aplicación.
3. **Given** un usuario que no es titular ni tiene pertenencia autorizada al hogar, o una persona que suplanta el identificador de otra, **When** intenta consultar o cambiar datos del hogar, **Then** el acceso se rechaza y no se divulga ni modifica la información.
4. **Given** una solicitud de creación de hogar cuyo titular no existe, **When** se procesa la solicitud, **Then** se rechaza y no queda un hogar sin titular.

---

### User Story 3 - Evolucionar y validar el almacenamiento con seguridad (Priority: P2)

Como desarrollador, quiero aplicar cambios de esquema de manera reproducible y probar la persistencia en un entorno aislado para desplegar cambios sin perder datos compatibles ni afectar datos reales.

**Why this priority**: La evolución verificable del esquema y las pruebas independientes reducen el riesgo operativo de las siguientes funcionalidades.

**Independent Test**: Aplicar los cambios versionados a una base de datos vacía y ejecutar las pruebas de registro, autenticación, hogares, persistencia tras reinicio y restricciones sin usar datos de producción.

**Acceptance Scenarios**:

1. **Given** una base de datos vacía, **When** se aplican las migraciones versionadas, **Then** queda preparado el esquema necesario para cuentas y hogares.
2. **Given** datos existentes, **When** se aplica un cambio compatible del esquema, **Then** los datos anteriores siguen disponibles.
3. **Given** un conjunto aislado de pruebas de integración, **When** se ejecuta sin acceso a los datos de producción, **Then** se validan la persistencia, la unicidad del correo, las relaciones y el aislamiento por hogar.
4. **Given** credenciales de conexión, **When** se prepara o ejecuta la aplicación, **Then** estas se obtienen de configuración externa y no quedan incluidas en el control de versiones.

### Edge Cases

- La base de datos no responde al iniciar el servicio o deja de estar disponible durante una operación.
- Dos solicitudes concurrentes intentan registrar el mismo correo.
- Se intenta asociar un hogar a un usuario inexistente o eliminar un usuario que todavía es titular de un hogar.
- Una consulta o modificación intenta cruzar el límite de propiedad de un hogar.
- Se aplica una migración a una base vacía, o a una base que ya contiene datos compatibles.
- Una prueba de integración se ejecuta sin credenciales de producción o con datos de pruebas previamente existentes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST usar PostgreSQL como almacenamiento persistente principal para las cuentas y los hogares actuales de HomeFinance.
- **FR-002**: El sistema MUST obtener los parámetros y credenciales de conexión desde configuración externa, y MUST NOT incluir credenciales reales en archivos versionados, registros de aplicación o respuestas al usuario.
- **FR-003**: El sistema MUST informar de forma segura cuando el almacenamiento no esté disponible y MUST NOT confirmar como exitosa una operación de escritura que no haya sido persistida.
- **FR-004**: El sistema MUST conservar los datos de cuenta requeridos por el flujo de registro y autenticación existente, incluido el correo y una representación de contraseña generada con un método adaptativo con sal; MUST NOT almacenar contraseñas en texto plano.
- **FR-005**: El sistema MUST impedir cuentas duplicadas por correo, incluso cuando solicitudes simultáneas intenten crear la misma cuenta.
- **FR-006**: El inicio de sesión existente MUST poder recuperar y autenticar una cuenta persistida después de reiniciar la aplicación, sin duplicar la lógica de autenticación; si la cuenta usa el formato de hash anterior, el sistema MUST actualizarlo al nuevo método después de validar correctamente la contraseña.
- **FR-007**: El sistema MUST conservar cada hogar con su nombre, titular, fecha de creación y configuración de moneda y zona horaria.
- **FR-008**: Todo hogar MUST tener un titular existente; las relaciones inválidas MUST rechazarse y no dejar registros huérfanos.
- **FR-009**: Las operaciones sobre hogares MUST respetar la titularidad o pertenencia autorizada existente y evitar que un usuario consulte o modifique datos de otro hogar.
- **FR-010**: Los cambios del esquema MUST estar versionados y poder aplicarse reproduciblemente a una instalación nueva; los cambios compatibles MUST conservar los datos existentes.
- **FR-011**: Las pruebas de integración MUST validar en PostgreSQL el registro, el inicio de sesión, la creación y configuración de hogares, la persistencia tras reinicio, la unicidad, las relaciones y el aislamiento por hogar.
- **FR-012**: Las pruebas de integración MUST ejecutarse con una base y datos aislados, sin depender de datos ni credenciales de producción.
- **FR-013**: La implementación MUST conservar los flujos actuales de acceso y gestión de hogares, reutilizando la lógica existente en lugar de implementar una segunda lógica de autenticación.
- **FR-014**: La API MUST establecer la identidad de la persona solicitante mediante un contexto de autenticación validado, MUST NOT tratar un identificador enviado por el cliente como prueba de identidad y MUST autorizar cada operación del hogar según la titularidad o pertenencia registrada.

### Key Entities *(include if feature involves data)*

- **Usuario**: cuenta con identificador, correo único, representación protegida de la contraseña y fecha de creación; puede ser titular de hogares.
- **Hogar**: entorno del usuario con identificador, nombre, titular obligatorio, fecha de creación, moneda y zona horaria; sus datos son accesibles solo conforme a las reglas de pertenencia autorizada.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de los casos de aceptación de registro e inicio de sesión permiten recuperar la misma cuenta tras reiniciar la aplicación.
- **SC-002**: El 100 % de los casos de aceptación de creación y actualización de hogares conservan titularidad, nombre, moneda y zona horaria tras reiniciar la aplicación.
- **SC-003**: El 100 % de los intentos de prueba de duplicar un correo o referenciar un titular inexistente son rechazados sin crear datos inconsistentes.
- **SC-004**: El 100 % de los intentos de prueba de acceso cruzado entre hogares se rechazan sin revelar ni alterar datos ajenos.
- **SC-005**: Las pruebas de integración pueden completarse sin leer ni modificar datos de producción.
- **SC-006**: Una instalación nueva puede prepararse aplicando los cambios versionados, y los cambios compatibles conservan todos los registros preexistentes cubiertos por las pruebas.
- **SC-007**: Una revisión de los archivos bajo control de versiones no encuentra credenciales reales de conexión.
- **SC-008**: Al menos 9 de cada 10 participantes de una prueba de aceptación pueden volver a iniciar sesión y encontrar su hogar sin volver a registrarse, reconfigurarlo ni recibir ayuda.

## Assumptions

- PostgreSQL es un requisito explícito de esta funcionalidad y de la constitución del proyecto; los detalles de infraestructura y del proveedor quedan fuera de esta especificación.
- El flujo actual de autenticación se reutiliza sin incorporar un mecanismo alternativo; el formato de almacenamiento de contraseñas se mejora y los hashes anteriores se actualizan gradualmente al iniciar sesión.
- El hogar mantiene un titular principal, conforme al modelo actual; no se introduce en este alcance una función nueva de invitaciones, membresías o permisos multiusuario.
- Los correos se comparan sin distinguir mayúsculas y minúsculas para evitar cuentas duplicadas por diferencias de capitalización.
- El almacenamiento actual está en memoria y no se presupone que existan cuentas previas que deban importarse; si se encuentran cuentas con el formato de hash anterior, su autenticación seguirá la transición gradual definida.
- Las pruebas de integración disponen de una base de datos de pruebas independiente o efímera y nunca usan credenciales ni datos de producción.
- Las credenciales compartidas en el texto de entrada no se copian a archivos del repositorio ni a esta especificación; deben administrarse por un canal externo seguro.

## Out of Scope

- Cuentas financieras, transacciones, transferencias, presupuestos, fondos personales, tarjetas de crédito, préstamos e informes financieros.
- Nuevos métodos de inicio de sesión, restablecimiento de contraseñas y funciones de colaboración o membresía entre hogares; la autenticación segura necesaria para validar la identidad y autorizar hogares sí forma parte de este alcance.
- Migración de datos que solo existían en memoria antes de incorporar almacenamiento persistente.
