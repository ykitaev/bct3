using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using Configuration;

namespace Cruncher
{
    static class NetworkCoordinator
    {
        public static async Task UploadHopesBlobAsync(string fileName)
        {
            var storageAccount = CloudStorageAccount.Parse(File.ReadAllText(Constants.BlobConnectionStringFileName));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("hopes");
            var blockBlob = container.GetBlockBlobReference(fileName.Split('\\').Last());

            using (var fileStream = File.OpenRead(fileName))
            {
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
        }
    }
}
