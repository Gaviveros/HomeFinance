### Conclusión ejecutiva

El archivo funciona como **dos módulos dentro de un mismo proyecto financiero**:

1. **Finanzas personales/familiares:** presupuesto mensual, movimientos, reservas, tarjetas y crédito.
2. **“Alma Mía”:** inventario, ventas, cobros, caja y arqueos de un pequeño negocio.

La arquitectura actual es similar a una aplicación con **tablas operativas**, una **capa de cálculo oculta** y un **panel de consulta**. La tabla `t_mov` es el núcleo de las finanzas personales; `al_mov` cumple un papel equivalente para el negocio. No existe todavía una integración contable automática entre ambos módulos. 

---

### 1. Mapa funcional de las hojas

| Hoja | Función actual |
|---|---|
| `h_mov` | Libro mayor de movimientos personales: fecha, tipo, concepto, importe y descripción. |
| `_h_res_pct` | Capa de cálculo auxiliar del presupuesto mensual; no debería ser una pantalla de usuario. |
| `h_res` | Panel mensual: presupuesto, saldo por rubro, reservas y proyección de tarjetas. |
| `h_tarj` | Registro maestro de compras financiadas y cuotas. |
| `h_cred` | Cronograma independiente de un crédito de 36 cuotas. |
| `al_mov` | Operaciones de Alma Mía: mercadería, ventas, caja y arqueos. |
| `al_res` | Resumen de caja e inventario de Alma Mía. |



---

## 2. Módulo de finanzas personales

### 2.1. Libro mayor: `h_mov`

La tabla `t_mov` contiene 509 registros y es la principal fuente transaccional. Sus campos son:

- `fecha`: fecha contable.
- `tipo`: naturaleza del movimiento.
- `item`: concepto presupuestario o cuenta.
- `monto`: importe.
- `Desc`: detalle libre.

Los valores de `tipo` tienen, al menos, tres significados:

- `previsto`: presupuesto asignado.
- `egreso`: gasto realizado.
- `ingreso`: entrada de dinero.

El sistema no utiliza una tabla separada de presupuestos: **presupuesto y movimientos reales conviven en `t_mov`**, diferenciados por `tipo`. Esto simplifica Excel, pero en una aplicación convendría separar “asignaciones presupuestarias” de “transacciones efectivas”. 

### 2.2. Mes de trabajo

`h_res!B3` es el controlador temporal de todo el panel. Tiene una lista desplegable basada en `X2:X10`, con meses comprendidos entre abril y diciembre de 2026. Los cálculos incluyen movimientos cuya fecha sea:

- Mayor o igual al primer día del mes seleccionado.
- Menor al primer día del mes siguiente.

La aplicación web debería reemplazar este control por un selector de período y utilizar un identificador de período como `2026-11`, evitando depender del formato visual `nov-26`. 

---

## 3. Presupuesto mensual

### 3.1. Distribución automática del sueldo

El ingreso base se calcula únicamente a partir de movimientos con:

- `tipo = "ingreso"`
- `item = "Sueldos"`
- Fecha dentro del mes seleccionado.

Ese sueldo se distribuye automáticamente así:

- **85%:** disponible para presupuesto general.
- **10%:** `Diezmo`.
- **5%:** `Ahorros`.

Para noviembre de 2026, el panel muestra $2.698.750 como base presupuestaria, $317.500 para diezmo y $158.750 para ahorros. 

La regla principal es equivalente a:

```excel
=SUMAR.SI.CONJUNTO(t_mov[monto];t_mov[tipo];"ingreso";t_mov[item];"Sueldos";
 t_mov[fecha];">="&mes;t_mov[fecha];"<"&FECHA.MES(mes;1))*85%
```

Esta distribución 85/10/5 está codificada directamente en fórmulas. En la web debería convertirse en una configuración editable y versionada, por ejemplo:

```text
ReglaDistribucionIngreso
- vigencia_desde
- porcentaje_presupuesto
- porcentaje_diezmo
- porcentaje_ahorro
```

### 3.2. Presupuesto por concepto

Para cada concepto, `_h_res_pct` suma los movimientos `previsto` del mes. Los conceptos actuales están agrupados en:

- Básico
- Impuestos
- Movilidad
- Salud
- Tarjeta
- Educación
- Varios
- Sobrante

Entre los conceptos se encuentran Alquiler, Comida, Crédito, Agua, Internet, Luz, Combustible, Médicos, CAECE, Spotify y Otros. 

```excel
=SUMAR.SI.CONJUNTO(t_mov[monto];t_mov[tipo];"previsto";
 t_mov[item];item;
 t_mov[fecha];">="&mes;
 t_mov[fecha];"<"&FECHA.MES(mes;1))
```

### 3.3. Significado real de la columna “real”

Existe una ambigüedad importante: la columna `real` de `t_res` **no representa el gasto real**, sino el **saldo disponible**:

```excel
=PresupuestoPrevisto-SumaDeEgresos
```

Por ejemplo, Comida tiene $450.000 previstos y $219.400 mostrados como “real”. Según la fórmula, eso significa que se gastaron $230.600 y quedan $219.400 disponibles. 

