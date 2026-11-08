# Modelo de datos y reglas de dominio

## Entidades principales

### Usuario

**Responsabilidad**: representa la identidad personal que inicia sesión y accede al sistema.

**Atributos clave**:
- Id
- Email
- PasswordHash
- Fecha de creación
- Estado activo/inactivo

**Reglas**:
- El email debe ser único.
- La contraseña debe almacenarse solo como hash.
- El usuario debe autenticarse antes de acceder a un hogar.

### Hogar

**Responsabilidad**: define el entorno financiero independiente del usuario.

**Atributos clave**:
- Id
- Nombre
- TitularId
- Fecha de creación
- Estado activo

**Reglas**:
- Debe existir un titular válido.
- Un hogar pertenece a un único propietario principal en la iteración actual.
- El nombre debe ser válido y obligatorio.

### Configuración del hogar

**Responsabilidad**: almacena los valores base del entorno financiero.

**Atributos clave**:
- HogarId
- Moneda
- ZonaHoraria
- NombreVisible
- FechaActualizacion

**Valores por defecto**:
- Moneda: ARS
- Zona horaria: America/Argentina/Cordoba

**Reglas**:
- La configuración debe estar asociada únicamente a un hogar.
- Los cambios de configuración deben ser auditable.

### Sesión de usuario

**Responsabilidad**: representa el estado autenticado del usuario dentro de la aplicación.

**Atributos clave**:
- Id
- UsuarioId
- Token o identificador de sesión
- Fecha de emisión
- Fecha de expiración

**Reglas**:
- Debe invalidarse al cerrar sesión.
- Debe rechazarse si la sesión ya no es válida.

## Relaciones

- Un usuario puede crear uno o varios hogares en el futuro, pero el diseño actual prioriza un hogar principal por titular.
- Cada hogar tiene una configuración propia.
- Cada hogar tiene un titular obligatorio y un estado claro de pertenencia.

## Invariantes de negocio

1. No puede existir un hogar sin titular válido.
2. La configuración de un hogar no puede quedar sin moneda o zona horaria relevante.
3. Un usuario no autenticado no puede acceder a recursos protegidos.
4. Un usuario solo puede operar sobre hogares a los que pertenece.
5. Todo valor financiero se almacena con precisión decimal.

## Decisiones de diseño

- Se mantiene la estructura mínima necesaria para el flujo principal.
- Se evita la implementación de permisos complejos multiusuario en esta fase.
- El dominio permanecerá preparado para una evolución futura pero sin introducir complejidad innecesaria.
