// Azure Static Web App for the Angular frontend
param staticWebAppName string

// Static Web Apps are only available in a limited set of regions
resource staticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  name: staticWebAppName
  location: 'eastus2'
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'

@secure()
output deploymentToken string = staticWebApp.listSecrets().properties.apiKey
