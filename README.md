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

### Create System Topic

- Create event grid topic subscription using Azure Portal: `Event Grid System Topics (search) -> Add`

### Create custom topic

- Create custom
  topic: `az eventgrid topic create --name "pkolosovcustomtopic" --resource-group "event-hub-demo-rg" --location "centralus"`
- Keep topic endpoint: `https://pkolosovcustomtopic.centralus-1.eventgrid.azure.net/api/events`
- Keep topic key: `bXJtk8I3t/Shd21ePMDP9UHpT0+SZb0xHtzVje5t8AE=`
- Create new event grid subscription using Azure Portal, use ngrok webhook

### Create web hook using Ngrok

- URL: `https://ngrok.com/`
- `ngrok http --host-header=localhost:44233 44233`
- `ngrok http --host-header=localhost:20222 20222`

### Important note: now event grid subs do not support ngrok self signed certificates, deploy app manually

- Create app service
  plan: `az appservice plan create --name "pkolosovserviceplan" --resource-group "event-hub-demo-rg" --sku "F1"`
- List available runtimes: `az webapp list-runtimes`
- Create web
  app: `az webapp create --resource-group "event-hub-demo-rg" --name "wephookapipkolosov" --plan "pkolosovserviceplan" --runtime "dotnet:6"`
- Create web
  app: `az webapp create --resource-group "event-hub-demo-rg" --name "corewebhookapipkolosov" --plan "pkolosovserviceplan" --runtime '"DOTNETCORE|3.1"'`
- Url: `https://wephookapipkolosov.azurewebsites.net`
- Publish: `dotnet publish --configuration Release --output .\bin\publish`
- Create artifact: `Compress-Archive .\bin\publish\* .\app.zip -Force`
- Deploy:
  `az webapp deployment source config-zip --resource-group "event-hub-demo-rg" --src "app.zip" --name "wephookapipkolosov"`
- View app service logs: `az webapp log tail --name "wephookapipkolosov" --resource-group "event-hub-demo-rg"`

### Drop resource group

- `az group delete --name "event-hub-demo-rg"`

### How to handle validation request upon event grid subscription

- `https://github.com/MicrosoftDocs/azure-docs/issues/87276`
