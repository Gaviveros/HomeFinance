# Guía rápida de validación: Persistencia en PostgreSQL

## Prerrequisitos

- Instalar .NET SDK 10 y Docker Desktop con el daemon activo para ejecutar las pruebas PostgreSQL de Testcontainers.
- Configurar `ConnectionStrings__HomeFinance` y `Authentication__SigningKey` por un canal externo seguro para ejecutar la API. La clave debe tener al menos 32 bytes.
- Para herramientas de migración, configurar `HOMEFINANCE_CONNECTION_STRING`. No usar credenciales de producción en pruebas ni agregarlas a `appsettings*.json`.
- Ejecutar los comandos desde la raíz del repositorio.

## Aplicar migraciones

```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
$env:HOMEFINANCE_CONNECTION_STRING = '<cadena de conexión aprobada>'
.\backend\scripts\apply-migrations.ps1
```

El script aplica solo migraciones versionadas y no se ejecuta automáticamente al iniciar la API. La variable debe apuntar a la base prevista y aprobada.

## Compilación y pruebas

```powershell
dotnet restore .\HomeFinance.sln
dotnet build .\HomeFinance.sln --no-restore
dotnet test .\backend\tests\HomeFinance.IntegrationTests\HomeFinance.IntegrationTests.csproj
```

Las pruebas usan Testcontainers y crean una base efímera independiente de la configuración de producción. Si Docker no está disponible, las pruebas PostgreSQL no pueden ejecutarse.

## Escenarios de validación

1. **Migración limpia**: iniciar una base vacía, aplicar las migraciones versionadas y confirmar que se crean las estructuras de usuario y hogar.
2. **Registro, reinicio e inicio de sesión**: registrar una cuenta, cerrar y reiniciar la API, autenticar la misma cuenta y comprobar que la contraseña no está almacenada en claro.
3. **Correo duplicado**: registrar un correo y volver a intentar con distinta capitalización o espacios periféricos; la segunda cuenta debe rechazarse, también ante intentos concurrentes.
4. **Transición de hash**: la prueba unitaria crea una cuenta con un hash SHA-256 heredado, inicia sesión con la contraseña válida y verifica que el hash queda actualizado; una contraseña incorrecta no debe cambiarlo.
5. **Hogar**: autenticar al titular, crear y modificar un hogar, reiniciar la API y verificar que titularidad, nombre, moneda y zona horaria permanecen.
6. **Autorización**: intentar consultar y modificar un hogar sin credencial, con una credencial de otro usuario y enviando un `X-User-Id` suplantado; todos los intentos deben rechazarse sin filtrar ni cambiar información.
7. **Integridad referencial**: intentar crear un hogar con titular inexistente y confirmar que la clave foránea lo rechaza; la relación usa borrado restringido.
8. **Reaplicación segura**: volver a ejecutar la migración en el entorno de integración y confirmar que los datos ya creados permanecen.
9. **Base no disponible**: ejecutar una operación con una conexión de prueba no enrutable y confirmar que la API devuelve `503` sin exponer secretos ni detalles internos.

## Resultado esperado

Todos los escenarios deben pasar con PostgreSQL real y datos efímeros. Repetir la suite no debe depender del orden de ejecución ni de datos de una ejecución anterior.

## Referencias

- [Modelo de datos](./data-model.md)
- [Contrato de API](./contracts/api.yaml)
- [Investigación y decisiones](./research.md)
