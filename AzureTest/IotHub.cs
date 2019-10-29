using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;

namespace IotHub
{
    /// <summary>
    /// Transport types supported by DeviceClient.
    /// </summary>
    public enum CloudTransports
    {
        AMQP,
        AMQP_TCP_ONLY,
        AMQP_WEBSOCKET_ONLY,
        HTTP1,
        MQTT,
        MQTT_TCP_ONLY,
        MQTT_WEBSOCKET_ONLY
    }

    /// <summary>
    /// Result of function
    /// </summary>
    public class Result
    {
        public bool Success { get; set; }
        public bool ExceptionError { get; set; }
        public string ErrorMessage { get; set; }
    }

    class IoTDevice
    {
        private string connectionstring;
        /// <summary>
        /// Connection string based on primary key used in API calls which allows device to communicate with Iot Hub
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return connectionstring;
            }
            set
            {
                connectionstring = value;
            }
        }

        private CloudTransports cloudtransporttype;
        /// <summary>
        /// Used transport type
        /// </summary>
        public CloudTransports CloudTransportType
        {
            get
            {
                return cloudtransporttype;
            }
            set
            {
                cloudtransporttype = value;
            }
        }

        private Microsoft.Azure.Devices.Client.TransportType GetTransportType(CloudTransports TransTypeName)
        {
            switch (TransTypeName)
                {
                case CloudTransports.AMQP:
                    return Microsoft.Azure.Devices.Client.TransportType.Amqp;
                case CloudTransports.AMQP_TCP_ONLY:
                    return Microsoft.Azure.Devices.Client.TransportType.Amqp_Tcp_Only;
                case CloudTransports.AMQP_WEBSOCKET_ONLY:
                    return Microsoft.Azure.Devices.Client.TransportType.Amqp_WebSocket_Only;
                case CloudTransports.HTTP1:
                    return Microsoft.Azure.Devices.Client.TransportType.Http1;
                case CloudTransports.MQTT:
                    return Microsoft.Azure.Devices.Client.TransportType.Mqtt;
                case CloudTransports.MQTT_TCP_ONLY:
                    return Microsoft.Azure.Devices.Client.TransportType.Mqtt_Tcp_Only;
                case CloudTransports.MQTT_WEBSOCKET_ONLY:
                    return Microsoft.Azure.Devices.Client.TransportType.Mqtt_WebSocket_Only;
                default:
                    return Microsoft.Azure.Devices.Client.TransportType.Mqtt;
            }
        }

        public async Task<Result> SendMessageDeviceToCloudAsync(string Message)
        {
            Result res = new Result();

            try
            {
                using (var deviceclient = DeviceClient.CreateFromConnectionString(connectionstring, GetTransportType(cloudtransporttype)))
                {
                    if (deviceclient != null)
                    {
                        var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(Message));
                        await deviceclient.SendEventAsync(message);
                        res.Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                res.ExceptionError = true;
                res.ErrorMessage = ex.Message;
            }
            
            return res;
        }

    }
}
