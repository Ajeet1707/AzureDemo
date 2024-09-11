using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using System;

namespace FunctionApp1
{
	public class SubmitFeedback
	{

		private readonly ILogger<SubmitFeedback> _logger;

		public SubmitFeedback(ILogger<SubmitFeedback> log)
		{
			_logger = log;
		}

		//[FunctionName("FeedbackFunction")]
		//[OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
		//[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
		//[OpenApiParameter(name: "userId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **UserId** parameter")]
		//[OpenApiParameter(name: "createdAt", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **CreatedAt** parameter")]
		//[OpenApiParameter(name: "message", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Message** parameter")]
		//[OpenApiParameter(name: "rating", In = ParameterLocation.Query, Required = true, Type = typeof(int), Description = "The **Rating** parameter")]
		//[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
		//public async Task<IActionResult> Run(
		//	[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
		//	[ServiceBus("feedbackqueue", Connection = "ServiceBusConnection")] ICollector<dynamic> output)
		//{


		[FunctionName("FeedbackFunction")]
		[OpenApiOperation(operationId: "Run", tags: new[] { "Feedback" })]
		[OpenApiParameter(name: "userId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **UserId** parameter")]
		[OpenApiParameter(name: "message", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Message** parameter")]
		[OpenApiParameter(name: "rating", In = ParameterLocation.Query, Required = true, Type = typeof(int), Description = "The **Rating** parameter")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
		public async Task<IActionResult> Run(
				[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
				[Blob("feedbackfile", System.IO.FileAccess.Write, Connection = "AzureWebJobsStorage")] BlobContainerClient stream,
				[ServiceBus("feedbackqueue", Connection = "ServiceBusConnection")] ICollector<dynamic> output)
		{
			_logger.LogInformation("C# HTTP trigger function processed a request.");

			string userId = req.Query["userId"];
			string createdAt = req.Query["createdAt"];
			string msg = req.Query["message"];
			int rating = int.Parse(req.Query["rating"]);

			string fileName = $"{userId}.json";
			BlobClient blobClient = stream.GetBlobClient(fileName);
			if (await blobClient.ExistsAsync())
			{
				_logger.LogInformation($"File for User ID {userId} already exists. No new feedback added.");
				return new OkObjectResult(new { message = "Review already submitted" });
				//return new OkObjectResult(new { message = $"File for User ID { newFeedback.UserId} already exists. No new feedback added." });
			}

			var feedback = new Feedback()
			{
				UserId = userId,
				CreatedAt = DateTime.Now,
				Message = msg,
				Rating = rating,
			};

			output.Add(feedback);

			return new OkObjectResult(new { message = "Feedback received", feedback });
		}

		public class Feedback
		{
			public string UserId { get; set; }
			public string Message { get; set; }
			public int Rating { get; set; }
			public DateTime CreatedAt { get; set; }
		}

		public class Response
		{
			public string Message { get; set; }
		}
	}
}
