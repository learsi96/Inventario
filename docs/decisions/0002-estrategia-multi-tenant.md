# 0002 — Estrategia multi-tenant

- **Estado:** Aceptada
- **Fecha:** 2026-09-07

## Contexto

El sistema da servicio a varios partners (empresas cliente) con datos aislados.
Empezamos con un cliente pero debe escalar a más sin rediseñar.

## Decisión

**Base de datos única compartida, con discriminador `TenantId`** en toda entidad
de negocio.

- Toda entidad de negocio implementa `Inventario.Domain.Tenancy.ITenantEntity`
  (`Guid TenantId`).
- `AppDbContext` aplica un **global query filter** de EF Core
  (`e => e.TenantId == tenantActual`) a todas esas entidades, resuelto desde
  `ITenantContext`, que a su vez lee el `tenant` claim del JWT.
- Las escrituras asignan `TenantId` automáticamente en `SaveChanges` a partir del
  mismo `ITenantContext`.
- Las consultas con Dapper (reportes) **deben** incluir el filtro por `TenantId`
  explícitamente; se centralizan en repositorios revisados.

## Consecuencias

- Coste de infraestructura mínimo (una sola BD) y operación simple.
- Riesgo principal: una consulta que omita el filtro expone datos de otro tenant.
  Mitigación: filtro global por defecto en EF Core, revisión obligatoria de todo
  SQL de Dapper, y pruebas de aislamiento por tenant.
- La migración futura a "BD por partner" para un cliente grande es posible pero
  costosa; se acepta el riesgo.

## Alternativas consideradas

- **BD por partner:** mejor aislamiento, peor coste y operación. Sobredimensionado
  para el tamaño previsto.
- **Esquema por partner:** complejidad de migraciones alta.
