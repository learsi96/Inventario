# 0005 — Infraestructura como código

- **Estado:** Aceptada
- **Fecha:** 2026-09-07

## Contexto

Los entornos de nube (`staging`, luego `prod`) deben ser reproducibles y
versionados, y el autor quiere aprender el flujo DevOps sobre Azure. El
despliegue objetivo es Azure (App Service / Static Web Apps / Azure SQL /
Blob Storage / Key Vault / Application Insights).

## Decisión

- **Bicep** para describir los recursos de Azure, en `infra/bicep/`.
- Despliegue desde **Azure Pipelines** (pipelines de CD, Hito 9).
- **Docker Compose** para la infraestructura local de desarrollo (SQL Server),
  independiente de Bicep.
- En el MVP1 el Bicep es mínimo: solo el entorno de pruebas. Se amplía en el
  Hito 9.

## Consecuencias

- Menos verboso que ARM y con soporte nativo de Microsoft.
- Atado a Azure: migrar a otra nube exigiría reescribir la IaC.
- El autor aprende Bicep + pipelines de CD, que es un objetivo del proyecto.

## Alternativas consideradas

- **Terraform:** multi-nube y con más ecosistema, pero añade una herramienta y un
  lenguaje extra sin beneficio mientras todo sea Azure.
- **Portal de Azure a mano:** no reproducible, no versionado. Descartado.
