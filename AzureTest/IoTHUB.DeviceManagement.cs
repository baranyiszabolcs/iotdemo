using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace IoTHub.DeviceManagement
{
    /// <summary>
    /// Result of function
    /// </summary>
    public class Result
    {
        public bool Success { get; set; }
        public bool ExceptionError { get; set; }
        public string ErrorMessage { get; set; }
        public string PrimaryKey { get; set; }
        public string Status { get; set; }
        public string ConnectionState { get; set; }
        public string LastActivityTime { get; set; }
    }

    /// <summary>
    /// IoT Device properties
    /// </summary>
    public class IoTDeviceProperty
    {
        public string DeviceId { get; set; }
        public string Status { get; set; }
        public string ConnectionState { get; set; }
        public string LastActivityTime { get; set; }
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


        public async Task<Result> AddNewAsync(string DeviceId)
        {
            Result res = new Result();
            Device device = new Device();

            try
            {
                var registryManager = RegistryManager.CreateFromConnectionString(connectionstring);
                device = await registryManager.AddDeviceAsync(new Microsoft.Azure.Devices.Device(DeviceId));

                res.PrimaryKey = device.Authentication.SymmetricKey.PrimaryKey;
                res.Status = device.Status.ToString();
                res.ConnectionState = device.ConnectionState.ToString();
                res.LastActivityTime = device.LastActivityTime.ToString();
                res.Success = true;
            }
            catch (Exception ex)
            {
                res.ExceptionError = true;
                res.ErrorMessage = ex.Message;
            }

            return res;
        }

        public async Task<Result> GetInfoAsync(string DeviceId)
        {
            Result res = new Result();
            Device device = new Device();

            try
            {
                var registryManager = RegistryManager.CreateFromConnectionString(connectionstring);
                device = await registryManager.GetDeviceAsync(DeviceId);

                res.PrimaryKey = device.Authentication.SymmetricKey.PrimaryKey;
                res.Status = device.Status.ToString();
                res.ConnectionState = device.ConnectionState.ToString();
                res.LastActivityTime = device.LastActivityTime.ToString();
                res.Success = true;
            }
            catch (Exception ex)
            {
                res.ExceptionError = true;
                res.ErrorMessage = ex.Message;
            }

            return res;
        }

        public async Task<Result> DeleteAsync(string DeviceId)
        {
            Result res = new Result();
            Device device = new Device();

            try
            {
                var registryManager = RegistryManager.CreateFromConnectionString(connectionstring);
                device = await registryManager.GetDeviceAsync(DeviceId);

                await registryManager.RemoveDeviceAsync(device);
                res.Success = true;
            }
            catch (Exception ex)
            {
                res.ExceptionError = true;
                res.ErrorMessage = ex.Message;
            }

            return res;
        }

        public async Task<Result> SetStatusAsync(string DeviceId, bool Status)
        {
            Result res = new Result();
            Device device = new Device();

            try
            {
                var registryManager = RegistryManager.CreateFromConnectionString(connectionstring);
                device = await registryManager.GetDeviceAsync(DeviceId);

                if (Status == true)
                {
                    device.Status = DeviceStatus.Enabled;
                }
                else
                {
                    device.Status = DeviceStatus.Disabled;
                }
                await registryManager.UpdateDeviceAsync(device);

                res.PrimaryKey = device.Authentication.SymmetricKey.PrimaryKey;
                res.Status = device.Status.ToString();
                res.ConnectionState = device.ConnectionState.ToString();
                res.LastActivityTime = device.LastActivityTime.ToString();
                res.Success = true;
            }
            catch (Exception ex)
            {
                res.ExceptionError = true;
                res.ErrorMessage = ex.Message;
            }

            return res;
        }

        public async Task<List<IoTDeviceProperty>> GetListOfDevicesAsync()
        {

            List<IoTDeviceProperty> listofdevices = new List<IoTDeviceProperty>();

            try
            {
                var registryManager = RegistryManager.CreateFromConnectionString(connectionstring);

                var query = registryManager.CreateQuery("select * from devices");

                while (query.HasMoreResults)
                {
                    IEnumerable<Twin> twins = await query.GetNextAsTwinAsync().ConfigureAwait(false);

                    foreach (Twin twin in twins)
                    {
                        var deviceprop = new IoTDeviceProperty();
                        deviceprop.DeviceId = twin.DeviceId;
                        deviceprop.Status = twin.Status.ToString();
                        deviceprop.ConnectionState = twin.ConnectionState.ToString();
                        deviceprop.LastActivityTime = twin.LastActivityTime.ToString();

                        listofdevices.Add(deviceprop);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            
            return listofdevices;
        }
    }   
}
