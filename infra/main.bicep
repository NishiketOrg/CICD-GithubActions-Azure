targetScope = 'subscription'

param appName string
param location string = 'eastus'
param resourceGroupName string = 'rg-cicd-demo'

// Create the resource group as part of the Bicep deployment
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
}

module plan 'modules/appserviceplan.bicep' = {
  scope: rg
  params: {
    planName: '${appName}-plan'
    location: location
  }
}

module swa 'modules/staticwebapp.bicep' = {
  scope: rg
  params: {
    staticWebAppName: '${appName}-swa'
  }
}

// Pass the SWA hostname as an allowed CORS origin for the backend
module web 'modules/webapp.bicep' = {
  scope: rg
  params: {
    webAppName: appName
    location: location
    planId: plan.outputs.planId
    allowedOrigins: [
      'http://localhost:4200'
      swa.outputs.staticWebAppUrl
    ]
  }
}

output webAppUrl string = web.outputs.webAppUrl
output staticWebAppUrl string = swa.outputs.staticWebAppUrl

@secure()
output staticWebAppDeploymentToken string = swa.outputs.deploymentToken
