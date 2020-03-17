using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;

namespace WaterExtAF
{
    public static class WaterMeasure
    {
        // Name of function is waterMeasure
        //  Sample hoto call it from  browser  when you run it from visualstudio debugger   http://localhost:7071/api/waterMeasure?day=3/12/2020  

        [FunctionName("waterMeasure")]
        public static async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",       
                Route = null)]HttpRequest req,
           [CosmosDB(
                databaseName: "iothub-stream",
                collectionName: "telemetry",
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client,    // cosmosdb connection string defined in   local.settings.json
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var searchterm = req.Query["day"];
            log.LogInformation($"Searching for: {searchterm}");
            
            // in our current example query  all day data.    input parameter of get id the   day =3/12/2020  dd/mm/yyyy
            if (string.IsNullOrWhiteSpace(searchterm))
            {
                return (ActionResult)new NotFoundResult();
            }
            DateTime searchdate; 
            if (!DateTime.TryParse(searchterm, out searchdate))
                searchdate = DateTime.Now;

            // we need an URal to the collection
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("iothub-stream", "telemetry");

            string strd = searchdate.ToString("yyyy-mm-dd");
            // specify Cosmos  SQL  query with paremeter
            var query = new SqlQuerySpec(
                "SELECT * FROM  telemetry where telemetry.EventEnqueuedUtcTime>=@stime and telemetry.EventEnqueuedUtcTime<=@etime  ",
                new SqlParameterCollection(new SqlParameter[] {
                    new SqlParameter { Name = "@stime", Value = searchdate},
                    new SqlParameter { Name = "@etime", Value = searchdate.AddDays(1)} }));

            dynamic documnets = client.CreateDocumentQuery<dynamic>(collectionUri, query,
                new FeedOptions { EnableCrossPartitionQuery = true }).AsEnumerable()   ;  //.FirstOrDefault();
           
            dynamic retarray = new JArray();
            foreach (var result in documnets)
            {
                string jsonString = JsonConvert.SerializeObject(result);
                //  here you can see the wholejson document do whatever want.
                retarray.Add(jsonString);
                //log.LogInformation(result.DevID);
            }
            return (ActionResult)new ObjectResult(retarray);

        }

    }

}
