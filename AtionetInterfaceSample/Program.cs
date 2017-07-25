namespace AtionetInterfaceSample
{
    using System;
    using System.Threading.Tasks;
    using Ationet.Sdk.Constants;

    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => MakeCall());

            Console.ReadKey();
        }

        static async void MakeCall()
        {
            var ationetClient = new Ationet.Sdk.Interface.InterfaceOperations("https://native-beta.ationet.com", "{yourusername}", "{yourpassword}");

            var transactions = await ationetClient.GetTransactionsAsync("CH9", "SystemModel", "1.0", DateTime.Now.AddMonths(-2));

            if (transactions.Status != SdkResponseStatus.Ok)
            {
                Console.WriteLine("Status: {0}", transactions.Status);
                Console.WriteLine("Error Message: {0}", transactions.ErrorMessage);
            }
            else
            {
                foreach (var transaction in transactions.Content)
                {
                    Console.WriteLine("{0} - {1} - {2}", transaction.AuthorizationCode, transaction.TransactionAmountDispensed, transaction.DateTime);
                }
            }
        }
    }
}
