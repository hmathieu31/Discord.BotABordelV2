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

@description('Path to the Lavalink image.')
param lavalinkImage string = 'ghcr.io/lavalink-devs/lavalink:4.0.0-beta.4'

@description('Name of the DiscordBot image.')
param discordBotImage string = 'discordbotabordelv2:v2.1.0'

@secure()
@description('Discord Bot Token')
param discordBotToken string

@description('Lavalink Internal config')
param lavalinkConfig object = {
  server: {
    password: 'youshallnotpass'
    address: 'lavalink'
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

var containerAppVolumeName = 'azurefiles'
var lavalinkContainerName = 'lavalink'
var discordBotContainerName = 'discordbotabordelv2'

var appConfDataReaderRole = resourceId('Microsoft.Authorization/roleDefinitions', '516239f1-63e1-4d78-a4de-a74fb236a071')
var acrPullRole = resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-08-01-preview' = {
  name: managedEnvironmentsName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        dynamicJsonColumns: true
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }

  resource azureFiles 'storages@2023-08-01-preview' = {
    name: 'azurefiles'
    properties: {
      azureFile: {
        accountName: storageAccount.name
        accountKey: storageAccount.listKeys().keys[0].value
        shareName: storageAccount::fileService::shares.name
        accessMode: 'ReadOnly'
      }
    }
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
        {
          image: lavalinkImage
          name: lavalinkContainerName
          env: [
            {
              name: 'LAVALINK_SERVER_PASSWORD'
              value: lavalinkConfig.server.password
            }
            {
              name: 'SERVER_ADDERSS'
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
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

resource containerAppDiscordBot 'Microsoft.App/containerapps@2023-08-01-preview' = {
  name: caDiscordBotName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    environmentId: containerAppEnvironment.id
    configuration: {
      secrets: [
        {
          name: 'appconf-connectionstring'
          value: appConfig.listKeys().value[0].value
        }
        {
          name: 'discordbot-token'
          value: discordBotToken
        }
        {
          name: 'lavalink-password'
          value: lavalinkConfig.server.password
        }
      ]
      activeRevisionsMode: 'Single'
      registries: [
        {
          identity: 'system'
          server: containerRegistry.properties.loginServer
        }
      ]
    }
    template: {
      containers: [
        {
          name: discordBotContainerName
          image: '${containerRegistry.properties.loginServer}/${discordBotImage}'
          env: [
            {
              name: 'DiscordBot__Token'
              secretRef: 'discordbot-token'
            }
            {
              name: 'Lavalink__Password'
              secretRef: 'lavalink-password'
            }
            {
              name: 'Lavalink__Host'
              value: containerAppLavalink.properties.outboundIpAddresses[0]
            }
            {
              name: 'ConnectionStrings__AppConfig'
              secretRef: 'appconf-connectionstring'
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
      id: appConfig.id
    }
    authInfo: {
      authType: 'userAssignedIdentity'
      clientId: uaiCaDiscordBot.properties.clientId
      subscriptionId: subscription().subscriptionId
    }
    scope: 'discordbotabordelv2'
  }
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: laWorkspaceName
  tags: tags
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    workspaceCapping: {
      dailyQuotaGb: json('0,1')
    }
    retentionInDays: 30
  }
}

resource appConfReader 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid(resourceGroup().id, containerAppDiscordBot.id, appConfDataReaderRole)
  properties: {
    roleDefinitionId: appConfDataReaderRole
    principalId: containerAppDiscordBot.identity.principalId
  }
  scope: resourceGroup()
}

resource acrPull 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid(resourceGroup().id, containerAppDiscordBot.id, acrPullRole)
  properties: {
    roleDefinitionId: 'roleDefinitionId'
    principalId: 'principalId'
    principalType: 'ServicePrincipal'
  }
}

