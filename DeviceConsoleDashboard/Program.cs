using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Networking;

namespace DeviceConsoleDashboard
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Refreshing...");
                var devices = NetworkCoordinator.GetDevicesStatus().ToList();
                Console.Clear();
                var i = 1;
                foreach (var device in devices)
                {
                    var cutoffError = NetworkCoordinator.CheckInInterval + NetworkCoordinator.CheckInInterval + TimeSpan.FromMinutes(5);
                    var cutoffWarning = NetworkCoordinator.CheckInInterval + TimeSpan.FromMinutes(5);
                    var lastSeenAgo = DateTime.UtcNow - device.LastActiveUtc;
                    if (lastSeenAgo > cutoffError)
                        Console.ForegroundColor = ConsoleColor.Red;
                    else if (lastSeenAgo > cutoffWarning)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else
                        Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine("{0}. {1}. Last seen {2} minutes ago.", i, device.DeviceName, (int)lastSeenAgo.TotalMinutes);
                    ++i;
                }
                Task.Delay(TimeSpan.FromMinutes(5)).Wait();
            }
        }
    }
}
