# Investigación y decisiones: acceso y configuración inicial del hogar

## 1. Modelo de acceso inicial

### Decisión

La iteración inicial implementará un hogar con un único titular. El usuario autenticado que crea el hogar quedará como propietario principal y administrador del mismo. Esta restricción se aplica explícitamente por alcance del feature y evita introducir permisos complejos antes de tener la base de seguridad validada.

### Justificación

- La constitución exige aislamiento estricto por hogar.
- La lógica de permisos multiusuario puede introducir riesgo si se implementa sin un modelo de membresía bien definido.
- La función de “configuración inicial del hogar” debe ser operativa desde el primer hogar válido.

### Resultado esperado

- Un usuario puede registrarse y autenticarse.
- Un usuario autenticado crea un hogar propio.
- El hogar queda asociado a un titular único.
- El sistema valida que el titular sea válido antes de crear la entidad.

## 2. Autenticación y sesión

### Decisión

La autenticación se realizará mediante correo y contraseña con sesión protegida en la API. El backend verificará credenciales, emitirá un token o cookie seguro y validará la sesión para cada request protegida.

### Reglas clave

- El registro exige correo único e información obligatoria válida.
- Las credenciales inválidas deben rechazar acceso.
- Las rutas protegidas deben requerir autenticación y validación adicional de pertenencia.

## 3. Configuración inicial del hogar

### Decisión

Toda nueva creación o actualización del hogar debe usar valores por defecto establecidos en la configuración del hogar:

- Moneda: ARS
- Zona horaria: America/Argentina/Cordoba
- Nombre: valor obligatorio, proporcionado por el usuario

### Reglas clave

- El nombre del hogar debe validarse al crear y editar.
- La moneda y la zona horaria deben ser persistidas en el contexto del hogar.
- Los cambios en la configuración deben aplicarse de forma consistente al entorno del hogar.

## 4. Aislamiento de datos

### Decisión

La API debe aplicar controles de acceso por hogar para toda consulta y modificación de registros. No se permite acceso a datos de un hogar ajeno ni siquiera por errores o rutas no protegidas.

### Reglas de validación

- Cada consulta debe verificar la pertenencia del usuario al hogar solicitado.
- Cualquier operación sensible exige titularidad o permiso explícito.
- Los errores deben devolver respuestas de autorización con trazabilidad y no leak de datos.

## 5. Pruebas de aceptación

Los escenarios clave a automatizar son:

- registro exitoso y rechazo de duplicados;
- login correcto e incorrecto;
- creación de hogar con titular válido;
- rechazo de hogar sin titular;
- acceso autorizado al hogar propio;
- acceso rechazado a un hogar ajeno;
- actualización correcta de configuración inicial.

## 6. Riesgos y mitigaciones

### Riesgo: sobrecarga funcional

La base de permisos múltiples podría requerir decisiones de diseño antes de estar listo. Se mitiga con la decisión de mantener un titular único en esta versión.

### Riesgo: fuga de datos

El manejo de filtros y consultas por hogar es crítico. Se mitiga con validaciones a nivel API y pruebas de integración por hogar.

### Riesgo: valores financieros incorrectos

La configuración del hogar determina el contexto monetario. Se mitiga con valores por defecto y validación de precisión decimal.
