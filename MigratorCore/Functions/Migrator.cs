using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MigratorCore.Functions
{
    public static class Migrator
    {
        [FunctionName("Migrator")]
        //public static void Run([CosmosDBTrigger(
        //    databaseName: "databaseName",
        //    collectionName: "collectionName",
        //    ConnectionStringSetting = "",
        //    LeaseCollectionName = "leases")]IReadOnlyList<Document> input, ILogger log)
        //{
        //    if (input != null && input.Count > 0)
        //    {
        //        log.LogInformation("Documents modified " + input.Count);
        //        log.LogInformation("First document Id " + input[0].Id);
        //    }
        //}
        public static async Task Run([CosmosDBTrigger(
            databaseName: "QuickReachDbV1",
            containerName: "E3C09A54-5103-4D4A-93C7-65436BA34E3A",
            Connection = "CONNECTION_STRING_BETA",
            StartFromBeginning = true
            //CreateLeaseContainerIfNotExists = true
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
                        log.LogInformation(doc.ToString());
                        var ten = doc.GetPropertyValue<string>("tenantId");
                        if (string.IsNullOrEmpty(ten))
                        {
                            doc.SetPropertyValue("tenantId", "E3C09A54-5103-4D4A-93C7-65436BA34E3A");
                        }
                        else
                        {
                            log.LogInformation("tenantId exist");
                        }
                        await destination.AddAsync(doc);
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
}
