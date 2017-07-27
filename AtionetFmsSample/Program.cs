using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ationet.Native.Model.JsonObjects.FMS;
using Ationet.Sdk.Constants;

namespace AtionetFmsSample
{
    class Program
    {
        static void Main(string[] args)
        {
        }
        static async void MakeCall()
        {
            var ationetClient = new Ationet.Sdk.FMS.FMSOperations("https://native-beta.ationet.com", "{yourusername}", "{yourpassword}");

            var deliveries = new List<FMSDeliveryData>();

            var response = await ationetClient.UploadNativeDeliveriesAsync("CH9", deliveries, "SystemModel", "1.0");

            if (response.Status != SdkResponseStatus.Ok)
            {
                Console.WriteLine("Delivery uploaded succesfully");
            }
            else
            {
                Console.WriteLine("Error uploading delivery - " + response.ErrorMessage);
            }
        }
    }
}
