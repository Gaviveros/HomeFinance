# Modelo de datos: Persistencia en PostgreSQL

## Entidades persistidas

### Usuario

**Propósito**: identidad que se registra e inicia sesión en HomeFinance.

| Atributo | Tipo conceptual | Reglas |
|---|---|---|
| Id | UUID | Clave primaria; estable durante la vida de la cuenta. |
| Email | Texto | Conserva la dirección presentada; obligatoria. |
| NormalizedEmail | Texto | Derivada de email recortado y en minúsculas; obligatoria y única. |
| PasswordHash | Texto | Hash adaptativo con sal, nunca contraseña en claro; suficiente longitud para almacenar versiones del hasher. |
| CreatedAt | Instante UTC | Obligatorio; representa cuándo se creó la cuenta. |

**Reglas de identidad**:

- Dos cuentas no pueden compartir el mismo correo después de normalizarlo, incluso con registros concurrentes.
- Los espacios periféricos no forman parte de la identidad del correo; la comparación no distingue mayúsculas.
- El login valida el hash actual. Si encuentra el formato heredado SHA-256 y la contraseña coincide, lo sustituye por el formato actual y persiste el cambio.
- Las credenciales de autenticación emitidas identifican al usuario mediante su `Id`; un identificador provisto por el cliente no sustituye la validación de la credencial.

### Hogar

**Propósito**: espacio privado de configuración que tiene un único titular.

| Atributo | Tipo conceptual | Reglas |
|---|---|---|
| Id | UUID | Clave primaria. |
| Name | Texto | Obligatorio y no vacío después de quitar espacios periféricos. |
| OwnerId | UUID | Obligatorio; clave foránea a `Usuario.Id`. |
| Currency | Texto | Obligatorio; valor inicial `ARS`. |
| TimeZone | Texto | Obligatorio; valor inicial `America/Argentina/Cordoba`. |
| CreatedAt | Instante UTC | Obligatorio; representa cuándo se creó el hogar. |

**Relaciones y reglas**:

- Un usuario puede ser titular de cero o más hogares.
- Cada hogar tiene exactamente un titular existente; no se permiten referencias huérfanas.
- La eliminación de un usuario con hogares asociados se restringe; no hay borrado en cascada ni transferencia de titularidad en este alcance.
- Solo el titular autenticado puede listar, ver o cambiar la configuración de su hogar. No existen entidades de membresía en esta iteración.
- La configuración forma parte del registro del hogar actual; no se introduce una tabla separada.

## Integridad y persistencia

- Aplicar la unicidad del correo normalizado y la relación `Hogar.OwnerId → Usuario.Id` en el esquema, además de las validaciones de aplicación.
- Indexar las claves usadas para localizar usuarios por correo y hogares por titular.
- Guardar instantes UTC; `TimeZone` es un identificador independiente y no equivale al desplazamiento horario del instante.
- Las operaciones de actualización deben confirmar el cambio en el almacenamiento antes de responder éxito.
- Las migraciones iniciales deben poder crear el esquema en una base vacía. Cambios futuros compatibles deben conservar filas existentes.

## Entidades fuera del alcance

No se agregan cuentas financieras, movimientos, presupuestos, tarjetas, préstamos, fondos, reportes, sesiones persistidas, membresías ni registros de auditoría financieros.
