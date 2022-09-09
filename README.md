# Azure EventHub Demo

Well, actually it is event grid demo but let's forget about it.

[![Run Build and Test](https://github.com/kolosovpetro/EventHubDemo.AZ204/actions/workflows/run-build-and-test-dotnet.yml/badge.svg)](https://github.com/kolosovpetro/EventHubDemo.AZ204/actions/workflows/run-build-and-test-dotnet.yml)

In this demo project an example of azure event grid utilizing is shown.
More precisely, we implement two subscriptions, one is subscription to azure blob storage event on behalf of azure
function,
another is to be subscription using ASP NET Core web application.

## Infrastructure provisioning

### Resource group

- Create resource group: `az group create --name "event-hub-demo-rg" --location "westus"`

### Storage Account & Container

- Create storage
  account: `az storage account create --name "storagepkolosov" --resource-group "event-hub-demo-rg" --location "centralus" --sku "Standard_ZRS" --kind "StorageV2"`
- Create storage
  container for
  images: `az storage container create --name "my-images" --account-name "storagepkolosov" --public-access "blob"`
- Create storage container for
  grayscale: `az storage container create --name "grayscale" --account-name "storagepkolosov" --public-access "blob"`

### Azure Function

- Create azure
  function: `az functionapp create --resource-group "event-hub-demo-rg" --name "functionpkolosov" --runtime "dotnet" --runtime-version "6" --functions-version "4" --os-type "Linux" --storage-account "storagepkolosov" --consumption-plan-location "centralus"`
- Run in folder: `mkdir AzureFunction && cd AzureFunction && func init --worker-runtime dotnet --force`
- Create function: `func new --template "HTTP trigger" --name "Grayscale"`
- Build & Start function: `func start --build`
- Install nuget packages to function project:
    - `dotnet add package Azure.Storage.Blobs`
    - `dotnet add package Microsoft.Azure.WebJobs.Extensions.EventGrid`
    - `dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage`
    - `dotnet add package SixLabors.ImageSharp`
- Deploy function using CLI: `func azure functionapp publish functionpkolosov`

### Event Grid

- Register event grid namespace: `az provider register --namespace Microsoft.EventGrid`
- Check registration status: `az provider show --namespace Microsoft.EventGrid --query "registrationState"`
- NOTE: enable event grid namespace possible as well from azure
  portal: `subscription -> resource providers (top down menu side)`
- Create azure event grid system
  topic: `az eventgrid system-topic create -g "event-hub-demo-rg" --name "topicpkolosov" --location "centralus" --topic-type "microsoft.storage.storageaccounts" --source "/subscriptions/{id}/resourceGroups/event-hub-demo-rg/providers/Microsoft.Storage/storageAccounts/storagepkolosov"`
- Create event grid topic subscription using Azure Portal: `Event Grid System Topics (search) -> Add`

### Create web hook using Ngrok
- URL: `https://ngrok.com/`
- `ngrok http --host-header=localhost:44233 44233`