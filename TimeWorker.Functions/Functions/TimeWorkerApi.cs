using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using TimeWorker.Commom.Models;
using TimeWorker.Commom.Responses;
using TimeWorker.Functions.Entities;

namespace TimeWorker.Functions.Functions
{
    public static class TimeWorkerApi
    {
        [FunctionName(nameof(CreateItem))]
        public static async Task<IActionResult> CreateItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "timeworker")] HttpRequest req,
            [Table("TimeWorker", Connection = "AzureWebJobsStorage")] CloudTable timeWorkerTable,
            ILogger log)
        {
            log.LogInformation("your request was successfulled.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Timeworker item = JsonConvert.DeserializeObject<Timeworker>(requestBody);



            if (string.IsNullOrEmpty(item?.Id.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    Mesages = "Request must have a  Id of the employee"
                });
            }

            TimeWorkerEntity timeworkerEntity = new TimeWorkerEntity
            {
                PartitionKey = "TimeWorker",
                RowKey = DateTime.UtcNow.ToString().Replace("/", "-"),
                ETag = "*",
                Id = item.Id,
                CreatedTime = DateTime.UtcNow,
                Type = "1",
                Consolidated = false
            };

            //---------------------< Check if Type field was stored before >------------------------------------------

            TableQuery<TimeWorkerEntity> query = new TableQuery<TimeWorkerEntity>().Where(TableQuery.GenerateFilterConditionForInt("Id", QueryComparisons.Equal, item.Id));
            TableQuerySegment<TimeWorkerEntity> Result = await timeWorkerTable.ExecuteQuerySegmentedAsync(query, null);

            string lastType = "";
            //string type = "1";

            foreach (TimeWorkerEntity record in Result)
            {
                lastType = record.Type;
            };



            if (string.IsNullOrEmpty(lastType) || lastType == "1")
            {
                timeworkerEntity.Type = "0";
            }

            //----------------<  store new record  >---------------------------------------------------------------------



            TableOperation AddTableOperation = TableOperation.Insert(timeworkerEntity);
            await timeWorkerTable.ExecuteAsync(AddTableOperation);

            string message = "Todo was storaged inside the table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {

                isSuccess = true,
                Mesages = message,
                Result = Result

            });

        }


        [FunctionName(nameof(UpdateItem))]
        public static async Task<IActionResult> UpdateItem(
                [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "timeworker/{id}")] HttpRequest req,
                [Table("TimeWorker", Connection = "AzureWebJobsStorage")] CloudTable TimeWorkerTable,
                string id,
                ILogger log)
        {
            log.LogInformation($"your {id} was received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Timeworker item = JsonConvert.DeserializeObject<Timeworker>(requestBody);

            //Check todo id

            TableOperation findOperation = TableOperation.Retrieve<TimeWorkerEntity>("TimeWorker", id);

            log.LogInformation($"YOur FindOperation is {findOperation}.");

            TableResult findResult = await TimeWorkerTable.ExecuteAsync(findOperation);

            log.LogInformation($"your Result is {findResult}.");


            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    Mesages = "¡Ups looks like somthing went wrong, Id no found. Try again!"

                });

            }

            //Update Todo
            TimeWorkerEntity timeworkerEntity = (TimeWorkerEntity)findResult.Result;


            if (!string.IsNullOrEmpty(item.CreatedTime.ToString()))
            {
                timeworkerEntity.CreatedTime = item.CreatedTime;
            }


            TableOperation AddTableOperation = TableOperation.Replace(timeworkerEntity);
            await TimeWorkerTable.ExecuteAsync(AddTableOperation);

            string message = $"Employee: {id} was updated";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {

                isSuccess = true,
                Mesages = message,
                Result = timeworkerEntity

            });
        }

        [FunctionName(nameof(GetAllItems))]
        public static async Task<IActionResult> GetAllItems(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "timeworker")] HttpRequest req,
        [Table("TimeWorker", Connection = "AzureWebJobsStorage")] CloudTable TimeWorkerTable,
        ILogger log)
        {
            log.LogInformation($"Geting all the items...");

            TableQuery<TimeWorkerEntity> query = new TableQuery<TimeWorkerEntity>();
            TableQuerySegment<TimeWorkerEntity> items = await TimeWorkerTable.ExecuteQuerySegmentedAsync(query, null);




            string message = $"Showing all items...";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {

                isSuccess = true,
                Mesages = message,
                Result = items

            });
        }

        //[FunctionName(nameof(GetItemById))]
        //public static IActionResult GetItemById(
        //[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "timeworker/{id}")] HttpRequest req,
        //[Table("TimeWorker", "TimeWorker", "{id}", Connection = "AzureWebJobsStorage")] TimeWorkerEntity timeworkerEntity,
        //string id,
        //ILogger log)
        //{
        //    log.LogInformation($"Geting id {id} from the table...");


        //    if (timeworkerEntity == null)
        //    {
        //        return new BadRequestObjectResult(new Response
        //        {
        //            isSuccess = false,
        //            Mesages = "¡Ups looks like somthing went wrong, Id no found. Try again!"

        //        });

        //    }


        //    string message = $"Showing id result...";
        //    log.LogInformation(message);

        //    return new OkObjectResult(new Response
        //    {

        //        isSuccess = true,
        //        Mesages = message,
        //        Result = timeworkerEntity

        //    });
        //}

        //[FunctionName(nameof(DeleteItemById))]
        //public static async Task<IActionResult> DeleteItemById(
        //[HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "timeworker/{id}")] HttpRequest req,
        //[Table("TimeWorker", "TimeWorker", "{id}", Connection = "AzureWebJobsStorage")] TimeWorkerEntity timeworkerEntity,
        //[Table("TimeWorker", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
        //string id,
        //ILogger log)
        //{
        //    log.LogInformation($"Deleting id {id} from the table...");


        //    if (timeworkerEntity == null)
        //    {
        //        return new BadRequestObjectResult(new Response
        //        {
        //            isSuccess = false,
        //            Mesages = "¡Ups looks like somthing went wrong, Id no found. Try again!"

        //        });

        //    }

        //    await todoTable.ExecuteAsync(TableOperation.Delete(timeworkerEntity));


        //    string message = $"Showing id deleted...";
        //    log.LogInformation(message);

        //    return new OkObjectResult(new Response
        //    {

        //        isSuccess = true,
        //        Mesages = message,
        //        Result = timeworkerEntity

        //    });
        //}
    }
}