Para la aplicación recomiendo utilizar tres campos explícitos:

| Campo web | Significado |
|---|---|
| Presupuestado | Importe asignado al concepto |
| Ejecutado | Suma de egresos |
| Disponible | Presupuestado menos ejecutado |

Esto evita trasladar a la web una etiqueta actualmente engañosa.

### 3.4. Sobrante

El sobrante se calcula como:

```excel
=IngresoBase-SUMA(PresupuestosPorConcepto)
```

No descuenta directamente los gastos reales: mide **dinero todavía no asignado al presupuesto**, no saldo bancario ni efectivo disponible. En noviembre aparece un sobrante presupuestario de $4.328. 

---

## 4. Resúmenes y cambio de vista

`h_res!C26` permite alternar entre:

- `Importes`
- `Porcentajes`

Este cambio modifica el tipo de salida de numerosas fórmulas:

- En modo importes devuelve números.
- En modo porcentajes devuelve texto formateado mediante `TEXTO`.

Esto produce columnas con tipos variables: una misma celda puede contener un número o una cadena como `"45%"`. En la web, la medida debe conservarse siempre como número y cambiar únicamente su presentación. 

### Resumen por categoría

`t_res_rese` agrega los saldos por categoría mediante `SUMAR.SI`. Su “diferencia” se calcula como:

```excel
=Real-Total
```

En modo porcentajes:

```excel
=(Real-Total)/ABS(Total)
```

Sin embargo, los encabezados `total`, `real` y `diferencia` no expresan claramente qué representa cada medida. Deben redefinirse antes de migrar para distinguir presupuesto, ejecución y saldo. 

---

## 5. Fondos y reservas

La tabla `t_rese` administra conceptos como:

- Ahorros
- Alma mia
- Arreglos
- Cuota CAECE
- Imprevistos
- Vacaciones

Para cada fondo calcula el movimiento neto mensual:

```excel
=IngresosDelFondo-EgresosDelFondo
```

La columna `recaudado` calcula el saldo histórico completo, sin filtro mensual, utilizando todos los movimientos del concepto. Por tanto, el modelo distingue implícitamente:

- Flujo del mes.
- Objetivo o importe cargado manualmente.
- Diferencia.
- Saldo acumulado histórico.

Actualmente algunos objetivos o valores “reales” son manuales. La aplicación debería modelar cada fondo con `objetivo`, `saldo_inicial`, `movimientos` y `saldo_actual`. 

---

## 6. Tarjetas y cuotas

### 6.1. Registro maestro

`h_tarj` contiene `t_tarj`, con:

- Tipo o tarjeta.
- Número total de cuotas.
- Importe de cada cuota.
- Detalle.
- Fecha de compra.
- Mes de comienzo.

El campo que realmente controla la proyección es `comienzo`; la fecha de compra no participa en los cálculos actuales. 

### 6.2. Regla de vigencia de una cuota

Para un mes consultado, se calcula:

```text
número de cuota =
meses transcurridos desde comienzo + 1
```

La compra aparece si:

```text
número de cuota >= 1
y
número de cuota <= total de cuotas
```

Cada proyección muestra:

- Tarjeta o tipo.
- Texto “n de total”.
- Importe.
- Detalle.

Después ordena las cuotas primero por cantidad de cuotas restantes y luego por importe, ambos en forma descendente. 

### 6.3. Proyección temporal

`h_res` presenta tres meses simultáneos:

- Mes seleccionado.
- Mes siguiente.
- Segundo mes siguiente.

Para noviembre de 2026, los totales proyectados son:

- Noviembre: $118.486.
- Diciembre: $48.000.
- Enero de 2027: $38.000.

La proyección representa compromisos programados, pero no registra si una cuota fue efectivamente pagada. En la web se debería incorporar un estado como `pendiente`, `pagada`, `vencida` o `cancelada`. 

---

## 7. Crédito

`h_cred` contiene un cronograma de 36 cuotas con:

- Mes.
- Valor.
- Número de cuota.

Actualmente no hay fórmulas que conecten `t_cred` con `h_res` o `t_mov`. Por tanto, el crédito funciona como **tabla informativa independiente**. Para que aparezca en el presupuesto, parece necesario registrar manualmente un movimiento con el concepto `Crédito`. 

En la aplicación debería definirse una relación:

```text
Crédito
  1 ─── N CuotasCrédito
  1 ─── N Pagos
```

Con ello se podrían generar automáticamente los compromisos mensuales y conciliarlos con pagos reales.

---

## 8. Módulo Alma Mía

### 8.1. Operaciones de mercadería

`al_ingreso` funciona como registro de movimientos de productos, aunque su nombre sugiere solamente entradas. Incluye:

- Fecha.
- Tipo en `Columna1` (`ingreso` o `egreso`).
- Producto.
- Cantidad.
- Costo unitario.
- Precio final unitario.
- Detalle.

`al_egreso` registra salidas o ventas con producto, cantidad, pago y detalle. Existe una superposición conceptual entre `al_ingreso` y `al_egreso`: ambos contienen información de salida, pero cumplen funciones diferentes en los cálculos. Esto debe normalizarse en la migración. 

