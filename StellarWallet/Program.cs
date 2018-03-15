using System;
using stellar_dotnetcore_sdk;
using System.Threading.Tasks;
using Nett;
using DocoptNet;

namespace StellarWallet
{
    class WalletConfig
    {
        public string NetworkPassphrase { get; set; }
        public string HorizonUrl { get; set; }
        public string AccountSeed { get; set; }
    }
    class Program
    {
        private const string usage = @"

    Usage:
      stellarwallet new_account
      stellarwallet active_account <account_id>
      stellarwallet check_balance <account_id>
      stellarwallet pay <account_id> <amount>
      stellarwallet (-h | --help)
      stellarwallet --version

    Options:
      -h --help     Show this screen.
      --version     Show version.

    ";
        private static string NetworkPassphrase = "";
        private static string seed = "";
        private static string horizonUrl = "";

        private static bool ReadWalletConfig()
        {
            try
            {
                var config = Toml.ReadFile<WalletConfig>("config.tml");
                NetworkPassphrase = config.NetworkPassphrase;
                seed = config.AccountSeed;
                horizonUrl = config.HorizonUrl;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

        }

        public static async Task Main(string[] args)
        {
            if (ReadWalletConfig() == false)
            {
                return;
            }
            var arguments = new Docopt().Apply(usage, args, version: "StellarWallet 1.0", exit: true);
            foreach (var argument in arguments)
            {
                Console.WriteLine("{0} = {1}", argument.Key, argument.Value);
            }
            if(arguments["new_account"].IsTrue)
            {
                var destAccount = KeyPair.Random();
                Console.WriteLine(destAccount.Address);
                Console.WriteLine(destAccount.SecretSeed);
                return;
            }
            if (arguments["check_balance"].IsTrue)
            {
                var destAccount = KeyPair.FromAccountId(arguments["<account_id>"].ToString());
                await GetAccountBalance(destAccount);
                return;
            }
            if (arguments["active_account"].IsTrue)
            {
                var destAccount = KeyPair.FromAccountId(arguments["<account_id>"].ToString());
                await CreateAccount(destAccount);
                return;
            }
            if (arguments["pay"].IsTrue)
            {
                var destAccount = KeyPair.FromAccountId(arguments["<account_id>"].ToString());
                var amount = arguments["<amount>"].AsInt;
                await MakePayment(destAccount.AccountId,amount);
                return;
            }
            return;
        }

        private static async Task GetAccountBalance(KeyPair keyPair)
        {
            var server = new Server(horizonUrl);
            var account = await server.Accounts.Account(keyPair);
            Console.WriteLine("Balance for account: " + account.KeyPair.AccountId);
            foreach (var b in account.Balances)
            {
                Console.WriteLine("Type : {0} , Code: {1} , Balance: {2}", b.AssetType, b.AssetCode, b.BalanceString);
            }
        }

        private static async Task MakePayment(string destAccountID,int amount)
        {
            Network.Use(new Network(NetworkPassphrase));
            var destAccountKeyPair = KeyPair.FromAccountId(destAccountID);

            var server = new Server(horizonUrl);
            var destAccount = server.Accounts.Account(destAccountKeyPair);
            var sourceKeypair = KeyPair.FromSecretSeed(seed);
            var sourceAccountResp = await server.Accounts.Account(sourceKeypair);
            var sourceAccount = new Account(sourceKeypair, sourceAccountResp.SequenceNumber);
            var operation = new PaymentOperation.Builder(destAccountKeyPair, new AssetTypeNative(), amount.ToString()).Build();
            var transaction = new Transaction.Builder(sourceAccount).AddOperation(operation).AddMemo(Memo.Text("sample payment")).Build();
            transaction.Sign(sourceKeypair);

            try
            {
                var resp = await server.SubmitTransaction(transaction);
                if (resp.IsSuccess())
                {
                    Console.WriteLine("transaction completed successfully!");
                    await GetAccountBalance(destAccountKeyPair);
                }
                else
                {
                    Console.WriteLine("transaction failed.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async Task CreateAccount(KeyPair destAccount)
        {
            Network.Use(new Network(NetworkPassphrase));
            Console.WriteLine(destAccount.Address);
            //Console.WriteLine(destAccount.SecretSeed);
            var server = new Server(horizonUrl);
            var sourceKeypair = KeyPair.FromSecretSeed(seed);
            var sourceAccountResp = await server.Accounts.Account(sourceKeypair);
            var sourceAccount = new Account(sourceKeypair, sourceAccountResp.SequenceNumber);
            var operation = new CreateAccountOperation.Builder(destAccount, "20").SetSourceAccount(sourceKeypair).Build();
            var transaction = new Transaction.Builder(sourceAccount).AddOperation(operation).AddMemo(Memo.Text("Hello Memo")).Build();
            transaction.Sign(sourceKeypair);

            try
            {
                var resp = await server.SubmitTransaction(transaction);
                if (resp.IsSuccess())
                {
                    Console.WriteLine("transaction completed successfully!");
                    await GetAccountBalance(destAccount);
                }
                else
                {
                    Console.WriteLine("transaction failed.");
                    var c = resp.SubmitTransactionResponseExtras.ExtrasResultCodes;
                    Console.WriteLine(c.TransactionResultCode);
                    foreach(var x in c.OperationsResultCodes)
                    {
                        Console.WriteLine(x);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public static async Task CreateRandomAccount()
        {
            Network.Use(new Network(NetworkPassphrase));
            var destAccount = KeyPair.Random();
            Console.WriteLine(destAccount.Address);
            Console.WriteLine(destAccount.SecretSeed);
            var server = new Server(horizonUrl);
            var sourceKeypair = KeyPair.FromSecretSeed(seed);
            var sourceAccountResp = await server.Accounts.Account(sourceKeypair);
            var sourceAccount = new Account(sourceKeypair, sourceAccountResp.SequenceNumber);
            var operation = new CreateAccountOperation.Builder(destAccount, "100").SetSourceAccount(sourceKeypair).Build();
            var transaction = new Transaction.Builder(sourceAccount).AddOperation(operation).AddMemo(Memo.Text("Hello Memo")).Build();
            transaction.Sign(sourceKeypair);

            try
            {
                var resp = await server.SubmitTransaction(transaction);
                if (resp.IsSuccess())
                {
                    Console.WriteLine("transaction completed successfully!");
                    await GetAccountBalance(destAccount);
                }
                else
                {
                    Console.WriteLine("transaction failed.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
