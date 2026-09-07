# infra/

Infraestructura del proyecto.

## Desarrollo local

`docker-compose.yml` levanta SQL Server 2022 (edición Developer) para desarrollo.

```bash
cp infra/.env.example infra/.env         # ajusta la contraseña si quieres
docker compose -f infra/docker-compose.yml up -d
docker compose -f infra/docker-compose.yml ps   # espera a que quede "healthy"
```

La cadena de conexión de la API en `Development` ya apunta aquí
(`backend/src/Inventario.Api/appsettings.Development.json`). Si cambias la
contraseña en `infra/.env`, actualízala también ahí.

Para aplicar el esquema:

```bash
dotnet ef database update \
  --project backend/src/Inventario.Infrastructure \
  --startup-project backend/src/Inventario.Api
```

Parar y borrar datos:

```bash
docker compose -f infra/docker-compose.yml down -v
```

## Nube (Hito 9)

Los recursos de Azure se definirán aquí como plantillas **Bicep**
(`infra/bicep/`), desplegadas desde Azure Pipelines. Ver
`docs/decisions/0005-infraestructura-como-codigo.md`.
