# azure-pipelines/

Definiciones de CI/CD para Azure DevOps.

| Archivo | Qué hace | Estado |
|---|---|---|
| `backend-ci.yml` | Restore + build + test de la solución .NET | **Sin vincular.** Crear el pipeline en Azure DevOps apuntando a este YAML |
| `frontend-ci.yml` | `pnpm install` + `nx run-many -t lint test build` | **Sin vincular** |

## Pendiente (Hito 9)

- Crear el proyecto en Azure DevOps y subir el repo a Azure Repos.
- Dar de alta ambos pipelines apuntando a estos YAML.
- Añadir los pipelines de **CD** (despliegue a `staging` y `prod`) con Bicep.
- Migrar el CI de frontend de `nx run-many` a `nx affected` (`nx-set-shas`).
- Mover secretos a Azure Key Vault y vincularlos como *variable groups*.
