using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Azure.Devices.Client;
//using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Azure.Devices;
using System.Threading.Tasks;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AzureTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string HUBownerConnectionString = @"HostName=bsziothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=zhEb4IJwxkJptoepoguOGb/YLLJLTj42lvjff38wxPo=";
        string IOTDeviceConnectionString = @"HostName=bsziothub.azure-devices.net;DeviceId=bszuwp;SharedAccessKey=SJXEKntewdKYI/oRIAVGr4qoaky/qC0FyJMkIiK5gOs=";

        int _messageId = 0;

          DeviceClient  iotDev;
          RegistryManager registryManager;


        public MainPage()
        {
            this.InitializeComponent();
            this.InitClient();
            iotDev.OpenAsync(); // regisztálni kell a Device Twin eseménykezelő callback et
            iotDev.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyUpdateAsync, this);
            // ugyanígy a metod kezelőt is:  A "MyMethodAsync"  és "Konsys"  hvásra reagál
            iotDev.SetMethodHandlerAsync("MyMethodAsync", MyMethodAsync, null);
            iotDev.SetMethodHandlerAsync("Konsys",KonsysAsync, null);
        }

        public  void InitClient()
        {
            try
            {
                messageBlock.Text += "Connecting to hub \r\n";
                iotDev = DeviceClient.CreateFromConnectionString(IOTDeviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                registryManager  = RegistryManager.CreateFromConnectionString(HUBownerConnectionString);
            }
            catch (Exception ex)
            {
                messageBlock.Text = ex.Message;
            }
        }


    
        private async void SendDataToAzure_Click(object sender, RoutedEventArgs e)
        {
            Random rdd = new Random();
            double currentTemperature = 20 + rdd.NextDouble() * 30;
         
        
         //  itt kreálom ameg a device üzenetét
            var telemetryDataPoint = new
            {
                messageId = _messageId++,
                temperature = parameter.Text,
                deviceMessageTime = DateTime.Now
            };
            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));
            /// massage propety  minta
            message.Properties.Add("forras", "SendDataToAzure_Click");
            // összeállított üzenetet beküldöm
            await iotDev.SendEventAsync(message);
        }

        private async void AddNewDevices_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Device kdev = new Device(parameter.Text);
                registryManager.AddDeviceAsync(kdev).Wait();
                RefreshDeveiceList();
            } catch (Exception ex)
            {
                messageBlock.Text = ex.Message;
            }
        }

        private async void GetDevice_Click(object sender, RoutedEventArgs e)
        {
            RefreshDeveiceList();
        }

        private async void RefreshDeveiceList()
        {
            try
            {
                // device idk lekérdezése
                var query = registryManager.CreateQuery(
                            "SELECT * FROM devices", 100);
                var mydevces = await query.GetNextAsTwinAsync();
                messageBlock.Text = "Devices :\r\n" +
                  string.Join("\r\n", mydevces.Select(t => t.DeviceId));
            }
            catch (Exception ex)
            {
                messageBlock.Text = ex.Message;
            }
        }

        private async void DeleteDevice_Click(object sender, RoutedEventArgs e)
        {
              try
            {
                Device dev = await registryManager.GetDeviceAsync(deviceid.Text);
                registryManager.RemoveDeviceAsync(dev).Wait();
                RefreshDeveiceList();
            }
            catch (Exception ex)
            {
                messageBlock.Text = ex.Message;
            }
        }


        private async void GetDeviceTwin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Twin mytwin = await iotDev.GetTwinAsync();
                StringBuilder sb = new StringBuilder("TWIN:");
                sb.AppendFormat("ID: {0}", mytwin.DeviceId)
                     .AppendLine(" twin reported:" + mytwin.Properties.Reported.ToJson())
                     .AppendLine(" twin desired :" + mytwin.Properties.Desired.ToJson());
                messageBlock.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                messageBlock.Text = ex.Message;
            }
        }



        private async void ReportDeviceTwin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TwinCollection reportedProperties, repcount;
                reportedProperties = new TwinCollection();
                repcount = new TwinCollection();
                repcount["cnt"] = _messageId;
                reportedProperties["repcount"] = repcount;
                await iotDev.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                messageBlock.Text= ex.Message;
            }
        }




        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            messageBlock.Text = "";
        }

        public async Task OnDesiredPropertyUpdateAsync(TwinCollection desiredProperties, object userContext)
        {
            //  desired property json minta:
            //{
            //    "properties": {
            //        "desired": { "barcodePort": 2}
            //    }
            //}
            string str = desiredProperties.ToJson();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.messageBlock.Text = str;
            });
          
        }

        public async Task<MethodResponse> MyMethodAsync(MethodRequest methodRequest, object userContext)
        {
           string str = methodRequest.DataAsJson;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.messageBlock.Text =str;
            });
        
            return null;
        }

        public Task<MethodResponse> KonsysAsync(MethodRequest methodRequest, object userContext)
        {
            Application.Current.Exit();
            return null;
        }

   
    }
}
