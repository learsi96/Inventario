# 0003 — Acceso a datos

- **Estado:** Aceptada
- **Fecha:** 2026-09-07

## Contexto

El backend tiene dos perfiles de acceso a datos muy distintos:

1. CRUD del dominio (catálogo, sucursales, movimientos, conteos): muchas
   entidades, muchas escrituras, necesita migraciones versionadas y el filtro
   multi-tenant automático.
2. Reportes y listados de lectura pesada (kardex, valorización, agregaciones):
   consultas donde importa el SQL afinado y el rendimiento.

## Decisión

**Enfoque híbrido:**

- **EF Core** para el modelo de dominio, todas las escrituras, el CRUD, las
  migraciones y el *global query filter* por `TenantId` (ver
  [0002](0002-estrategia-multi-tenant.md)).
- **Dapper** para reportes y consultas de solo lectura donde EF Core genere SQL
  subóptimo. Estas consultas viven en repositorios dedicados en
  `Inventario.Infrastructure` y **siempre** filtran por `TenantId` de forma
  explícita.

Ambas librerías comparten la misma cadena de conexión y transacción cuando hace
falta.

## Consecuencias

- Productividad alta en el 80% del código (CRUD) y control fino donde importa.
- Dos modelos mentales de acceso a datos; se acota Dapper a repositorios de
  lectura claramente identificados.
- El filtro por tenant deja de ser automático en las rutas Dapper: requiere
  disciplina y pruebas.

## Alternativas consideradas

- **Solo EF Core:** más simple, pero SQL de reportes difícil de optimizar.
- **Solo Dapper:** máximo control, pero se pierde el filtro global, las
  migraciones y mucho tiempo en boilerplate de CRUD.
- **Stored procedures:** mueve lógica a la BD, complica versionado y testing.
