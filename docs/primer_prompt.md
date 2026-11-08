Te recomendaría construirla como un proyecto de software propiamente dicho, no simplemente como una aplicación que reemplaza tu Excel.

Hay una decisión arquitectónica que considero fundamental: separar completamente la lógica financiera de la interfaz visual. Esto te permitirá empezar con una aplicación web sencilla y, cuando llegue el momento, desarrollar una aplicación Android/iOS sin tener que rehacer las reglas de negocio.

Además, aprovecharía tu experiencia con .NET, Angular, SQL y Spec Kit, sin incorporar tecnologías innecesarias.

# 1. Arquitectura propuesta



Mi recomendación

&#x20;Arquitectura modular, API-first y preparada para mobile.



No utilizaría microservicios. Para una aplicación de economía hogareña sería complejidad innecesaria.

Utilizaría un patrón de Modular Monolith.

Capa de presentación

## Angular

Web responsive · PWA · Futura integración con Capacitor

HTTPS · REST · JSON

Capa de aplicación

## ASP.NET Core Web API

Autenticación · Validaciones · Reglas de negocio · Reportes

Domain

Entidades y reglas

Application

Casos de uso

Infrastructure

Persistencia

Capa de datos

## PostgreSQL

Base de datos online · Neon

### Tecnologías específicas

| Componente           | Tecnología                         |
| -------------------- | ---------------------------------- |
| IDE                  | VS Code                            |
| Frontend             | Angular + TypeScript               |
| Backend              | ASP.NET Core Web API               |
| Framework            | .NET 10 LTS                        |
| ORM                  | Entity Framework Core 10           |
| Provider             | Npgsql                             |
| Base de datos        | PostgreSQL                         |
| API                  | REST / JSON                        |
| Autenticación        | ASP.NET Core Identity + Bearer/JWT |
| Control de versiones | Git + GitHub                       |
| Pruebas E2E          | Playwright                         |
| Pruebas backend      | xUnit                              |

Elegiría .NET 10 LTS porque tiene soporte hasta noviembre de 2028. Además, Npgsql ya dispone de su proveedor compatible con EF Core 10.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://dotnet.microsoft.com\&sz=32)

.NET+1



No necesitás Visual Studio. Podés trabajar con todo desde VS Code, incluyendo terminal, compilación, migraciones, depuración y pruebas.

### Organización del repositorio

Te propongo una estructura como esta:

```
HomeFinance/
│
├── .github/
│   ├── agents/
│   └── prompts/
│
├── .specify/
│   └── memory/
│       └── constitution.md
│
├── specs/
│   ├── 001-foundation/
│   ├── 002-accounts/
│   ├── 003-transactions/
│   ├── 004-dashboard/
│   └── 005-excel-import/
│
├── src/
│   ├── backend/
│   │   ├── HomeFinance.Api/
│   │   ├── HomeFinance.Application/
│   │   ├── HomeFinance.Domain/
│   │   └── HomeFinance.Infrastructure/
│   │
│   └── frontend/
│       └── home-finance-web/
│
├── tests/
│   ├── HomeFinance.UnitTests/
│   ├── HomeFinance.IntegrationTests/
│   └── e2e/
│
├── docs/
│
└── README.md
```

La ventaja es que las especificaciones funcionales y el código viven en el mismo repositorio.

# 2. Base de datos: ¿cuál conviene?

Para tu caso evaluaría principalmente dos alternativas.

