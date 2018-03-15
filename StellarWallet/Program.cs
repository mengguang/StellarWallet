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
                Console.WriteLine("Address:    " + destAccount.Address);
                Console.WriteLine("SecretSeed: " + destAccount.SecretSeed);
                return;
            }
            if (arguments["check_balance"].IsTrue)
            {
                KeyPair destAccount;
                try
                {
                    destAccount = KeyPair.FromAccountId(arguments["<account_id>"].ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Invalid account_id, please check it again.");
                    Console.WriteLine("Exception Message: " + e.Message);
                    return;
                }
                
                await stellar.GetAccountBalance(destAccount);
                return;
            }
            if (arguments["active_account"].IsTrue)
            {
                KeyPair destAccount;
                try
                {
                    destAccount = KeyPair.FromAccountId(arguments["<account_id>"].ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Invalid account_id, please check it again.");
                    Console.WriteLine("Exception Message: " + e.Message);
                    return;
                }
                await stellar.CreateAccount(destAccount);
                return;
            }
            if (arguments["pay"].IsTrue)
            {
                KeyPair destAccount;
                try
                {
                    destAccount = KeyPair.FromAccountId(arguments["<account_id>"].ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Invalid account_id, please check it again.");
                    Console.WriteLine("Exception Message: " + e.Message);
                    return;
                }
                var amount = arguments["<amount>"].ToString();
                await stellar.MakePayment(destAccount,amount);
                return;
            }
            return;
        }
    }
}
