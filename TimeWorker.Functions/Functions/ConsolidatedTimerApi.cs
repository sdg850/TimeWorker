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
using System.Linq;

namespace TimeWorker.Functions.Functions
{
    public static class ConsolidatedTimerApi
    {
        //GetAllRecords:: (req, ConsolidatedTimerTable, TimeWorkerTable, log ) --> Response
        [FunctionName(nameof(GetAllRecords))]
        public static async Task<IActionResult> GetAllRecords(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidatedtimer")] HttpRequest req,
        [Table("Consolidatedtimer", Connection = "AzureWebJobsStorage")] CloudTable ConsolidatedTimerTable,
        [Table("TimeWorker", Connection = "AzureWebJobsStorage")] CloudTable TimeWorkerTable,
        ILogger log)
        {
            log.LogInformation($"Geting all the items...");

            //---------------------< Calling all data from TimeWorkerTable >------------------------------------------
            
            TableQuery<TimeWorkerEntity> query = new TableQuery<TimeWorkerEntity>().Where(TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false)); ;
            TableQuerySegment<TimeWorkerEntity> Result = await TimeWorkerTable.ExecuteQuerySegmentedAsync(query, null);

            
            List<int> IdFinded = new List<int>(); //---------------------< Save data to verify that Id wasn´t process before
            List<TimeWorkerEntity> Consolidated = new List<TimeWorkerEntity>(); //---------------------< Save data to change consolidated boolean true at the end of the process

            foreach (TimeWorkerEntity record in Result)
            {
                if (!IdFinded.Contains(record.Id))
                {
                    //---------------------< Calling filter data from TimeWorkerTable >------------------------------------------
                    TableQuery<TimeWorkerEntity> Query = new TableQuery<TimeWorkerEntity>().Where(TableQuery.GenerateFilterConditionForInt("Id", QueryComparisons.Equal, record.Id));                    
                    TableQuerySegment<TimeWorkerEntity> EmployeeResults = await TimeWorkerTable.ExecuteQuerySegmentedAsync(Query, null);

                    bool isZero = false; //-----------------------> check Entry time
                    TimeWorkerEntity Employee = new TimeWorkerEntity();
                    TimeSpan timer = new TimeSpan();//------------>Counter timeWorked
                    bool hasOutput = false; //-----------------------> check Entry and Out time

                    foreach (TimeWorkerEntity employeeResult in EmployeeResults)
                    {
                        if (isZero 
                            && employeeResult.Type == "1" 
                            && employeeResult.Consolidated == false)
                        {
                            
                            DateTime outputTime = employeeResult.CreatedTime;
                            timer = timer + outputTime.Subtract(Employee.CreatedTime);
                            isZero = !isZero;
                            hasOutput = true;

                            Consolidated.Add(Employee);
                            Consolidated.Add(employeeResult);
                        }

                        if (!isZero 
                            && employeeResult.Type == "0" 
                            && employeeResult.Consolidated == false)
                        {
                            Employee = employeeResult;
                            isZero = !isZero;                            
                        }
                    }

                    //---------------------< Save data to ConsolidatedTimerTable >------------------------------------------
                    if (hasOutput)
                    {
                        string[] RowKey = DateTime.UtcNow.ToString().Replace("/", "").Replace(":", "").Replace(".", "").Split(' ');

                        ConsolidatedTimerEntity consolidatedEntity = new ConsolidatedTimerEntity
                        {
                            PartitionKey = record.Id.ToString(),
                            RowKey = RowKey[0],
                            ETag = "*",
                            id = record.Id,
                            ExecutionDate = DateTime.UtcNow,
                            TimeWorked = timer.TotalMinutes.ToString()

                        };

                        //-----------------------> check if Id already exist in the table ConsolidatedTimerTable and update or insert data --------------------------------------
                        TableOperation findOperation = TableOperation.Retrieve<ConsolidatedTimerEntity>(record.Id.ToString(), consolidatedEntity.RowKey);
                        TableResult findResult = await ConsolidatedTimerTable.ExecuteAsync(findOperation);

                        if (!(findResult.Result == null))
                        {
                            ConsolidatedTimerEntity consolidate = (ConsolidatedTimerEntity)findResult.Result;
                            TimeSpan time = timer + TimeSpan.FromMinutes(double.Parse(consolidate.TimeWorked));
                            consolidatedEntity.TimeWorked = time.TotalMinutes.ToString();

                        }

                        TableOperation AddTableOperation = TableOperation.InsertOrReplace(consolidatedEntity);
                        await ConsolidatedTimerTable.ExecuteAsync(AddTableOperation);

                        IdFinded.Add(record.Id);
                    }

                    //---------------------< Update consolidated field in TimeWorkerTable table >------------------------------------------
                    foreach (TimeWorkerEntity consolidate in Consolidated)
                    {
                        consolidate.Consolidated = true;
                        TableOperation AddTableOperation = TableOperation.Replace(consolidate);
                        await TimeWorkerTable.ExecuteAsync(AddTableOperation);
                    }

                }

            }

            //---------------------< Calling All records from ConsolidatedTimerTable >------------------------------------------
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

        //GetRecordsByDate:: (req, ConsolidatedTimerTable, ExecutionDate, log) --> Response
        [FunctionName(nameof(GetRecordsByDate))]
        public static async Task<IActionResult> GetRecordsByDate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidatedtimer/{ExecutionDate}")] HttpRequest req,
        [Table("Consolidatedtimer", Connection = "AzureWebJobsStorage")] CloudTable ConsolidatedTimerTable,
        DateTime ExecutionDate,
        ILogger log)
        {

            //---------------------< Calling all data from ConsolidatedTimerTable >------------------------------------------

            TableQuery<ConsolidatedTimerEntity> query = new TableQuery<ConsolidatedTimerEntity>();
            TableQuerySegment<ConsolidatedTimerEntity> ResultQuery = await ConsolidatedTimerTable.ExecuteQuerySegmentedAsync(query, null);

            if (ResultQuery == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    Mesages = "¡Ups looks like somthing went wrong, Date no found. Try again!"

                });
            }


            //---------------------< Filter DateTime records from  ConsolidatedTimerTable>------------------------------------------

            List<ConsolidatedTimerEntity> filter = new List<ConsolidatedTimerEntity>();
            string[] date = ExecutionDate.ToString().Split(' ');

            foreach (ConsolidatedTimerEntity result in ResultQuery)
            {
                if (result.ExecutionDate.ToString().Contains(date[0]))
                {
                    filter.Add(result);
                }

            }

            string message = $"¡Consolidated data got from {date} was succesfully!";

            return new OkObjectResult(new Response
            {

                isSuccess = true,
                Mesages = message,
                Result = filter

            });
        }
    }
}