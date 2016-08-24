using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net;

namespace Cruncher
{
    static class NetworkCoordinator
    {
        public static async Task UploadHopesBlobAsync(string fileName)
        {
            var storageAccount = CloudStorageAccount.Parse(File.ReadAllText(Constants.AzureConnectionStringFileName));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("hopes");
            var blockBlob = container.GetBlockBlobReference(fileName.Split('\\').Last());

            using (var fileStream = File.OpenRead(fileName))
            {
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
        }

        public static async Task<int> GetNextFreeBatchIndex()
        {
            // This should probably be implmented as a tree so that a worker would try to grab a chunk from a least-busy segment
            // to avoid collisions, but if that starts to matter, then I'll gladly re-write this to accomodate hundreds of requestors..
            // For now, we can live with VERY rare Etag mismatches
            var map = await GetState();
            for (var i = 0; i < map.part1.Length; ++i)
            {
                if (map.part1[i] == (byte)BatchState.NotProcessed)
                {
                    if (await CheckOut(map, 1, i))
                    {
                        return i;
                    }
                }
            }

            for (var i = 0; i < map.part2.Length; ++i)
            {
                if (map.part2[i] == (byte)BatchState.NotProcessed)
                {
                    if (await CheckOut(map, 2, i))
                    {
                        return i;
                    }
                }
            }

            for (var i = 0; i < map.part3.Length; ++i)
            {
                if (map.part3[i] == (byte)BatchState.NotProcessed)
                {
                    if (await CheckOut(map, 3, i))
                    {
                        return i;
                    }
                }
            }

            // Wow all processed?
            throw new Exception("Looks like all batches are taken...");
        }

        private static async Task<bool> CheckOut(Map map, int part, int index)
        {
            try
            {
                if (part == 1)
                    map.part1[index] = (byte)BatchState.CheckedOut;
                else if (part == 2)
                    map.part2[index] = (byte)BatchState.CheckedOut;
                else if (part == 3)
                    map.part3[index] = (byte)BatchState.CheckedOut;
                else
                    throw new ArgumentException("Last time I checked there were only 3 parts in the Map...");

                var storageAccount = CloudStorageAccount.Parse(File.ReadAllText(Constants.AzureConnectionStringFileName));
                var tableClient = storageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference("bct3map1");

                TableOperation insertOperation = TableOperation.Merge(map);
                var res = await table.ExecuteAsync(insertOperation);
                if (res.HttpStatusCode == (int)HttpStatusCode.OK
                    || res.HttpStatusCode == (int)HttpStatusCode.NoContent)
                    return true;
                else
                    return false;
            }
            catch (StorageException e)
            {
                Console.WriteLine("Looks like a conflict: " + e.Message);
                return false;
            }
        }

        public static async Task MarkCrunched(int batch)
        {
            var done = false;
            while (!done)
            {
                try
                {
                    var map = await GetState();
                    if (batch < map.part1.Length)
                        map.part1[batch] = (byte)BatchState.Crunched;
                    else if (batch < (map.part1.Length + map.part2.Length))
                        map.part2[batch - map.part1.Length] = (byte)BatchState.Crunched;
                    else if (batch < (map.part1.Length + map.part2.Length + map.part3.Length))
                        map.part3[batch - map.part1.Length - map.part2.Length] = (byte)BatchState.Crunched;
                    else
                        throw new ArgumentException("Batch index out of range: " + batch);

                    
                    var storageAccount = CloudStorageAccount.Parse(File.ReadAllText(Constants.AzureConnectionStringFileName));
                    var tableClient = storageAccount.CreateCloudTableClient();
                    var table = tableClient.GetTableReference("bct3map1");

                    TableOperation insertOperation = TableOperation.Merge(map);
                    var res = await table.ExecuteAsync(insertOperation);
                    if (res.HttpStatusCode == (int)HttpStatusCode.OK
                        || res.HttpStatusCode == (int)HttpStatusCode.NoContent)
                        done = true;
                    else
                        done = false;
                }
                catch (StorageException e)
                {
                    Console.WriteLine("Looks like a conflict: " + e.Message);
                    done = false;
                }

                if (!done)
                {
                    Console.WriteLine("Ok, let's rest..."); // In case it's a coding bug...
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            }
        }

        public static async Task InitializeBlankState()
        {
            // Ideally this code should execute just once ever after it's tested
            var storageAccount = CloudStorageAccount.Parse(File.ReadAllText(Constants.AzureConnectionStringFileName));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("bct3map1");

            var p1 = new byte[60000];
            var p2 = new byte[60000];
            var p3 = new byte[56059];

            var blank = new Map()
            {
                part1 = p1,
                part2 = p2,
                part3 = p3,
            };

            TableOperation insertOperation = TableOperation.Insert(blank);
            await table.ExecuteAsync(insertOperation);
        }

        private static async Task<Map> GetState()
        {
            var storageAccount = CloudStorageAccount.Parse(File.ReadAllText(Constants.AzureConnectionStringFileName));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("bct3map1");
            TableOperation insertOperation = TableOperation.Retrieve<Map>("1", "1");
            var result = await table.ExecuteAsync(insertOperation);
            return (Map)result.Result;
        }

        private enum BatchState
        {
            NotProcessed = 0,
            CheckedOut = 1,
            Crunched = 2,
            Verified = 3,
        }

        private class Map : TableEntity
        {
            public Map()
            {
                this.PartitionKey = "1";
                this.RowKey = "1";
            }

            // Max number = 176058334 ~= 176059k

            // First 60k
            public byte[] part1 { get; set; }

            // Second 60k
            public byte[] part2 { get; set; }

            // Last 56059
            public byte[] part3 { get; set; }
        }
    }
}
