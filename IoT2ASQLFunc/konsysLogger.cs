//  nuget install : System.Data.SqlClient
//  Microsoft.Azure.WebJobs.Extensions.EventHubs --version 4.0.1
//  paket add Microsoft.Azure.WebJobs.Extensions.ServiceBus --version 3.1.1
//  Event que triggered function !
using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data;
using System;

namespace IoT2ASQLFunc
{
    public static class konsysLogger
    {
        private static HttpClient client = new HttpClient();

        /// <summary>
        ///  messages/events
        /// </summary>
        /// <param name="message"></param>
        /// <param name="log"></param>
        [FunctionName("konsysLogger")]
        public static async void Run([IoTHubTrigger("konsyshub", Connection = "iothub")]EventData message, ILogger log)
        {
            string messageBody = Encoding.UTF8.GetString(message.Body.Array);
            log.LogInformation($"C# IoT Hub trigger function processed a message: {messageBody}");


            // Get the connection string from app settings and use it to create a connection.
         
            var str = System.Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                //conn.Execute("INSERT INTO [dbo].[messages] ([Msg]) VALUES (@Log)", messageBody);
                conn.Open();
                var text = "INSERT INTO [dbo].[messages] ([Msg], [msgdate]) VALUES (@Msg, @eventdate) " ;
               // var text = "UPDATE SalesLT.SalesOrderHeader " + "SET [Status] = 5  WHERE ShipDate < GetDate();";

                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    cmd.Parameters.Add("@Msg", SqlDbType.VarChar);
                    cmd.Parameters["@Msg"].Value = messageBody;
                    cmd.Parameters.Add("@eventdate", SqlDbType.DateTime);
                    cmd.Parameters["@eventdate"].Value = DateTime.Now;
                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were inserted");
                }
            }
         
           
        }


        /*
        [FunctionName("EventHubTriggerCSharp")]
        public static void Run(
    [EventHubTrigger("samples-workitems", Connection = "EventHubConnectionAppSetting")] EventData myEventHubMessage,
    DateTime enqueuedTimeUtc,
    Int64 sequenceNumber,
    string offset,
    ILogger log)
        {
            log.LogInformation($"Event: {Encoding.UTF8.GetString(myEventHubMessage.Body)}");
            // Metadata accessed by binding to EventData
            log.LogInformation($"EnqueuedTimeUtc={myEventHubMessage.SystemProperties.EnqueuedTimeUtc}");
            log.LogInformation($"SequenceNumber={myEventHubMessage.SystemProperties.SequenceNumber}");
            log.LogInformation($"Offset={myEventHubMessage.SystemProperties.Offset}");
            // Metadata accessed by using binding expressions in method parameters
            log.LogInformation($"EnqueuedTimeUtc={enqueuedTimeUtc}");
            log.LogInformation($"SequenceNumber={sequenceNumber}");
            log.LogInformation($"Offset={offset}");
        }
        */
    }





}