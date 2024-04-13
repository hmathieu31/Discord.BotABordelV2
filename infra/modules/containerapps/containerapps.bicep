param location string = resourceGroup().location

param tags object = {}

param managedEnvironmentsName string = ''
param caLavalinkName string = ''
param caDiscordBotName string = ''

param lavalinkImage string = ''
param lavalinkContainerName string = ''
param lavalinkConfig object = {
  server: {
    password: ''
    address: ''
    port: ''
  }
}

param discordBotContainerName string = ''
param discordBotImage string = ''

param acrUri string = ''
param appConfId string = ''

param lawCustomerId string = ''
@secure()
param lawCustomerKey string = ''

param appConfKvSecretUri string = ''
param appInsKvSecretUri string = ''
param lavalinkPasswordKvSecretUri string = ''

param storageAccountName string = ''
param shareName string = ''

var containerAppVolumeName = 'azurefiles'
var appConfDataReaderRole = resourceId(
  'Microsoft.Authorization/roleDefinitions',
  '516239f1-63e1-4d78-a4de-a74fb236a071'
)
var acrPullRole = resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
var kvSecretsUsersRole = resourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')

var lavalinkContainer = {
  image: lavalinkImage
  name: lavalinkContainerName
  env: [
    {
      name: 'LAVALINK_SERVER_PASSWORD'
      value: lavalinkConfig.server.password
    }
    {
      name: 'SERVER_ADDRESS'
      value: lavalinkConfig.server.address
    }
    {
      name: 'LAVALINK_SERVER_SOURCES_LOCAL'
      value: 'true'
    }
    {
      name: 'SERVER_PORT'
      value: lavalinkConfig.server.port
    }
  ]
  resources: {
    cpu: json('0.25')
    memory: '0.5Gi'
  }
  probes: []
  volumeMounts: [
    {
      volumeName: containerAppVolumeName
      mountPath: '/mount/${containerAppVolumeName}'
    }
  ]
}

var discordBotContainer = {
  name: discordBotContainerName
  image: discordBotImage
  env: [
    {
      name: 'Lavalink__Password'
      secretRef: 'lavalink-password'
    }
    {
      name: 'Lavalink__Host'
      value: containerAppLavalink.name
    }
    {
      name: 'Lavalink__Scheme'
      value: 'http'
    }
    {
      name: 'ConnectionStrings__AppConfig'
      secretRef: 'appconf-connectionstring'
    }
    {
      name: 'ConnectionStrings__AppInsights'
      secretRef: 'appinsights-connectionstring'
    }
  ]
  resources: {
    cpu: json('0.5')
    memory: '1Gi'
  }
  probes: []
  volumeMounts: [
    {
      volumeName: containerAppVolumeName
      mountPath: '/mount/${containerAppVolumeName}'
    }
  ]
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' existing = {
  name: storageAccountName
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-08-01-preview' = {
  name: managedEnvironmentsName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: lawCustomerId
        dynamicJsonColumns: true
        sharedKey: lawCustomerKey
      }
    }
  }

  resource azureFiles 'storages@2023-08-01-preview' = {
    name: containerAppVolumeName
    properties: {
      azureFile: {
        accountName: storageAccount.name
        accountKey: storageAccount.listKeys().keys[0].value
        shareName: shareName
        accessMode: 'ReadOnly'
      }
    }
  }
}

resource containerAppLavalink 'Microsoft.App/containerapps@2023-08-01-preview' = {
  name: caLavalinkName
  location: location
  tags: tags
  identity: {
    type: 'None'
  }
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    environmentId: containerAppEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 2333
        exposedPort: 2333
        transport: 'Tcp'
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
        allowInsecure: false
        stickySessions: {
          affinity: 'none'
        }
      }
    }
    template: {
      volumes: [
        {
          name: containerAppVolumeName
          storageType: 'AzureFile'
          storageName: containerAppEnvironment::azureFiles.name
        }
      ]
      containers: [
        lavalinkContainer
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

resource uaiCaDiscordBot 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' = {
  name: 'uai-${caDiscordBotName}'
  location: location
  tags: tags
}

resource containerAppDiscordBot 'Microsoft.App/containerapps@2023-08-01-preview' = {
  name: caDiscordBotName
  tags: tags
  identity: {
    userAssignedIdentities: {
      '${uaiCaDiscordBot.id}': {}
    }
    type: 'UserAssigned'
  }
  location: location
  dependsOn: [
    kvSecretsUsersAssignment
  ]
  properties: {
    environmentId: containerAppEnvironment.id
    configuration: {
      secrets: [
        {
          name: 'appconf-connectionstring'
          keyVaultUrl: appConfKvSecretUri
          identity: uaiCaDiscordBot.id
        }
        {
          name: 'lavalink-password'
          keyVaultUrl: lavalinkPasswordKvSecretUri
          identity: uaiCaDiscordBot.id
        }
        {
          name: 'appinsights-connectionstring'
          keyVaultUrl: appInsKvSecretUri
          identity: uaiCaDiscordBot.id
        }
      ]
      activeRevisionsMode: 'Single'
      registries: [
        {
          identity: uaiCaDiscordBot.id
          server: acrUri
        }
      ]
    }
    template: {
      containers: [
        discordBotContainer
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
      volumes: [
        {
          name: containerAppVolumeName
          storageType: 'AzureFile'
          storageName: containerAppEnvironment::azureFiles.name
        }
      ]
    }
  }
}

resource serviceCoAppConfig 'Microsoft.ServiceLinker/linkers@2022-11-01-preview' = {
  scope: containerAppDiscordBot
  name: 'serviceco_appconfig_b17cc'
  properties: {
    clientType: 'dotnet'
    targetService: {
      type: 'AzureResource'
      id: appConfId
    }
    authInfo: {
      authType: 'userAssignedIdentity'
      clientId: uaiCaDiscordBot.properties.clientId
      subscriptionId: subscription().subscriptionId
    }
    scope: 'discordbotabordelv2'
  }
}

resource appConfReaderAssignment 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid(resourceGroup().id, uaiCaDiscordBot.id, appConfDataReaderRole)
  properties: {
    roleDefinitionId: appConfDataReaderRole
    principalId: uaiCaDiscordBot.properties.principalId
  }
  scope: resourceGroup()
}

resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid(resourceGroup().id, uaiCaDiscordBot.id, acrPullRole)
  properties: {
    roleDefinitionId: acrPullRole
    principalId: uaiCaDiscordBot.properties.principalId
  }
  scope: resourceGroup()
}

resource kvSecretsUsersAssignment 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid(resourceGroup().id, uaiCaDiscordBot.id, kvSecretsUsersRole)
  properties: {
    roleDefinitionId: kvSecretsUsersRole
    principalId: uaiCaDiscordBot.properties.principalId
  }
  scope: resourceGroup()
}
