@description('Location of the resources.')
param location string = resourceGroup().location

@description('Environment')
param environment string

@description('Base Name of the Lavalink container app.')
param containerAppLavalinkNameBase string = 'ca-lavalink'

@description('Base Name of the DiscordBot container app.')
param containerAppDiscordBotNameBase string = 'ca-discordbotabordelv2'

@description('Base Name of the Storage Account.')
param storageAccountNameBase string = 'stbotabordel'

@description('Base Name of the App Configuration.')
@minLength(5)
param appConfigNameBase string = 'APPCS-BotaBordelV2'

@description('Base Name of the Managed Environments.')
param managedEnvironmentsNameBase string = 'CAE-BotaBordelV2'

@description('Base Name of the Container Registry.')
param containerRegistryNameBase string = 'CRDiscordBotABordelV2'

@description('Name of the Log Analytics Workspace.')
@minLength(5)
param laWorkspaceNameBase string = 'law-botabordelv2'

@description('Name of the Application Insights.')
@minLength(5)
param appInsNameBase string = 'appins-botabordelv2'

@description('Path to the Lavalink image.')
param lavalinkImage string = 'ghcr.io/lavalink-devs/lavalink:4.0.0-beta.4'

@description('Path to the DiscordBot image.')
param discordBotImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@description('Name of the Key Vault.')
param keyVaultBaseName string = 'kv-botabordelv2'

@description('Lavalink Internal config')
param lavalinkConfig object = {
  server: {
    password: 'youshallnotpass'
    address: '0.0.0.0'
    port: '2333'
  }
}

@description('Tags')
param tags object = {
  Environment: environment
  Unit: 'HM Hobby'
  Maintainer: 'Hugo Mathieu'
}

var caLavalinkName = '${containerAppLavalinkNameBase}-${toLower(environment)}'
var caDiscordBotName = '${containerAppDiscordBotNameBase}-${toLower(environment)}'
var storageAccountName = '${storageAccountNameBase}${toLower(environment)}'
var appConfigName = '${appConfigNameBase}-${toUpper(environment)}'
var managedEnvironmentsName = '${managedEnvironmentsNameBase}-${toUpper(environment)}'
var containerRegistryName = '${containerRegistryNameBase}${toUpper(environment)}'
var laWorkspaceName = '${laWorkspaceNameBase}-${toLower(environment)}'
var appInsName = '${appInsNameBase}-${toLower(environment)}'
var kvName = '${keyVaultBaseName}-${toLower(environment)}'
var lavalinkContainerName = 'lavalink'

var discordBotContainerName = 'discordbotabordelv2'

var appConfConnStrKvName = 'appconf-connectionstring'
var lavalinkPasswordKvName = 'lavalink-password'

module containerApps './modules/containerapps/containerapps.bicep' = {
  name: 'containerApps'
  params: {
    location: location
    lawCustomerId: logAnalytics.outputs.customerId
    lawCustomerKey: keyVault.getSecret('law-customerkey')
    managedEnvironmentsName: managedEnvironmentsName
    acrUri: containerRegistry.properties.loginServer
    appConfId: appConfig.id
    appConfKvSecretUri: first(filter(kvModule.outputs.secretUris, secret => secret.name == appConfConnStrKvName)).uri
    caDiscordBotName: caDiscordBotName
    caLavalinkName: caLavalinkName
    discordBotContainerName: discordBotContainerName
    lavalinkContainerName: lavalinkContainerName
    lavalinkImage: lavalinkImage
    lavalinkConfig: lavalinkConfig
    discordBotImage: discordBotImage
    lavalinkPasswordKvSecretUri: first(filter(kvModule.outputs.secretUris, secret => secret.name == lavalinkPasswordKvName)).uri
    appInsKvSecretUri: logAnalytics.outputs.appInsConnStrKvUri
    storageAccountName: storageAccountName
    shareName: storageAccount::fileService::shares.name
    tags: tags
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: kvName
}

module kvModule './modules/keyvault/keyvault.bicep' = {
  name: 'kvSecrets'
  params: {
    location: location
    kvName: kvName
    secrets: [
      { name: 'appconf-connectionstring', value: appConfig.listKeys().value[0].value }
      { name: 'lavalink-password', value: lavalinkConfig.server.password }
    ]
  }
}

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: appConfigName
  location: location
  tags: tags
  sku: {
    name: 'free'
  }
  properties: {
    encryption: {}
    disableLocalAuth: false
    softDeleteRetentionInDays: 0
    enablePurgeProtection: false
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: containerRegistryName
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    policies: {
      azureADAuthenticationAsArmPolicy: {
        status: 'enabled'
      }
    }
    publicNetworkAccess: 'Enabled'
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
  }

  resource fileService 'fileServices@2023-01-01' = {
    name: 'default'

    resource shares 'shares@2023-01-01' = {
      name: 'botabordelresources'
      properties: {
        accessTier: 'Hot'
        shareQuota: 5120
        enabledProtocols: 'SMB'
      }
    }
  }
}

module logAnalytics './modules/loganalyticsworspace/loganalyticsworspace.bicep' = {
  name: 'logAnalytics'
  params: {
    location: location
    tags: tags
    applicationInsightsName: appInsName
    namelaw: laWorkspaceName
    kvName: kvName
  }
}
