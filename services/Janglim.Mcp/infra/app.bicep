param location string = resourceGroup().location
param appName string = 'janglim-mcp'
param environmentName string = 'janglim-mcp-env'
param registryName string = 'janglimmcp${uniqueString(resourceGroup().id)}'
param identityName string = 'janglim-mcp-pull'

@description('Immutable image reference in the dedicated registry, including its sha256 digest.')
param image string

@secure()
@minLength(32)
@maxLength(256)
param apiKey string

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: registryName
}
resource environment 'Microsoft.App/managedEnvironments@2025-07-01' existing = {
  name: environmentName
}
resource pullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: identityName
}

resource app 'Microsoft.App/containerApps@2025-07-01' = {
  name: appName
  location: location
  tags: {
    project: 'Janglim'
    component: 'MCP'
    managedBy: 'Bicep'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${pullIdentity.id}': {} }
  }
  properties: {
    environmentId: environment.id
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      secrets: [ { name: 'mcp-api-key', value: apiKey } ]
      registries: [
        { server: registry.properties.loginServer, identity: pullIdentity.id }
      ]
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
    }
    template: {
      containers: [
        {
          name: 'janglim-mcp'
          image: image
          resources: { cpu: 1, memory: '2Gi' }
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
            { name: 'ASPNETCORE_HTTP_PORTS', value: '8080' }
            { name: 'AllowedHosts', value: '${appName}.${environment.properties.defaultDomain};localhost;127.0.0.1;[::1]' }
            { name: 'JanglimMcp__ApiKey', secretRef: 'mcp-api-key' }
          ]
          probes: [
            { type: 'Startup', tcpSocket: { port: 8080 }, periodSeconds: 10, timeoutSeconds: 1, failureThreshold: 9 }
            { type: 'Readiness', tcpSocket: { port: 8080 }, periodSeconds: 5, timeoutSeconds: 1, failureThreshold: 3 }
            { type: 'Liveness', tcpSocket: { port: 8080 }, periodSeconds: 10, timeoutSeconds: 1, failureThreshold: 3 }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 2
        cooldownPeriod: 300
        rules: [
          { name: 'http-requests', http: { metadata: { concurrentRequests: '2' } } }
        ]
      }
    }
  }
}

output mcpUrl string = 'https://${app.properties.configuration.ingress.fqdn}/mcp'
output healthUrl string = 'https://${app.properties.configuration.ingress.fqdn}/health'
