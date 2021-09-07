using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TimeWorker.Commom.Models;
using TimeWorker.Functions.Functions;
using TimeWorker.Test.Helpers;
using Xunit;

namespace TimeWorker.Test.Tests
{
    public class ApiTestTimeworker
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateItem_Should_Return_200()
        {
            // Arrenge
            MockCloudTables mockTable = new MockCloudTables(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Timeworker Request = TestFactory.GetRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(Request);

            // Act
            IActionResult response = await TimeWorkerApi.CreateItem(request, mockTable, logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void UpdateItemById_Should_Return_200()
        {
            // Arrenge
            MockCloudTables mock = new MockCloudTables(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Timeworker Request = TestFactory.GetRequest();
            int Id = 98763850;
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(Id, Request);

            // Act
            IActionResult response = await TimeWorkerApi.UpdateItem(request, mock, Id.ToString(), logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }


    }
}
