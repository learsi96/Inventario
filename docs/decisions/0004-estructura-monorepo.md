# 0004 — Estructura de repositorio (monorepo)

- **Estado:** Aceptada
- **Fecha:** 2026-09-07

## Contexto

El proyecto tiene tres artefactos: cliente web (Angular), app móvil (Ionic +
Angular) y API (.NET). Web y móvil comparten modelos de dominio, tipos del
contrato de la API, autenticación/tenant, validaciones y componentes de UI.

## Decisión

**Un único repositorio Git** con dos zonas:

```
frontend/   workspace Nx (pnpm). apps/web y apps/mobile comparten libs/.
backend/    solución .NET. Clean Architecture: Domain / Application /
            Infrastructure / Api.
infra/      docker-compose (local) y Bicep (nube).
docs/       PRD, roadmap, ADRs.
azure-pipelines/  YAML de CI/CD.
```

- **Nx** gestiona `frontend/`: fuerza límites entre librerías por tags
  (`@nx/enforce-module-boundaries`), da grafo de dependencias y `nx affected`.
- La solución .NET vive en `backend/` **fuera de Nx** (Nx no gestiona bien
  proyectos .NET).
- CI: pipelines separados para `frontend/` y `backend/`, disparados por rutas.

## Consecuencias

- Un solo historial; los cambios que tocan contrato de API y cliente van en el
  mismo PR.
- Sin publicación de paquetes npm privados para compartir código front.
- El repo mezcla dos toolchains (pnpm/Node y dotnet); los desarrolladores
  necesitan ambas instaladas.

## Alternativas consideradas

- **Repos separados (front / back / infra):** aíslan toolchains pero rompen la
  atomicidad de los cambios de contrato y multiplican la gestión.
- **Todo dentro de Nx (incluida la API con un plugin .NET):** los plugins .NET
  para Nx no están lo bastante maduros.
