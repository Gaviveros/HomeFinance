# Quickstart para validación del feature

## Objetivo

Validar que el flujo de acceso y configuración inicial del hogar funciona de acuerdo con la especificación y las reglas de la constitución.

## Requisitos previos

- .NET 10 SDK
- Node.js y Angular CLI
- PostgreSQL disponible para desarrollo
- Base de datos de prueba configurada

## 1. Preparación del backend

```bash
dotnet restore

dotnet build
```

Luego, ejecutar la API desde la solución backend y validar la autenticación, creación del hogar y configuración del mismo.

## 2. Preparación del frontend

```bash
npm install
npm run start
```

Validar los flujos de:
- registro de cuenta;
- inicio de sesión;
- creación de hogar;
- configuración inicial del hogar;
- acceso restringido a hogares ajenos.

## 3. Casos de prueba críticos

- Registro con email duplicado debe fallar.
- Login con credenciales inválidas debe rechazar acceso.
- Crear hogar sin titular debe fallar.
- Crear hogar con usuario autenticado debe completarse exitosamente.
- Configurar moneda y zona horaria predeterminadas debe guardar ARS y America/Argentina/Cordoba.
- Intentar consultar un hogar ajeno debe devolver error de autorización.

## 4. Verificación de cumplimiento

La feature estará validada cuando:

- el usuario puede registrarse y autenticarse;
- el hogar se crea con titular válido;
- el hogar usa los valores por defecto definidos;
- la información del hogar queda aislada por pertenencia;
- el sistema rechaza accesos no autorizados.

## 5. Siguientes pasos

- Generar contratos de API para registro, login y hogar.
- Preparar tareas técnicas de backend y frontend.
- Implementar pruebas de integración para todos los criterios de aceptación.
