using System;
using stellar_dotnetcore_sdk;
using System.Threading.Tasks;

namespace StellarWallet
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            /*
            var keyPair = KeyPair.Random();
            Console.WriteLine(keyPair.Address);
            Console.WriteLine(keyPair.SecretSeed);
            */
            //var address = "GCKVX5H5XVVDE653PU7SWDDZEAMBHRY6ECTDVLRCFFHNP33R2JOLRPXC";
            var seed = "SBKDCHYZHNANMNPFUKFKDIAHEL2K3GLNKKRARQWSE67TTEHTGNGGRKKH";

            var serverUrl = "http://47.91.208.241:8000";

            var keyPair = KeyPair.FromSecretSeed(seed);
            var address = keyPair.Address;
            Console.WriteLine(address);

            var server = new Server(serverUrl);
            var account = await server.Accounts.Account(keyPair);
            Console.WriteLine("Balance for account: " + account.KeyPair.AccountId);
            foreach(var b in account.Balances)
            {
                Console.WriteLine("Type : {0} , Code: {1} , Balance: {2}",b.AssetType,b.AssetCode,b.BalanceString);
            }

        }
    }
}
