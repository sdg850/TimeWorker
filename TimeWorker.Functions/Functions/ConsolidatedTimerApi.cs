using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using TimeWorker.Functions.Entities;
using TimeWorker.Commom.Responses;

namespace TimeWorker.Functions.Functions
{
    public static class ConsolidatedTimerApi
    {
        [FunctionName(nameof(GetAllItems))]
        public static async Task<IActionResult> GetAllItems(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidatedtimer")] HttpRequest req,
        [Table("Consolidatedtimer", Connection = "AzureWebJobsStorage")] CloudTable ConsolidatedTimerTable,
        ILogger log)
        {
            log.LogInformation($"Geting all the items...");

            TableQuery<ConsolidatedTimerEntity> query = new TableQuery<ConsolidatedTimerEntity>();
            TableQuerySegment<ConsolidatedTimerEntity> items = await ConsolidatedTimerTable.ExecuteQuerySegmentedAsync(query, null);




            string message = $"Showing all items...";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {

                isSuccess = true,
                Mesages = message,
                Result = items

            });
        }

        [FunctionName(nameof(GetItemById))]
        public static IActionResult GetItemById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "timeworker/{id}")] HttpRequest req,
        [Table("TimeWorker", "TimeWorker", "{id}", Connection = "AzureWebJobsStorage")] TimeWorkerEntity timeworkerEntity,
        string id,
        ILogger log)
        {
            log.LogInformation($"Geting id {id} from the table...");


            if (timeworkerEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    Mesages = "¡Ups looks like somthing went wrong, Id no found. Try again!"

                });

            }


            string message = $"Showing id result...";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {

                isSuccess = true,
                Mesages = message,
                Result = timeworkerEntity

            });
        }
    }
