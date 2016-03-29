using Microsoft.Azure.NotificationHubs;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessD2CInteractiveMessages
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Process D2C Interactive Messages app\n");

            string connectionString = "{service bus listen connection string}";
            QueueClient Client = QueueClient.CreateFromConnectionString(connectionString, "d2ctutorial");

            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            Client.OnMessage((message) =>
            {
                try
                {
                    var bodyStream = message.GetBody<Stream>();
                    bodyStream.Position = 0;
                    var bodyAsString = new StreamReader(bodyStream, Encoding.ASCII).ReadToEnd();

                    Console.WriteLine("Received message: {0} messageId: {1}", bodyAsString, message.MessageId);
                    SendNotificationAsync(bodyAsString);

                    message.Complete();
                }
                catch (Exception)
                {
                    Console.WriteLine("Message abandon!");
                    message.Abandon();
                }
            }, options);

            Console.WriteLine("Receiving interactive messages from SB queue...");
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        private static async void SendNotificationAsync(String notification)
        {
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString("<connection string with full access>", "<hub name>");
            var toast = @"<toast><visual><binding template=""ToastText01""><text id=""1"">" + notification + "</text></binding></visual></toast>";
            await hub.SendWindowsNativeNotificationAsync(toast);
        }
    }
}
