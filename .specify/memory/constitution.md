<!--
Informe de impacto de sincronización
- Cambio de versión: 1.0.0 -> 1.1.0
- Principios modificados: constitución inicial -> Arquitectura modular monolítica; separación de dominios; arquitectura API-first; requisitos de calidad y seguridad; entrega guiada por especificación
- Secciones agregadas: Restricciones de arquitectura y tecnología; Gobernanza por hogar y seguridad; Flujo de calidad y entrega
- Secciones eliminadas: ninguna
- Items diferidos: ninguno
-->

# Constitución de HomeFinance

## Principios Fundamentales

### I. Arquitectura monolítica modular
HomeFinance DEBE implementarse como un monolito modular, no como una arquitectura de microservicios. El proyecto DEBE organizar el código en dominios de negocio y responsabilidades técnicas claras para que el sistema siga siendo simple de evolucionar y, al mismo tiempo, esté listo para entregas futuras a dispositivos móviles.

### II. Separación de responsabilidades
El sistema DEBE mantener una separación explícita entre las capas de Dominio, Aplicación, Infraestructura y API. La lógica de dominio DEBE expresar reglas de negocio e invariantes; los servicios de aplicación DEBEN orquestar casos de uso; la infraestructura DEBE manejar la persistencia y las integraciones externas; la API DEBE exponer contratos para los clientes. El código de presentación NO DEBE contener reglas del dominio ni transiciones de estado que pertenezcan al backend.

### III. Arquitectura backend y API-first
El backend DEBE ser una API web ASP.NET Core ejecutándose en .NET 10. Todas las interacciones del usuario, incluidos los clientes web y futuros clientes móviles, DEBEN consumir la API a través de contratos bien definidos en lugar de conectarse directamente a la base de datos o duplicar la lógica de negocio en el frontend.

### IV. Frontend y experiencia del cliente
El frontend DEBE implementarse con Angular y TypeScript como capa de interacción con el usuario. La interfaz DEBE centrarse en la presentación, la interacción y la experiencia del usuario, mientras que el backend permanece como fuente de verdad para validación, autorización y reglas financieras. La arquitectura DEBE permitir una futura integración móvil sin exigir una reescritura del modelo de negocio.

### V. Persistencia e integridad de datos
HomeFinance DEBE usar PostgreSQL como base de datos principal con Entity Framework Core para la persistencia. Los modelos de datos DEBEN preservar la corrección financiera y cada cálculo monetario DEBE usar precisión decimal/numeric en lugar de tipos de punto flotante. El esquema DEBE diseñarse sobre conceptos financieros reales y no DEBE tratar la conveniencia de la interfaz como sustituto de la integridad contable.

### VI. Seguridad y aislamiento por hogar
El sistema DEBE aislar los datos por hogar y aplicar autorización en cada ruta de acceso. Los miembros DEBEN acceder solo a la información del hogar para la cual tienen permiso, y la API DEBE imponer reglas que impidan filtraciones entre hogares, ediciones no autorizadas o divulgación de registros financieros privados.

### VII. Gestión financiera precisa
Las operaciones financieras DEBEN ser precisas, auditable y trazables. Cada ingreso, egreso, transferencia, asignación presupuestaria y corrección DEBE conservar su origen, hogar, fecha y contexto. El proyecto DEBE evitar ambigüedad en el tratamiento del dinero y DEBE mantener un historial claro para correcciones, reversos y conciliaciones.

### VIII. Pruebas y puertas de calidad
HomeFinance DEBE incluir pruebas automatizadas en las capas relevantes, incluyendo xUnit para la lógica del backend, pruebas de integración para flujos de negocio y Playwright para flujos visibles para el usuario. El comportamiento financiero crítico DEBE cubrirse con pruebas antes del lanzamiento, y ninguna regla financiera puede aceptarse sin verificar el comportamiento real.

