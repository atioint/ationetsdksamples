namespace AtionetAuthSample
{
    using System;
    using System.Globalization;
    using Ationet.Native.Model.JsonObjects.Auth;
    using Ationet.Sdk.Constants;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => MakeCall());

            Console.ReadKey();
        }

        static async void MakeCall()
        {
            ////First of all, we create anAuthOperation object to interact with Ationet Authorization Engine
            var ationetSdk = new Ationet.Sdk.Auth.AuthOperations("https://native-beta.ationet.com");

            var executeCompletion = false;

            ////Since the SendPreAuthorization() method receives a AuthTransactionRequest object, we create one.
            var preAuthRequest = new AuthTransactionRequest
            {
                DeviceTypeIdentifier = "4", //// Required Field (indicates the terminal type (IndoorPaymentTerminal = "1" / OutdoorPaymentTerminal = "2" / CardReaderInDispenser = "3" / OtherSelfService = "4")) 
                ApplicationType = Ationet.Native.Model.Constants.ApplicationType.FleetControlSystem,
                ////Required field (use suggested value)
                ProcessingMode = Ationet.Native.Model.Constants.ProcessingMode.HostProcessingRequired,
                ////Required field (use suggested value)
                MessageFormatVersion = Ationet.Native.Model.Constants.MessageVersion.AuthMessageVersion,
                ////Required field (use suggested value)
                EntryMethod = Ationet.Native.Model.Constants.EntryMethod.Unknown,
                ////Required field (use suggested value)
                AccountType = Ationet.Native.Model.Constants.AccountType.ProprietaryGlobalFleetCard,
                ////Required field (use suggested value)
                TerminalIdentification = "CH905ZJPV7LK", ////Required field (this value is configured in the portal)
                SystemModel = "SystemModel", ////Required field (Third party app name or description)
                SystemVersion = "1.0", ////Required field (Third party app version)
                PrimaryTrack = "B3CF6C7AC98FD590", ////Required field (track2 information for primary identification)
                TransactionSequenceNumber = 51, //// A sequence number that the terminal needs to store and handle
                LocalTransactionDate =
                    int.Parse(
                        $"{DateTime.Now.Year}{DateTime.Now.Month.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}{DateTime.Now.Day.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}"),
                //// YYYYMMDD
                LocalTransactionTime =
                    int.Parse(
                        $"{DateTime.Now.Hour.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}{DateTime.Now.Minute.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}{DateTime.Now.Second.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}")
                //// HHMMSS
            };

            //// Sends the Pre Auth to the host
            var preAuthResponse = await ationetSdk.SendPreAuthorizationAsync(preAuthRequest);

            //// First of all we check if the response was proccessed OK (does NOT mean the transaction was approved)
            if (preAuthResponse.Status != SdkResponseStatus.Ok)
            {
                Console.WriteLine("Status: {0}", preAuthResponse.Status);
                Console.WriteLine("Error Message: {0}", preAuthResponse.ErrorMessage);
            }
            else
            {
                //// We check if the transaction was approved
                if (preAuthResponse.Content.ResponseCode == "00000")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("APPROVED");
                    Console.ResetColor();
                    executeCompletion = true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DECLINED");
                    Console.ResetColor();
                }

                Console.WriteLine("Status: {0}", preAuthResponse.Status);
                Console.WriteLine("Auth Code: {0}", preAuthResponse.Content.AuthorizationCode);
                Console.WriteLine("Response Code: {0}", preAuthResponse.Content.ResponseCode);
                Console.WriteLine("Response Text: {0}", preAuthResponse.Content.ResponseText);
                Console.WriteLine("");
            }

            if (!executeCompletion)
            {
                Console.WriteLine("Pre Auth not Approved");
                Console.Read();

                return;
            }

            Console.WriteLine("Completion [C] - Cancel [X]");
            Console.WriteLine("");


            var action = Console.ReadKey();

            ////If Send Completion is selected, we send the completion to Ationet, if NOT, the transaction is cancelled
            if (action.Key == ConsoleKey.C)
            {
                ////Since the SendConfirmation() method receives a AuthTransactionRequest object, we create one.
                var completionRequest = new AuthTransactionRequest
                {
                    ApplicationType = Ationet.Native.Model.Constants.ApplicationType.FleetControlSystem,
                    ////Required field (use suggested value)
                    ProcessingMode = Ationet.Native.Model.Constants.ProcessingMode.HostProcessingRequired,
                    ////Required field (use suggested value)
                    MessageFormatVersion = Ationet.Native.Model.Constants.MessageVersion.AuthMessageVersion,
                    ////Required field (use suggested value)
                    EntryMethod = Ationet.Native.Model.Constants.EntryMethod.Unknown,
                    ////Required field (use suggested value)
                    AccountType = Ationet.Native.Model.Constants.AccountType.ProprietaryGlobalFleetCard,
                    ////Required field (use suggested value)
                    TerminalIdentification = "CH905ZJPV7LK", ////Required field (this value is configured in the portal)
                    SystemModel = "SystemModel", ////Required field (Third party app name or description)
                    SystemVersion = "1.0", ////Required field (Third party app version)
                    PrimaryTrack = "B3CF6C7AC98FD590",
                    ////Required field (track2 information for primary identification)
                    TransactionSequenceNumber = preAuthRequest.TransactionSequenceNumber,
                    ////Required field (track2 information for primary identification)

                    LocalTransactionDate =
                        int.Parse(
                            $"{DateTime.Now.Year}{DateTime.Now.Month.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}{DateTime.Now.Day.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}"),
                    //// YYYYMMDD
                    LocalTransactionTime =
                        int.Parse(
                            $"{DateTime.Now.Hour.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}{DateTime.Now.Minute.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}{DateTime.Now.Second.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}"),
                    //// HHMMSS
                    ////ProductAmount = 99, 
                    ProductUnitPrice = decimal.Parse("1.23"),
                    ProductQuantity = decimal.Parse("9.82"),
                    AuthorizationCode = preAuthResponse.Content.AuthorizationCode
                    ////Required field (use the Authorization Code returned in the Pre Auth)
                };

                //// Sends the Completion to the host
                var completionResponse = await ationetSdk.SendConfirmationAsync(completionRequest);

                if (completionResponse.Status != SdkResponseStatus.Ok)
                {
                    Console.WriteLine("Status: {0}", completionResponse.Status);
                    Console.WriteLine("Error Message: {0}", completionResponse.ErrorMessage);
                }
                else
                {
                    //// We check if the completion was approved (can be rejected also)
                    if (completionResponse.Content.ResponseCode == "00000")
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("APPROVED");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("DECLINED");
                        Console.ResetColor();
                    }

                    Console.WriteLine("Status: {0}", completionResponse.Status);
                    Console.WriteLine("Auth Code: {0}", completionResponse.Content.AuthorizationCode);
                    Console.WriteLine("Response Code: {0}", completionResponse.Content.ResponseCode);
                    Console.WriteLine("Response Text: {0}", completionResponse.Content.ResponseText);
                }
            }
            else
            {
                var cancellationRequest = new AuthTransactionRequest
                {
                    ApplicationType = Ationet.Native.Model.Constants.ApplicationType.FleetControlSystem,
                    ////Required field (use suggested value)
                    ProcessingMode = Ationet.Native.Model.Constants.ProcessingMode.HostProcessingRequired,
                    ////Required field (use suggested value)
                    MessageFormatVersion = Ationet.Native.Model.Constants.MessageVersion.AuthMessageVersion,
                    ////Required field (use suggested value)
                    EntryMethod = Ationet.Native.Model.Constants.EntryMethod.Unknown,
                    ////Required field (use suggested value)
                    AccountType = Ationet.Native.Model.Constants.AccountType.ProprietaryGlobalFleetCard,
                    ////Required field (use suggested value)
                    TerminalIdentification = "CH905ZJPV7LK", ////Required field (this value is configured in the portal)
                    SystemModel = "SystemModel", ////Required field (Third party app name or description)
                    SystemVersion = "1.0", ////Required field (Third party app version)
                    TransactionSequenceNumber = preAuthRequest.TransactionSequenceNumber,
                    ////Required field (track2 information for primary identification)

                    OriginalData = new Dictionary<string, string>
                    {
                        {"TransactionCode", preAuthRequest.TransactionCode},
                        {"LocalTransactionDate", preAuthRequest.LocalTransactionDate.ToString()},
                        {"LocalTransactionTime", preAuthRequest.LocalTransactionTime.ToString()}
                    },
                    LocalTransactionDate = preAuthRequest.LocalTransactionDate, //// YYYYMMDD
                    LocalTransactionTime = preAuthRequest.LocalTransactionTime, //// HHMMSS
                };

                var cancelationResponse = ationetSdk.SendCancellation(cancellationRequest);

                if (cancelationResponse.Status != SdkResponseStatus.Ok)
                {
                    Console.WriteLine("Status: {0}", cancelationResponse.Status);
                    Console.WriteLine("Error Message: {0}", cancelationResponse.ErrorMessage);
                }
                else
                {
                    Console.WriteLine("Status: {0}", cancelationResponse.Status);
                    Console.WriteLine("Auth Code: {0}", cancelationResponse.Content.AuthorizationCode);
                    Console.WriteLine("Response Code: {0}", cancelationResponse.Content.ResponseCode);
                    Console.WriteLine("Response Text: {0}", cancelationResponse.Content.ResponseText);
                }
            }
        }
    }
}
