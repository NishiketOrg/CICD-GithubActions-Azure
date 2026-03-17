// Web App running .NET 8 on the provided App Service Plan
param webAppName string
param location string
param planId string
param allowedOrigins string[]

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  properties: {
    serverFarmId: planId
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      cors: {
        allowedOrigins: allowedOrigins
        supportCredentials: false
      }
    }
    httpsOnly: true
  }
}

output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
