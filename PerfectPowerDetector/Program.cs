﻿namespace PerfectPowerDetector
{
    using Configuration;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    partial class Program
    {
        private static Random r = new Random();
        private static DateTime startTime = DateTime.Now;

        /// <summary>
        /// Scans a file and checks if it has any perfect powers
        /// If if fines any, writes a, x, b, and y to dreams.txt
        /// </summary>
        /// <param name="args">Not used for now</param>
        /// <remarks>Code copied over from my work on this back in 2013, so the coding style might be different</remarks>
        static void Main(string[] args)
        {
            boot();
            var cnt = 0;

            // First, check the files locally on disk, if there are any files
            var fileNames = new List<string>()
            {
               //@"M:\temp\hopes-bloom-102000.txt",
            };

            foreach (var fileName in fileNames)
            {
                ProcessFile(fileName);
                ++cnt;
            }

            // Then, check the blob storage
            var fromBlob = GetNewHopesFromBlobs();
            foreach (var fileName in fromBlob)
            {
                ProcessFile(fileName);
                ++cnt;
            }

            Console.WriteLine("Done. {0} files checked. Press any key to quit...", cnt);
            Console.ReadKey();
        }

        private static List<string> GetNewHopesFromBlobs()
        {
            // Decided not to do "yield return" for now because I want all my blobs downloaded and extracted first
            var newFiles = new List<string>();
            var storageAccount = CloudStorageAccount.Parse(File.ReadAllText(Constants.AzureConnectionStringFileName));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("hopes");
            BlobContinuationToken continuationToken = null;
            do
            {
                var segment = container.ListBlobsSegmented(continuationToken);
                continuationToken = segment.ContinuationToken;
                foreach (var item in segment.Results)
                {
                    var blob = (CloudBlockBlob)item;
                    // blob.FetchAttributes();
                    var name = blob.Name;
                    var textFileName = name.Replace(".zip", ".txt");
                    var textFilePath = Constants.root + @"\" + textFileName;
                    if (File.Exists(textFilePath))
                    {
                        Console.WriteLine("File '{0}' already exists, skipping", textFileName);
                        continue;
                    }

                    using (var stream = blob.OpenRead())
                    {
                        using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                        {
                            archive.Entries.Single().ExtractToFile(textFilePath);
                        }
                    }

                    newFiles.Add(textFilePath);
                }
            }
            while (continuationToken != null);

            return newFiles;
        }

        private static void ProcessFile(string fileName)
        {
            var errors = 0;
            Console.WriteLine("Checking file " + fileName);
            var cnt = 0;
            var lines = File.ReadAllLines(fileName);
            foreach (var l in lines)
            {
                var parts = l.Split('\t');
                if (parts.Length != 4)
                {
                    Console.WriteLine("Invalid line format, skipping!");
                    ++errors;
                    break;
                }

                var a = int.Parse(parts[0]);
                var x = int.Parse(parts[1]);
                var b = int.Parse(parts[2]);
                var y = int.Parse(parts[3]);

                var cz = BigInteger.Pow(a, x) + BigInteger.Pow(b, y);
                var res = isSuitablePower(cz);
                if (res)
                {
                    Console.WriteLine("*** Wow, something passed our filter!");
                    var str = l + Environment.NewLine;
                    File.AppendAllText(Constants.DreamsFileName, str);
                }

                cnt++;
                if (cnt % 100000 == 0)
                {
                    Console.WriteLine("Done {0}/{1}", cnt, lines.Count());
                }
            }

            if (errors > 5)
            {
                throw new Exception("Too many errors, this is strange: " + errors);
            }
        }
    }
}
