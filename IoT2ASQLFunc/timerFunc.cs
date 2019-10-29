using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace IoT2ASQLFunc
{
    public static  class timerFunc
    {
        [FunctionName("timerFunc")]
        public static async void Run([TimerTrigger("*/15 * * * * *")]TimerInfo myTimer, ILogger log)
        {  /// every 15 sec   cron:sec min hour day month year
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Get the connection string from app settings and use it to create a connection.
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var text = "UPDATE messages " +
                        "SET [Status] = 5  ;";

                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were updated");
                }
            }
        }
    }


}
