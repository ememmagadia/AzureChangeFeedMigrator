using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MigratorNew
{
    public static class Migrator
    {
        [FunctionName("Migrator")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "QuickReachDbV1",
            containerName: "E3C09A54-5103-4D4A-93C7-65436BA34E3A",
            Connection = "CONNECTION_STRING_BETA",
            StartFromBeginning = true,
            CreateLeaseContainerIfNotExists = true,
            MaxItemsPerInvocation = 1000
            )] IReadOnlyList<Document> source,
                [CosmosDB(
                    databaseName: "QuickReachDbV2",
                    containerName: "E3C09A54-5103-4D4A-93C7-65436BA34E3A",
                    Connection = "CONNECTION_STRING_BETA" // In case your destination is on a different account, otherwise, it could be the same value as the Trigger
                )] IAsyncCollector<Document> destination, ILogger log)
        {
            try
            {

                if (source != null && source.Count > 0)
                {
                    foreach (var doc in source)
                    {
                        log.LogInformation("OLD :" + doc.ToString());
                        var ten = doc.GetPropertyValue<string>("tenantId");
                        if (string.IsNullOrEmpty(ten))
                        {
                            doc.SetPropertyValue("tenantId", "E3C09A54-5103-4D4A-93C7-65436BA34E3A");
                            //log.LogInformation("OLD :" + doc.ToString());
                        }
                        else
                        {
                            log.LogInformation("tenantId exist");
                        }

                        if (doc.GetPropertyValue<string>("id") != "BA444351-DA0A-4B1F-998D-A8BD0E23F3E7")
                        {
                            await destination.AddAsync(doc);
                            log.LogInformation("Success!");
                        }
                        else
                        {
                            log.LogInformation("same value");
                        }

                    }
                }
            }
            catch (Exception e)
            {
                log.LogInformation(e.Message);
                throw;
            }
        }
    }

    // Customize the model with your own desired properties
    //public class ToDoItem
    //{
    //    public string id { get; set; }
    //    public string Description { get; set; }
    //}
}
