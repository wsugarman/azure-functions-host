﻿// Use storage v12
#r "Azure.Storage.Blobs"

using Azure.Storage.Blobs;

public static void Run(string input, ILogger log)
{
    // it is enough to just reference the type; compilation should fail
    BlobClient blobClient;
    log.LogInformation(input);
}