# Investigación y decisiones: Persistencia en PostgreSQL

## Decisiones

### Persistencia y límites de capas

**Decisión**: usar Entity Framework Core con el proveedor Npgsql, ubicando contexto, mapeos, repositorios y migraciones en `HomeFinance.Infrastructure`. Mantener las interfaces de repositorio del dominio y las responsabilidades existentes de Aplicación y API.

**Rationale**: coincide con la constitución, satisface el almacenamiento PostgreSQL real y evita que los clientes o las capas de negocio dependan directamente del proveedor. El contexto de EF Core representa una unidad de trabajo y no es seguro para uso concurrente; su ciclo de vida debe ser acotado a la solicitud y no singleton.

**Alternativas consideradas**: acceso directo a la base desde los controladores (rechazado por separación de capas); crear un nuevo proyecto de persistencia (innecesario para la solución existente).

Fuentes: [EF Core: proveedores](https://learn.microsoft.com/en-us/ef/core/providers/), [Proveedor EF Core de Npgsql](https://www.npgsql.org/efcore/index.html), [configuración y ciclo de vida de DbContext](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/).

### Correo único y relaciones

**Decisión**: normalizar el correo de forma determinista (recortar espacios y comparar sin distinguir mayúsculas), buscar usando la misma forma normalizada y respaldar la unicidad con una restricción/índice único en PostgreSQL. Mantener el correo presentado para visualización. El hogar conserva un único titular obligatorio mediante una clave foránea no nula a usuario; la eliminación de un usuario titular se restringe mientras existan sus hogares.

**Rationale**: la validación previa da un error de dominio comprensible, pero solo una restricción única en la base resuelve correctamente registros simultáneos. La clave foránea evita titulares huérfanos. El modelo actual no expone transferencia ni borrado de cuentas.

**Alternativas consideradas**: validación exclusiva en Application (no evita carreras); `citext` o índice sobre expresión `lower(email)` (posibles implementaciones, a evaluar frente a una propiedad normalizada persistida; evitar duplicar políticas de normalización).

Fuentes: [Índices por expresión de PostgreSQL](https://www.postgresql.org/docs/current/indexes-expressional.html), [tipo citext](https://www.postgresql.org/docs/current/citext.html), [restricciones de PostgreSQL](https://www.postgresql.org/docs/current/ddl-constraints.html).

### Autenticación, autorización y protección de contraseñas

**Decisión**: reutilizar el servicio de autenticación existente para verificar credenciales y emitir un token JWT de duración limitada, validado por el backend. Las rutas de hogar obtienen el identificador de usuario de la identidad validada y comprueban titularidad antes de leer o modificar datos; `X-User-Id` y el `UserId` del cuerpo no autentican. Usar un hasher adaptativo con sal mantenido por la plataforma; al verificar una contraseña válida con el hash SHA-256 anterior, reemplazarlo por el hash nuevo.

**Rationale**: el cliente Angular y futuros clientes API no deben poder elegir la identidad solicitante. Un token JWT facilita que clientes distintos de un navegador usen la misma API; la clave de firma se guarda fuera del repositorio. Reutilizar la verificación en el mismo flujo de autenticación evita una segunda lógica de credenciales. La conversión al iniciar sesión mantiene el acceso sin guardar contraseñas en texto claro.

**Alternativas consideradas**: confiar en el encabezado enviado por el cliente (rechazado: permite suplantar usuarios); cookie de navegador solamente (menos adecuada para el objetivo API-first con clientes móviles); exigir restablecimiento de todas las contraseñas existentes (interrupción de acceso); seguir usando SHA-256 sin sal (protección insuficiente).

Fuente: [`PasswordHasher<TUser>` de ASP.NET Core Identity](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.passwordhasher-1?view=aspnetcore-10.0).

### Unidad de trabajo y persistencia de cambios

**Decisión**: hacer explícita la persistencia de modificaciones de configuración y usar operaciones de repositorio asíncronas que compartan el contexto por solicitud. Asegurar que un error de escritura no se presente como éxito.

**Rationale**: los servicios actuales mutan objetos en memoria y algunas interfaces no ofrecen actualización explícita. Una base relacional solo conserva cambios confirmados; EF Core requiere confirmar la unidad de trabajo.

**Alternativas consideradas**: confiar en que los cambios en objetos se persistan automáticamente sin confirmar (incorrecto); crear un repositorio genérico adicional (no necesario).

Fuente: [unidad de trabajo y ciclo de vida de DbContext](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/).

### Migraciones y despliegue

**Decisión**: versionar migraciones, probar aplicación desde cero y actualización compatible con datos existentes; revisar los cambios del esquema y aplicar migraciones como paso de despliegue controlado, no automáticamente en cada instancia de la API. Separar permisos/secretos de ejecución y despliegue.

**Rationale**: mejora reproducibilidad y evita que varias réplicas compitan por modificar el esquema o que la aplicación de ejecución tenga permisos administrativos innecesarios.

**Alternativas consideradas**: migrar automáticamente en el arranque en todos los entornos (rechazado como práctica de despliegue); cambios SQL manuales no versionados (no reproducibles).

Fuentes: [migraciones EF Core](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/), [aplicación de migraciones en producción](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying).

### Pruebas y configuración

**Decisión**: mantener pruebas unitarias ligeras, y comprobar persistencia, restricciones, migraciones y autorización con PostgreSQL real en una base desechable por ejecución. Obtener la cadena de conexión y la clave de firma de configuración externa, nunca de archivos versionados; no usar secretos de producción en pruebas.

**Rationale**: dobles y proveedores alternativos no reproducen todas las reglas de PostgreSQL. El entorno de pruebas debe ser repetible e independiente de los datos reales.

**Alternativas consideradas**: EF InMemory o SQLite como sustituto en pruebas de integración (rechazado por no verificar semántica real de PostgreSQL); usar una base o credenciales de producción (prohibido); base compartida persistente para pruebas (riesgo de contaminación entre ejecuciones).

Fuentes: [estrategias de pruebas EF Core](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy), [Testcontainers para PostgreSQL](https://dotnet.testcontainers.org/modules/postgres/), [pruebas de integración ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0), [secretos de ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0), [configuración ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0).

## Alineación de plataforma

**Decisión**: apuntar todos los proyectos backend a .NET 10, según la constitución y la decisión del usuario. La máquina de planificación solo tiene .NET SDK 9.0.306; instalar .NET 10 es requisito para implementar y ejecutar la solución.

**Rationale**: conservar .NET 9 dejaría la feature en conflicto con el estándar vigente del proyecto.

**Alternativas consideradas**: excepción temporal de .NET 9 (rechazada por el usuario); instalar SDK 10 durante esta fase de planificación (no necesario para generar documentos).

## Cuestiones cerradas por el alcance

- El correo se considera único sin distinguir mayúsculas, coherente con el repositorio actual y los supuestos de la especificación.
- Solo existe el vínculo de titularidad `Usuario 1:N Hogar`; no se agrega entidad de miembros. El acceso es exclusivo para el titular autenticado.
- La eliminación de un usuario titular se restringe si conserva hogares; transferir titularidad no forma parte del alcance.
- No se especifica volumen o SLO de rendimiento, por lo que no se inventan metas cuantitativas de operación.
