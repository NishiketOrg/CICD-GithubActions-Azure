using 'main.bicep'

// Choose a globally unique name — used as the App Service name and prefix for plan/SWA
// e.g. 'todo-cicd-demo-abc123' (must be 2-60 chars, alphanumeric and hyphens only)
param appName = 'todo-cicd-nishiket'

// Azure region for all resources
param location = 'eastus'

// Resource group that Bicep will create automatically
param resourceGroupName = 'rg-cicd-demo'