[GitHub - neondatabase/serverless: Connect to Neon PostgreSQL from serverless/worker/edge functions · GitHub](https://images.openai.com/static-rsc-4/OPYqAGJD2rvXt69XMtu30Tm-6ks8EFbOgNRVPoeAycGk_ZfXT2RLGK8BtCiahiRhMyk9aaupwcUF1u5J2fuoEbBYCvOlPHFxJNzXlegthAr0Pqjp4LZUiWFYnhoXEkT9bTq38XONByOg6stBUQMoif-ISWmwwf9AtL57sC0ITm8?purpose=inline)

Neon

Mi primera elección

$0

Es PostgreSQL administrado en la nube, sin necesidad de mantener un servidor propio.

Su plan gratuito ofrece:

- 1 GB de almacenamiento por proyecto.
- 100 CU-hours de computación mensuales por proyecto.
- Escalado a cero luego de 5 minutos de inactividad.
- Hasta 10 ramas por proyecto.

Es una opción muy interesante cuando querés utilizar PostgreSQL como base de datos y mantener tu propio backend .NET.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://github.com\&sz=32)

GitHub+1



Web oficial: [Neon](https://neon.com/?utm_source=chatgpt.com)

[Supabase Logo PNG Vector (SVG) Free Download](https://images.openai.com/static-rsc-4/vnOvGyXHAU4o0j-4PXwxPvLIZjRrtzChuGVRYnY1UFV4OP5yOfEdei2vq5WgSjbaR4ESySWN4YhvcT0XjOGBxELuT-GjYDCUO-pnsh4ohdY9e-Jy1NvlIhyS1ZDjXqyaU08LG2AhuaVIFv_cGUEYZ7_brKishT6ZIP6d4p5BWHk?purpose=inline)

Supabase

Alternativa integrada

$0

También trabaja con PostgreSQL, pero incorpora otros servicios:

- Base de datos PostgreSQL de 500 MB.
- Autenticación de usuarios.
- Almacenamiento de archivos.
- API y servicios adicionales.

Su principal ventaja es que te resuelve varias piezas de infraestructura. Su desventaja es que los proyectos gratuitos con poca actividad se pausan después de una semana.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://supabase.com\&sz=32)

Supabase+1



Web oficial: [Supabase](https://supabase.com/?utm_source=chatgpt.com)

### Mi decisión

Elegiría Neon + ASP.NET Core + PostgreSQL.

¿Por qué?

1. No dependés de un proveedor específico para la lógica de negocio.
2. PostgreSQL es una base de datos estándar que podés migrar.
3. Toda la gestión de datos y reglas queda en tu API.
4. Podés administrar autenticación y permisos desde .NET.
5. No necesitás incorporar servicios adicionales al principio.

Importante: el frontend nunca debería conectarse directamente a PostgreSQL. Toda operación debe pasar por la API.

También mantendría exportaciones periódicas de respaldo. No confiaría la única copia de tus movimientos financieros a un plan gratuito.

# 3. Modelo de datos: cómo empezaría

Acá está una de las decisiones más importantes del proyecto.

No empezaría creando tablas a partir de las columnas de tu Excel. Primero identificaría los conceptos financieros y sus relaciones.

Mi propuesta inicial sería:

Estructura conceptual

Household

Economía del hogar

HouseholdMember

Usuarios

Account

Cuentas y billeteras

Category

Categorías

Movement

Ingresos y gastos

Transfer

Transferencias

Budget

Presupuestos

Esquema conceptual inicial, no el diagrama definitivo de entidades.

### Entidades principales

| Entidad         | Responsabilidad                                   |
| --------------- | ------------------------------------------------- |
| Household       | Representa la economía familiar                   |
| HouseholdMember | Personas que participan                           |
| Account         | Banco, efectivo, billetera virtual, tarjeta       |
| Category        | Alimentación, servicios, transporte, sueldo, etc. |
| Movement        | Registro de ingresos y gastos                     |
| Transfer        | Movimiento entre dos cuentas                      |
| Budget          | Presupuesto mensual por categoría                 |
| RecurringRule   | Reglas para movimientos recurrentes               |

Aunque inicialmente seas el único usuario, incorporaría `Household` desde el primer día.

Esto te permitiría incorporar otro integrante del hogar posteriormente sin tener que rediseñar todas las tablas.

### Reglas de negocio que definiría desde el comienzo

Hay algunas decisiones que te recomiendo establecer como principios:

1\. Los movimientos son la fuente de verdad.

El saldo de una cuenta debería calcularse a partir del saldo inicial y sus movimientos.

No mantendría un saldo editable independiente que pueda quedar desincronizado.

2\. Las transferencias no son ingresos ni gastos.

Ejemplo:

Transferís $100.000 de Banco a Mercado Pago.

- Banco: -$100.000.
- Mercado Pago: +$100.000.
- Ingresos: no cambia.
- Gastos: no cambia.

Una transferencia debe registrarse atómicamente para evitar que quede aplicada en una cuenta y no en la otra.

3\. Los importes deben manejarse con precisión decimal.

Usaría `numeric(19,4)` en PostgreSQL y `decimal` en C#. Nunca utilizaría `float` o `double` para importes monetarios.

4\. Las categorías deben ser configurables.

No codificaría categorías directamente en el backend. El usuario debe poder crear, modificar y organizar sus propias categorías.

5\. Separaría fecha del movimiento y fecha de creación.

Un gasto realizado el 30 de octubre y registrado el 2 de noviembre debe afectar octubre.

6\. Consideraría la moneda desde el inicio.

Aunque inicialmente trabajes solamente con ARS, incorporaría `currency_code` en cuentas y movimientos. No se deben sumar automáticamente importes de monedas diferentes sin una regla de conversión.

7\. Las tarjetas requieren un tratamiento particular.

Una compra con tarjeta es un gasto cuando se realiza. El pago posterior del resumen debe ser una transferencia desde el banco hacia la cuenta de la tarjeta, no un segundo gasto.

Estas reglas son más importantes que la cantidad de tablas que tengamos.

# 4. ¿Conviene implementar SDD?

Sí, lo recomiendo

En este proyecto considero que SDD puede aportarte muchísimo valor.

El motivo es que no estás desarrollando solamente pantallas CRUD. Tenés cálculos financieros, diferentes tipos de movimientos, reglas de imputación, reportes y, seguramente, excepciones que hoy están resueltas mediante fórmulas en Excel.

Como ya venís trabajando con Spec Kit, aprovecharía esa metodología.

La estructura sería:

1. Constitution

   Define principios generales: seguridad, mantenibilidad, arquitectura, pruebas y manejo financiero.
2. Specify

   Describe qué necesita hacer el usuario, sin definir todavía la solución técnica.
3. Plan

   Define arquitectura, tecnologías, persistencia y decisiones técnicas.
4. Tasks

   Descompone el trabajo en tareas pequeñas y ordenadas.
5. Implement

   Desarrolla la funcionalidad.
6. Converge

   Comprueba que la implementación cumpla lo definido en las especificaciones.

Flujo principal de Spec Kit. También cuenta con etapas adicionales de análisis, aclaración y calidad.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://github.com\&sz=32)

GitHub+1

No generaría una especificación gigante para toda la aplicación.

Trabajaría por funcionalidades:

- `001`: Foundation & Authentication.
- `002`: Account Management.
- `003`: Income & Expense Management.
- `004`: Transfers.
- `005`: Dashboard & Financial Reports.
- `006`: Excel Import.

Una ventaja importante es que podés utilizar tu Excel como referencia funcional: sus fórmulas, cálculos y resultados servirían para validar que la nueva aplicación se comporte correctamente.

# 5. ¿Conviene utilizar Playwright?

Sí, pero progresivamente

Playwright es un framework para pruebas automatizadas de extremo a extremo.

Simula el comportamiento de una persona utilizando tu aplicación desde un navegador.

Además, tiene una extensión oficial para VS Code con ejecución, depuración, grabación de interacciones y visualización de resultados.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://playwright.dev\&sz=32)

Playwright+2



Por ejemplo, podrías automatizar esta prueba:

Caso: registrar un gasto

1. Abrir la aplicación.
2. Iniciar sesión.
3. Seleccionar una cuenta.
4. Registrar un gasto de $25.000.
5. Asignar la categoría Alimentación.
6. Confirmar el registro.
7. Comprobar que el saldo se actualizó.
8. Comprobar que el resumen mensual refleja el gasto.

También probaría especialmente:

- Una transferencia no debe modificar los totales de ingresos y gastos.
- Una operación registrada con fecha anterior debe impactar en el mes correspondiente.
- Una cuenta debe reflejar correctamente su saldo.
- Los filtros por período deben devolver los resultados correctos.

Pero no utilizaría Playwright para absolutamente todo.

La estrategia de pruebas que propongo sería:

| Tipo de prueba                | Herramienta |
| ----------------------------- | ----------- |
| Reglas de negocio financieras | xUnit       |
| Servicios y lógica backend    | xUnit       |
| Integración API + PostgreSQL  | xUnit       |
| Flujos reales del usuario     | Playwright  |

Para una aplicación financiera, las pruebas unitarias de las reglas de negocio son particularmente importantes.

# 6. Preparar la futura aplicación móvil

No desarrollaría una app mobile ahora.

Prepararía la arquitectura para que sea posible hacerlo después.

Mi propuesta:

Angular Web

Primera versión

API REST compartida

Todo el procesamiento y las reglas financieras

Web

Angular

Mobile

Angular + Capacitor

Para eso utilizaría Ionic Capacitor.

Capacitor permite empaquetar una aplicación Angular existente para Android e iOS y agregar funcionalidades nativas cuando sean necesarias.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://capacitorjs.com\&sz=32)

Capacitor Documentation+1



La evolución sería:

- Versión 1: aplicación web responsive.
- Versión 1.1: PWA instalable.
- Versión 2: aplicación Android/iOS con Capacitor.

Una aclaración: aunque podés programar el proyecto desde VS Code, la compilación y publicación nativa para iOS requiere Xcode y una Mac.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://capacitorjs.com\&sz=32)

Capacitor Documentation+1



# 7. Roadmap que seguiría

No intentaría desarrollar todo desde el principio.

| Etapa | Funcionalidad                               | Prioridad |
| ----- | ------------------------------------------- | --------- |
| 0     | Análisis del Excel y reglas de negocio      | Inicial   |
| 1     | Arquitectura, autenticación y base de datos | Inicial   |
| 2     | Gestión de cuentas y categorías             | Alta      |
| 3     | Ingresos, gastos y transferencias           | Alta      |
| 4     | Dashboard y resumen mensual                 | Alta      |
| 5     | Importación de Excel y exportaciones        | Alta      |
| 6     | Presupuestos y gastos recurrentes           | Media     |
| 7     | PWA y funcionalidades móviles               | Posterior |
| 8     | Automatizaciones y análisis avanzados       | Posterior |

Algo que considero muy importante: no perdería tiempo inicialmente intentando replicar todos los gráficos y análisis que ya tenés en Excel.

Primero resolvería correctamente el registro de información. Después construiría los reportes sobre esa información.

# 8. Mis decisiones iniciales

Para dejar una primera versión de arquitectura definida, elegiría lo siguiente:

| Decisión      | Elección           |
| ------------- | ------------------ |
| Arquitectura  | Modular Monolith   |
| Desarrollo    | VS Code            |
| Frontend      | Angular            |
| Backend       | ASP.NET Core 10    |
| Persistencia  | EF Core 10         |
| Base de datos | Neon PostgreSQL    |
| API           | REST               |
| SDD           | GitHub Spec Kit    |
| Testing       | xUnit + Playwright |
| Mobile futuro | Capacitor          |
| Repositorio   | GitHub             |

No incorporaría inicialmente: microservicios, Docker obligatorio, Kubernetes, Redis, colas de mensajes ni integración bancaria.

No aportan suficiente valor en esta etapa como para justificar la complejidad.

## 9. ¿Cuál debería ser nuestro primer entregable?

Antes de escribir código, propongo realizar una etapa de definición funcional y técnica.

La dividiría en tres documentos:

1. Project Constitution: principios arquitectónicos, tecnológicos y de calidad.
2. Domain Model: entidades, relaciones y reglas financieras.
3. MVP Specification: funcionalidades concretas de la primera versión, con criterios de aceptación.

A partir de ahí podemos utilizar Spec Kit para generar el plan técnico y las tareas de implementación.

Mi recomendación es que la aplicación nazca como un sistema que pueda administrar una economía familiar, no simplemente como un registrador de gastos personales. Esa pequeña diferencia de diseño puede ahorrarte mucho trabajo cuando quieras incorporar otros integrantes, más cuentas o nuevas funcionalidades.

También podemos establecer un recordatorio semanal para revisar el progreso del MVP:

💻Revisar avance del proyecto cada viernesToca para configurar
