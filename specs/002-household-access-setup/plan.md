# Plan de implementación: Acceso y configuración inicial del hogar

**Branch**: `[002-household-access-setup]` | **Fecha**: 2026-11-08 | **Spec**: [spec.md](./spec.md)

**Entrada**: Especificación funcional de la feature en `/specs/002-household-access-setup/spec.md`

## Resumen

La funcionalidad cubre el primer ciclo de acceso y configuración del hogar en HomeFinance: registro de usuario, autenticación segura, creación del hogar, configuración inicial y aislamiento de datos por hogar. En esta iteración se define un modelo de titular único: cada hogar tiene un propietario principal y el sistema valida pertenencia antes de autorizar operaciones internas. El enfoque técnico se basa en una API ASP.NET Core en .NET 10, PostgreSQL como almacén principal y Angular como cliente. La lógica financiera y de autorización debe mantenerse en backend, dejando al frontend únicamente la experiencia de usuario.

## Contexto técnico

**Idioma/Versión**: C# en .NET 10, TypeScript en Angular, SQL para PostgreSQL

**Dependencias principales**: ASP.NET Core Web API, Entity Framework Core, PostgreSQL, Angular, xUnit, Playwright

**Persistencia**: PostgreSQL con EF Core y tipos numéricos/decimal para dinero

**Pruebas**: xUnit para backend, pruebas de integración para flujos de negocio y Playwright para validación end-to-end

**Plataforma objetivo**: Web moderna con API backend y cliente Angular; compatible con evolución a clientes móviles futuros

**Tipo de proyecto**: Aplicación web full-stack con arquitectura modular monolítica

**Objetivos de rendimiento**: respuestas de API rápidas en escenarios cotidianos del hogar, validación segura de acceso y escritura consistente en información financiera

**Restricciones**: aislamiento de datos por hogar, precisión decimal de finanzas, autenticación obligatoria, trazabilidad de cambios y autorización por rol o titularidad

**Escala/alcance**: flujo de onboarding de usuario y hogar, foco en un hogar por usuario principal en la versión inicial

## Revisión de la constitución

*Puerta de entrada: debe cumplirse antes del diseño técnico. Se vuelve a verificar al finalizar la etapa de diseño.*

### Cumplimiento verificado

- Arquitectura modular monolítica: sí, con separación de dominio, aplicación, infraestructura y API.
- API-first: sí, el frontend solo consumirá contratos del backend.
- Aislamiento por hogar: sí, se exige control de acceso y validación de pertenencia a cada hogar.
- Integridad financiera: sí, todos los valores monetarios usarán precisión decimal/numeric.
- Seguridad: sí, se requiere autenticación, autorización y rechazo de accesos no autorizados.
- Calidad: sí, se incluirán pruebas automatizadas y validación de reglas financieras.
- Especificación previa: sí, esta funcionalidad ya fue definida antes de la implementación.

### Resultado

La feature cumple la constitución sin desviaciones. No se requiere justificación de complejidad adicional ni excepciones.

## Estructura del proyecto

### Documentación de la feature

```text
specs/002-household-access-setup/
├── spec.md              # Especificación funcional actual
├── plan.md              # Documento de planificación
├── research.md          # Hallazgos de investigación y decisiones
├── data-model.md        # Modelo de dominio y reglas relevantes
├── quickstart.md        # Guía de arranque para validación
├── checklists/
│   ├── requirements.md
│   └── tasks.md        # Si se genera posteriormente
└── contracts/           # Contratos API y ejemplos de uso
```

### Código fuente (raíz del repositorio)

```text
backend/
├── src/
│   ├── HomeFinance.Api/
│   ├── HomeFinance.Application/
│   ├── HomeFinance.Domain/
│   ├── HomeFinance.Infrastructure/
│   └── HomeFinance.Shared/
├── tests/
│   ├── HomeFinance.Application.Tests/
│   ├── HomeFinance.Integration.Tests/
│   └── HomeFinance.Api.Tests/
└── HomeFinance.sln

frontend/
├── src/
│   ├── app/
│   ├── features/
│   ├── shared/
│   └── core/
└── tests/
    └── e2e/
```

**Decisión de estructura**: se utilizará un monolito modular con una solución backend y un proyecto frontend Angular separados, manteniendo la lógica financiera y de autorización en el servidor. La solución y los contratos se diseñarán para soportar futuras ampliaciones de miembros del hogar sin romper la estructura base.

## Fase de diseño propuesta

1. Validar el modelo de identidad y registro de usuarios.
2. Definir el aggregate de hogar y su titular único.
3. Establecer el contrato de autenticación y la sesión del usuario.
4. Definir la configuración inicial del hogar con valores por defecto ARS y America/Argentina/Cordoba.
5. Diseñar reglas de acceso y permisos por hogar.
6. Especificar pruebas de integración para los escenarios críticos.

## Seguimiento de complejidad

No se detectan violaciones de la constitución que requieran justificación adicional.

| Violación | Motivo | Alternativa simple rechazada |
|-----------|--------|------------------------------|
| Ninguna | El alcance está alineado con la constitución y la arquitectura aceptada | No aplica |
