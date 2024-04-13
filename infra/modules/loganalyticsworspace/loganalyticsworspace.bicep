param location string = resourceGroup().location

param namelaw string =  ''
param tags object = {}

param applicationInsightsName string =  ''

param kvName string =  ''

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: namelaw
  tags: tags
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    workspaceCapping: {
      dailyQuotaGb: json('0.1')
    }
    retentionInDays: 30
  }
}


resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: applicationInsightsName
  location: location
  kind: 'other'
  properties: {
    WorkspaceResourceId: logAnalytics.id
    Application_Type: 'other'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: kvName
}

resource appInsConnSecret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  parent: keyVault
  name: 'appins-connectionstring'
  properties: {
    value: appInsights.properties.ConnectionString
  }
}

resource lawCustomKeySecret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  parent: keyVault
  name: 'law-customerkey'
  properties: {
    value: logAnalytics.listKeys().primarySharedKey
  }
}


output logAnalyticsId string = logAnalytics.id
output instrumentationKey string = appInsights.properties.InstrumentationKey
output appInsConnStrKvUri string = appInsConnSecret.properties.secretUri
output logAnalyticsIdName string = appInsights.name
output customerId string = logAnalytics.properties.customerId
