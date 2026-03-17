using 'main.bicep'

// Choose a globally unique name — used as the App Service name and prefix for plan/SWA
// e.g. 'todo-cicd-demo-abc123' (must be 2-60 chars, alphanumeric and hyphens only)
param appName = 'todo-cicd-demo'

// Azure region for the App Service resources
param location = 'eastus'
