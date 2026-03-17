// App Service Plan (the hosting infrastructure for the Web App)
param planName string
param location string

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: planName
  location: location
  sku: {
    name: 'F1'  // Free tier
    tier: 'Free'
  }
  kind: 'linux'
  properties: {
    reserved: true  // required for Linux plans
  }
}

output planId string = appServicePlan.id
