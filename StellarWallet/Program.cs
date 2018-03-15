using System;
using stellar_dotnetcore_sdk;
using System.Threading.Tasks;
using Nett;
using DocoptNet;

namespace StellarWallet
{

    class Program
    {
        private const string usage = @"

    Usage:
      StellarWallet.dll new_account
      StellarWallet.dll active_account <account_id>
      StellarWallet.dll check_balance <account_id>
      StellarWallet.dll pay <account_id> <amount>
      StellarWallet.dll (-h | --help)
      StellarWallet.dll --version

    Options:
      -h --help     Show this screen.
      --version     Show version.

    ";

        public static async Task Main(string[] args)
        {
            var stellar = new Stellar();
            stellar.InitFromConfigFile("config.tml");
            var arguments = new Docopt().Apply(usage, args, version: "StellarWallet 1.0", exit: true);
            foreach (var argument in arguments)
            {
                //Console.WriteLine("{0} = {1}", argument.Key, argument.Value);
            }
            if(arguments["new_account"].IsTrue)
            {
                var destAccount = stellar.RandomAccount();
                Console.WriteLine(destAccount.Address);
                Console.WriteLine(destAccount.SecretSeed);
                return;
            }
            if (arguments["check_balance"].IsTrue)
            {
                var destAccount = KeyPair.FromAccountId(arguments["<account_id>"].ToString());
                await stellar.GetAccountBalance(destAccount);
                return;
            }
            if (arguments["active_account"].IsTrue)
            {
                var destAccount = KeyPair.FromAccountId(arguments["<account_id>"].ToString());
                await stellar.CreateAccount(destAccount);
                return;
            }
            if (arguments["pay"].IsTrue)
            {
                var destAccount = KeyPair.FromAccountId(arguments["<account_id>"].ToString());
                var amount = arguments["<amount>"].ToString();
                await stellar.MakePayment(destAccount,amount);
                return;
            }
            return;
        }
    }
}
