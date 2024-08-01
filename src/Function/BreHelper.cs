using System;
using System.IO;
using System.Threading;
using Azure.Storage.Blobs;
using Microsoft.Azure.Workflows.RuleEngine;

namespace ProcessManager
{

    public static class BreHelper
    {

        public static DebugTrackingInterceptor GetTrackingInterceptor(string trackingFilePath)
        {

            DebugTrackingInterceptor ti = null;
            int attempts = 0;
            bool success = false;
            const int MaxAttempts = 3;
            const int DelayMilliseconds = 1000;

            while (attempts < MaxAttempts && !success)
            {
                try
                {
                    ti = new DebugTrackingInterceptor(trackingFilePath);
                    success = true; // If no exception is thrown, set success to true
                }
                catch (IOException ex)
                {
                    attempts++;
                    if (attempts >= MaxAttempts)
                    {
                        // If the maximum number of attempts is reached, rethrow the exception
                        throw new Exception("Failed to create DebugTrackingInterceptor after multiple attempts.", ex);
                    }
                    // Wait for a short delay before retrying
                    Thread.Sleep(DelayMilliseconds);
                }
            }

            return ti;
        }

        public static void CopyTrackingFileToBlob(string trackingFilePath)
        {
            // Retrieve the connection string for your Azure Storage account
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            // Create a BlobServiceClient object using the connection string
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Get a reference to the container where you want to store the file
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("bre-tracking");
            //create the container if it doesn't already exist
            containerClient.CreateIfNotExists();

            // Get the file name from the tracking file path
            string fileName = Path.GetFileName(trackingFilePath);
            string pathOnly = Path.GetDirectoryName(trackingFilePath);

            //generate a unique filename for temporary file - prevents file locked problems
            string tempFileName = Guid.NewGuid().ToString() + "_" + fileName;
            string tempFilePath = Path.Combine(pathOnly, tempFileName);
            File.Copy(trackingFilePath, tempFilePath, true);

            // Get a reference to the blob where you want to copy the file
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Upload the file to the blob
            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                blobClient.Upload(fileStream, true);
            }
        }
    }
}


