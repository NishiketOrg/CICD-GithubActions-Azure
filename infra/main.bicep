targetScope = 'resourceGroup'

param appName string
param location string = resourceGroup().location

module plan 'modules/appserviceplan.bicep' = {
  params: {
    planName: '${appName}-plan'
    location: location
  }
}

module swa 'modules/staticwebapp.bicep' = {
  params: {
    staticWebAppName: '${appName}-swa'
  }
}

// Pass the SWA hostname as an allowed CORS origin for the backend
module web 'modules/webapp.bicep' = {
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
