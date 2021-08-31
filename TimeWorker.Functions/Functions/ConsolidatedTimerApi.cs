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
using TimeWorker.Commom.Models;
using System.Collections.Generic;

namespace TimeWorker.Functions.Functions
{
    public static class ConsolidatedTimerApi
    {
        [FunctionName(nameof(GetAllRecords))]
        public static async Task<IActionResult> GetAllRecords(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidatedtimer")] HttpRequest req,
        [Table("Consolidatedtimer", Connection = "AzureWebJobsStorage")] CloudTable ConsolidatedTimerTable,
        [Table("TimeWorker", Connection = "AzureWebJobsStorage")] CloudTable TimeWorkerTable,
        ILogger log)
        {
            log.LogInformation($"Geting all the items...");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Consolidatedtimer item = JsonConvert.DeserializeObject<Consolidatedtimer>(requestBody);



            //if (string.IsNullOrEmpty(item?.Id.ToString()))
            //{
            //    return new BadRequestObjectResult(new Response
            //    {
            //        isSuccess = false,
            //        Mesages = "Request must have a  Id of the employee"
            //    });
            //}

            //---------------------< calling TimeWorkerTable >------------------------------------------

            //TableQuery<TimeWorkerEntity> query = new TableQuery<TimeWorkerEntity>().Where(TableQuery.GenerateFilterConditionForInt("Id", QueryComparisons.Equal, item.Id));
            TableQuery<TimeWorkerEntity> query = new TableQuery<TimeWorkerEntity>();
            TableQuerySegment<TimeWorkerEntity> Result = await TimeWorkerTable.ExecuteQuerySegmentedAsync(query, null);
                               
            List<int> IdFinded = new List<int>();

            foreach (TimeWorkerEntity record in Result)
            {
                log.LogInformation($"---------------------->>>inside  results foreach...");

                if (!IdFinded.Contains(record.Id))
                {
                    TableQuery<TimeWorkerEntity> Query = new TableQuery<TimeWorkerEntity>().Where(TableQuery.GenerateFilterConditionForInt("Id", QueryComparisons.Equal, record.Id));                    
                    TableQuerySegment<TimeWorkerEntity> EmployeeResults = await TimeWorkerTable.ExecuteQuerySegmentedAsync(Query, null);

                    bool employeeType = false;
                    int IdRecord = 0;
                    DateTime EntryTime = new DateTime();
                    TimeSpan timer = new TimeSpan();

                    foreach (TimeWorkerEntity employeeResult in EmployeeResults)
                    {

                        log.LogInformation($"---------------------->>>inside  employeeResult foreach...");

                        if (employeeType && employeeResult.Type == "1")
                        {
                            
                            DateTime outputTime = employeeResult.CreatedTime;
                            timer.Add(EntryTime.Subtract(outputTime));
                            employeeType = !employeeType;

                            log.LogInformation($"timerrrr---------------------->>>{timer}...");
                        }

                        if (!employeeType && employeeResult.Type == "0")
                        {
                            EntryTime = employeeResult.CreatedTime;
                            employeeType = !employeeType;
                        }

                        IdRecord = employeeResult.Id;


                    }

                    
                    log.LogInformation($"---------------------->> timer : {timer.TotalHours}");
                    log.LogInformation($"---------------------->>>Saving timer...");

                    ConsolidatedTimerEntity consolidatedEntity = new ConsolidatedTimerEntity
                    {
                        PartitionKey = "CONSOLIDATED",
                        RowKey = DateTime.UtcNow.ToString().Replace("/", "-"),
                        ETag = "*",
                        id = IdRecord,
                        ExecutionDate = DateTime.UtcNow,
                        TimeWorked = timer.TotalHours.ToString()

                    };

                    TableOperation AddTableOperation = TableOperation.Insert(consolidatedEntity);
                    await ConsolidatedTimerTable.ExecuteAsync(AddTableOperation);

                    IdFinded.Add(record.Id);

                }

            }

            TableQuery<ConsolidatedTimerEntity> querySql = new TableQuery<ConsolidatedTimerEntity>();
            TableQuerySegment<ConsolidatedTimerEntity> consolidated = await ConsolidatedTimerTable.ExecuteQuerySegmentedAsync(querySql, null);

            string message = $"Showing all records...";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {

                isSuccess = true,
                Mesages = message,
                Result = consolidated

            });

        }

        [FunctionName(nameof(GetRecordsById))]
        public static IActionResult GetRecordsById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidatedtimer/{id}")] HttpRequest req,
        [Table("Consolidatedtimer", "TimeWorker", "{id}", Connection = "AzureWebJobsStorage")] TimeWorkerEntity ConsolidatedTimerTable,
        string id,
        ILogger log)
        {
            log.LogInformation($"Geting id {id} from the table...");


            if (ConsolidatedTimerTable == null)
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
                Result = ConsolidatedTimerTable

            });
        }
    }
}