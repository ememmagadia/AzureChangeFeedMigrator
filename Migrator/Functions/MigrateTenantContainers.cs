using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Migrator.Functions
{

    public static class MigrateTenantContainers
    {

        [FunctionName("MigrateTenantContainers")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "QuickReachDbV1",
            containerName: "E3C09A54-5103-4D4A-93C7-65436BA34E3A",
            Connection = "CONNECTION_STRING_BETA",
            StartFromBeginning = true
            //CreateLeaseContainerIfNotExists = true
            )] IReadOnlyList<Document> source,
                [CosmosDB(
                    databaseName: "QuickReachDbV3",
                    containerName: "E3C09A54-5103-4D4A-93C7-65436BA34E3A",
                    Connection = "CONNECTION_STRING_BETA_NEW" // In case your destination is on a different account, otherwise, it could be the same value as the Trigger
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
                        if (ten.IsNull())
                        {
                            doc.SetPropertyValue("tenantId", "E3C09A54-5103-4D4A-93C7-65436BA34E3A");
                        } else
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
