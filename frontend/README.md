# frontend/

Workspace **Nx** (pnpm) del cliente web y la app móvil.

## Proyectos

| Proyecto | Tipo | Descripción |
|---|---|---|
| `web` | app Angular | Cliente web. `apps/web` |
| `mobile` | app Angular | Base de la app móvil. Ionic + Capacitor se añaden en el Hito 7. `apps/mobile` |
| `shared-domain` | lib | Modelos y contratos de dominio (sin framework). `@inventario/shared-domain` |
| `shared-util` | lib | Utilidades puras. `@inventario/shared-util` |
| `shared-ui` | lib | Componentes de presentación reutilizables. `@inventario/shared-ui` |
| `shared-api-client` | lib | Cliente HTTP tipado contra la API .NET. `@inventario/shared-api-client` |
| `shared-auth` | lib | Login, token, guard, interceptor, tenant. `@inventario/shared-auth` |
| `inventario-data-access` | lib | Estado y llamadas del dominio de inventario. `@inventario/inventario-data-access` |

## Comandos

```bash
pnpm install
pnpm nx serve web                       # http://localhost:4200
pnpm nx run-many -t lint test build typecheck
pnpm nx graph                           # grafo de dependencias
pnpm nx e2e web-e2e                     # requiere: pnpm exec playwright install
```

## Límites entre librerías

`eslint.config.mjs` aplica `@nx/enforce-module-boundaries` según los tags de cada
proyecto:

- `scope:inventario` puede depender de `scope:shared`, no al revés.
- `type:ui` y `type:util` no dependen de `type:feature` ni `type:data-access`.
- `type:domain` solo depende de `type:util`.

## Notas

- Test runner: **Vitest** (vía `@analogjs/vitest-angular`).
- Build de Angular: `@angular/build:application` (esbuild).
- Al añadir una librería, pásale `--tags` correctos o el lint de límites fallará.