### 8.2. Inventario

`al_res` calcula las existencias por producto como:

```excel
=SUMAPRODUCTO(entradas del producto)-SUMAPRODUCTO(salidas del producto)
```

Se aplica `ESPACIOS` al producto de los movimientos, pero no al producto del catálogo. Esto revela que los nombres de producto actúan como claves y que existen riesgos por espacios o variaciones ortográficas. 

En la web cada producto debe tener un `producto_id`; las comparaciones no deberían depender del texto.

### 8.3. Caja

`al_mov_dine` registra movimientos monetarios con fecha, tipo, importe y detalle. La caja resumida se obtiene mediante:

```text
Ingresos de caja
- Egresos de caja
+ Cobros de ventas
```

Si el campo `pago` de una venta contiene `"-"`, el sistema busca el precio final del producto mediante `BUSCARX`; en caso contrario utiliza el valor escrito en `pago`. El saldo mostrado actualmente es $111.400. 

El uso de `"-"` como indicador de ausencia es frágil. En la web debe utilizarse `null` junto con una regla explícita:

```text
importe_cobrado =
importe informado
o, si no existe,
cantidad × precio vigente
```

También debe definirse si `pago` es unitario o total. La fórmula actual multiplica `cantidad × pago`, lo que implica que se interpreta como valor unitario.

### 8.4. Arqueos

`Tabla12` registra:

- Fecha.
- Total esperado.
- Total real.
- Diferencia.

Actualmente la diferencia está cargada como dato, no calculada mediante fórmula. En la web debería ser siempre:

```text
diferencia = real - esperado
```

Además, cada arqueo debería guardar el período de operaciones conciliado y el usuario responsable. 

---

## 9. Relación entre ambos módulos

La conexión entre finanzas personales y Alma Mía es principalmente conceptual:

- `Alma mia` aparece como fondo o cuenta en `h_res`.
- Las operaciones detalladas del negocio viven en `al_mov`.
- No hay fórmulas que lleven automáticamente caja, ventas, aportes o retiros de Alma Mía hacia `t_mov`.
- Los créditos anotados al pie de `al_mov` tampoco están estructurados como tabla operativa.

Por tanto, existe riesgo de doble carga o diferencias entre el saldo del negocio y el saldo reconocido en las finanzas personales. 

La web debería manejar transferencias internas mediante una sola operación con dos contrapartidas:

```text
Salida: Finanzas personales / Fondo Alma Mía
Entrada: Caja de Alma Mía
```

---

## 10. Modelo de datos recomendado

### Entidades principales

1. `periodos`
2. `categorias_presupuesto`
3. `conceptos`
4. `transacciones`
5. `presupuestos_mensuales`
6. `fondos`
7. `movimientos_fondo`
8. `tarjetas`
9. `compras_cuotas`
10. `cuotas_generadas`
11. `creditos`
12. `cuotas_credito`
13. `productos`
14. `movimientos_inventario`
15. `ventas`
16. `cobros`
17. `movimientos_caja`
18. `arqueos`
19. `transferencias_internas`
20. `reglas_distribucion`

### Relaciones esenciales

```text
Categoría 1 ─── N Conceptos
Concepto 1 ─── N Presupuestos mensuales
Concepto 1 ─── N Transacciones

Tarjeta 1 ─── N Compras
Compra 1 ─── N Cuotas

Producto 1 ─── N Movimientos de inventario
Venta 1 ─── N Detalles de venta
Venta 1 ─── N Cobros

Fondo 1 ─── N Movimientos de fondo
Crédito 1 ─── N Cuotas de crédito
```

---

## 11. Riesgos que conviene resolver antes de desarrollar

1. **“Real” significa cosas distintas** según la tabla: saldo, valor manual o importe ejecutado.
2. **Claves basadas en texto:** concepto, categoría y producto dependen de coincidencias exactas.
3. **Porcentajes convertidos en texto**, lo que mezcla presentación con lógica.
4. **Reglas 85/10/5 codificadas**, sin vigencia ni configuración.
5. **Crédito desconectado** del presupuesto y de los pagos.
6. **Cuotas proyectadas sin estado de pago**.
7. **Alma Mía no concilia automáticamente** con las finanzas personales.
8. **Uso de `"-"` para representar datos ausentes**.
9. **Arqueos y varias diferencias cargados manualmente**.
10. **Catálogos incompletos:** no existen tablas maestras formales para conceptos, productos, cuentas o tipos de movimiento.
11. **Sin trazabilidad de cambios:** no hay usuario, fecha de creación, modificación ni historial.
12. **Campos genéricos** como `Columna1`, que esconden su significado real. 

### Tesis final

El archivo ya contiene la lógica necesaria para una primera versión de la web, pero la migración no debería copiar las hojas literalmente. La mejor estrategia es conservar sus reglas —períodos, presupuesto, distribución 85/10/5, fondos, cuotas, inventario y caja— mientras se normalizan los conceptos, se separan datos de presentación y se integran formalmente las finanzas personales con Alma Mía.