### IX. Mantenibilidad y escalabilidad
El proyecto DEBE priorizar un código modular, legible, documentado y testeable por encima de atajos ingeniosos. Los componentes DEBEN organizarse para un mantenimiento a largo plazo, y la arquitectura DEBE soportar crecimiento futuro sin sacrificar claridad ni corrección. El sistema DEBE seguir siendo fácil de razonar a medida que crecen las funciones y los reportes.

### X. Desarrollo guiado por especificación
Cada funcionalidad y cambio importante DEBE empezar con una especificación antes de la implementación. El proyecto DEBE definir intención de usuario, reglas de negocio, criterios de aceptación y pasos de validación antes de escribir código. La especificación es una entrada obligatoria para la implementación, revisión y aprobación.

## Restricciones de arquitectura y tecnología

HomeFinance DEBE seguir una arquitectura de monolito modular con límites claros entre las capas de Dominio, Aplicación, Infraestructura y API. El backend DEBE construirse con ASP.NET Core Web API en .NET 10, mientras que el frontend DEBE usar Angular y TypeScript. La persistencia DEBE ser PostgreSQL con EF Core, y la aplicación DEBE preservar la separación entre lógica de negocio y lógica de presentación.

El proyecto DEBE usar patrones de integración API-first. El frontend y los clientes móviles futuros DEBEN consumir contratos de la API; ningún cliente DEBE evitar la API para escribir directamente en la base de datos o volver a implementar reglas de negocio. La arquitectura también DEBE seguir siendo compatible con una entrega móvil futura, incluso con patrones híbridos o PWA, sin cambiar el modelo central del dominio.

## Gobernanza por hogar y seguridad

Todo acceso a los datos DEBE estar acotado por hogar y validado mediante reglas de autorización. Un miembro del hogar SOLO DEBE ver y modificar registros para los que tiene permiso explícito. La API DEBE aplicar control de acceso y el modelo de dominio DEBE proteger relaciones que no deben cruzar los límites del hogar.

Los registros financieros DEBEN ser auditable y trazables. Los hechos históricos NO DEBEN sobrescribirse silenciosamente; las correcciones o reversos DEBEN dejar evidencia de la acción original y del motivo del cambio. El proyecto DEBE preservar la responsabilidad por decisiones que afectan saldos, reportes y presupuestos.

## Flujo de calidad y entrega

El proyecto DEBE seguir un flujo de trabajo guiado por especificación. Antes de la implementación, cada funcionalidad DEBE definir alcance, reglas relevantes del dominio, comportamiento esperado y pasos de validación. La especificación DEBE revisarse antes de escribir código y la implementación DEBE medirse según los criterios de aceptación acordados.

El equipo DEBE validar el código con pruebas automatizadas relevantes para el comportamiento cambiado. Las reglas del backend DEBEN cubrirse con xUnit y pruebas de integración. Los flujos visibles para el usuario DEBEN cubrirse con Playwright o pruebas end-to-end equivalentes. La revisión de código DEBE confirmar que la arquitectura, la seguridad, la integridad de los datos y la cobertura de pruebas sigan alineadas con la constitución.

## Gobernanza

Esta constitución es el estándar rector para el desarrollo de HomeFinance. Cualquier práctica que entre en conflicto con este documento queda invalidada hasta que la constitución se modifique.

Las enmiendas DEBEN incluir una justificación, la sección o principio afectado y cualquier impacto de migración o compatibilidad. La versión DEBE seguir el versionado semántico. Un cambio de versión MAYOR marca cambios incompatibles en la gobernanza o en los principios. Un cambio de versión MENOR marca principios o secciones nuevas o materialmente ampliadas. Un cambio de versión PATCH marca aclaraciones, correcciones de redacción o mejoras no semánticas.

El proyecto DEBE mantener la constitución aprobada en .specify/memory/constitution.md y NO DEBE modificar las fuentes subyacentes de la plantilla. La revisión de cumplimiento DEBE producirse antes del merge para cualquier cambio que afecte la arquitectura, la integridad de datos, los reportes, la seguridad o la lógica financiera.

**Versión**: 1.1.0 | **Ratificada**: 2026-11-06 | **Última modificación**: 2026-11-06
