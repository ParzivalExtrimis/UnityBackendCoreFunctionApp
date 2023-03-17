
# UnityBackendCoreFunctionApp

A powerful and robust solution to manage users, institutions and provide a scalable and balanced storage pipeline. Allowing for the ease of having an out-of-the-box backend for large streamable files that need to delievered with minimized load on the host machine. Also provides state management for processes, as well as for active users.



## Getting Started

There are two main branches that are designed for testing on typical dev environments and preferences.

Note: If you do not have an Azure account then you can use  [Azurite with an HTTPS endpoint](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio#authorization-for-tools-and-sdks) to emulate
a storage account and a [CosmosDB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21) as an alternative. 

    Azure Development environment: Here it is assumed that you have access
        to an Azure account along with a storage account and a cosmos DB account
        ( any configuration should work ) but I have chosed a general purpose v2 
        for storage and default dedicated plan for cosmos. With that here's how
        to get started

            1. Clone the project into Visual Studio ( the simplest way as there is
               a .sln file included )
            2. If you want to use connection strings use the method below or if
               you want to use managed identities you can follow this [tutorial](https://learn.microsoft.com/en-us/azure/azure-functions/functions-identity-based-connections-tutorial)
               to set that up for yourself. For developmenet purposes I would suggest
               using connection strings, by moving onto step 3
            3. Add a local.settings.json file to the root project directory which 
                should look something like this:

```json
        {
            "IsEncrypted": false,
            "Values": {
                "AzureWebJobsStorage" : "UseDevelopmentStorage=true",
                "FUNCTIONS_WORKER_RUNTIME": "dotnet",
                "CosmosDBConnection": "AccountEndpoint=https://<your-cosmos>.documents.azure.com:443/;AccountKey=<your-key>;"
            },
            "ConnectionStrings": {
                "CloudStorageAccount": "DefaultEndpointsProtocol=https;AccountName=<your-storage>;AccountKey=<your-key>;EndpointSuffix=core.windows.net",
                "CosmosDBConnection": "<same-as-the-other-cosmos>"
            }
        }
 ```
           4. Replace the above slots with your own connection strings and your good to 
              go. Just make sure to tun dotnet restore on the project to resolve dependencies.
              Build and run.
           5. You should have a console output with a link to a locally hosted endpoint to
              initiate the process.  
           6. Send a post request to the link with a json body attatched like in the
              example below


## Example

![PostmanLocalRequest](https://user-images.githubusercontent.com/72618565/225881260-1ea7f29f-8a5c-44b3-baee-bfee3c5bfdaf.png)

Make sure to set Content-type to application/json in your headers

### You can Skip all of this and directly test the hosted app like this:

![vrBackendPostmanRemote](https://user-images.githubusercontent.com/72618565/225882719-600286fd-f1ec-45a8-89fe-730182fe49b8.png)

The only problem here is that the pipeline endpoint that is required to read from the user's temp container where the content is stored during the session is still in development. You will be able test it soon as it goes into release.



## 🔗 Links

### Also checkout other companion projects that expand on the capabilities of this app. 

* [Creator Utility Scripts](https://github.com/ParzivalExtrimis/Utility-Creator-Scripts)
Note that this project is still in development and is subject to changes. Checkback here to see many drastic changes and improvements coming soon. 

