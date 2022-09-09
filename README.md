# Azure EventHub Demo

[![Run Build and Test](https://github.com/kolosovpetro/EventHubDemo.AZ204/actions/workflows/run-build-and-test-dotnet.yml/badge.svg)](https://github.com/kolosovpetro/EventHubDemo.AZ204/actions/workflows/run-build-and-test-dotnet.yml)

In this demo project an example of azure event hub utilizing is shown.
More precisely, we implement two subscriptions, one is subscription to azure blob storage event on behalf of azure
function,
another is to be subscription using ASP NET Core web application.

## Infrastructure provisioning

### Resource group

- Create resource group: `az group create --name "event-hub-demo-rg" --location "westus"`

### Storage Account & Container

- Create storage
  account: `az storage account create --name "storagepkolosov" --resource-group "event-hub-demo-rg" --location "westus" --sku "Standard_ZRS" --kind "StorageV2"`
- Create storage
  container: `az storage container create --name "my-files" --account-name "storagepkolosov" --public-access "blob"`

### Azure Function

- Create azure
  function: `az functionapp create --resource-group "event-hub-demo-rg" --name "functionpkolosov" --runtime "dotnet" --runtime-version "6.0" --os-type "Linux" --storage-account "storagepkolosov" --consumption-plan-location "westus"`
- Run in folder: `func init --worker-runtime dotnet --force`

### Event Grid

- Register event grid namespace: `az provider register --namespace Microsoft.EventGrid`
- Check registration status: `az provider show --namespace Microsoft.EventGrid --query “registrationState”`
- NOTE: enable event grid namespace possible as well from azure
  portal: `subscription -> resource providers (top down menu side)`
- Create azure event grid system
  topic: `az eventgrid system-topic create -g "event-hub-demo-rg" --name "topicpkolosov" --location "westus" --topic-type "microsoft.storage.storageaccounts" --source /subscriptions/{id}/resourceGroups/event-hub-demo-rg/providers/Microsoft.Storage/storageAccounts/storagepkolosov`
- Create event grid topic
  subscription: `az eventgrid system-topic event-subscription create --name "storage-function-subscription" --resource-group "event-hub-demo-rg" --system-topic-name "topicpkolosov" --endpoint-type "azurefunction"`