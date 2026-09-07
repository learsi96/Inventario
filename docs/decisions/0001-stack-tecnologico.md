# 0001 — Stack tecnológico

- **Estado:** Aceptada
- **Fecha:** 2026-09-07

## Contexto

Sistema de inventario multi-partner para refaccionarias, con cliente web y app
móvil, que consume una API propia. El autor trabaja habitualmente con el
ecosistema .NET / Azure DevOps y quiere profundizar en DevOps sobre Azure.

## Decisión

- **Frontend web:** Angular (última LTS).
- **App móvil:** Ionic + Angular + Capacitor, compartiendo código con la web.
- **Backend:** .NET 10 (LTS), API REST.
- **Base de datos:** SQL Server (local en contenedor; Azure SQL Database en la nube).
- **Almacenamiento de archivos:** Azure Blob Storage.
- **CI/CD:** Azure DevOps (Repos + Pipelines).
- **IaC:** Bicep.
- **Observabilidad:** Application Insights.

## Consecuencias

- Un solo lenguaje (TypeScript) en web y móvil; un solo lenguaje (C#) en backend.
- Curva de aprendizaje concentrada en DevOps/Azure, que es un objetivo explícito.
- Dependencia del ecosistema Microsoft; se asume conscientemente.

## Alternativas consideradas

- **React / React Native:** descartado por preferencia y experiencia del autor.
- **Backend en Node:** descartado; el autor es más fuerte en .NET y quiere
  mantener el proyecto mantenible por él mismo.
- **PostgreSQL:** viable y más barato en la nube, pero SQL Server encaja mejor
  con el conocimiento del autor y con Azure SQL. Revisable si el coste aprieta.
