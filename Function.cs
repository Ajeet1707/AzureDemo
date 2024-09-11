using System;
using System.IO;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FunctionApp1
{
    public class Function
    {
        [FunctionName("Function")]
		public async Task Run([ServiceBusTrigger("feedbackqueue", Connection = "ServiceBusConnection")] string myQueueItem,
							[Blob("feedbackfile", System.IO.FileAccess.Write, Connection = "AzureWebJobsStorage")] BlobContainerClient stream,
							ILogger log)
		{
			log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

			Feedback newFeedback = JsonSerializer.Deserialize<Feedback>(myQueueItem);
			
			// Create the file name based on the userId
			string fileName = $"{newFeedback.UserId}.json";

			// Check if the blob (file) exists
			BlobClient blobClient = stream.GetBlobClient(fileName);
			if (await blobClient.ExistsAsync())
			{
				log.LogInformation($"File for User ID {newFeedback.UserId} already exists. No new feedback added.");
				throw new Exception("Review already submitted");
				//return new OkObjectResult(new { message = $"File for User ID { newFeedback.UserId} already exists. No new feedback added." });
			}

			using (var memoryStream = new MemoryStream())
			{
				await JsonSerializer.SerializeAsync(memoryStream, newFeedback);
				memoryStream.Position = 0; // Reset stream position for uploading

				await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = "application/json" });
				log.LogInformation($"Feedback for User ID {newFeedback.UserId} has been added.");
				
			}
		}

		public class Feedback
		{
			public string UserId { get; set; }
			public string Message { get; set; }
			public int Rating { get; set; }
			public DateTime CreatedAt { get; set; }
		}
	}
}
