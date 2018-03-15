using Nett;
using stellar_dotnetcore_sdk;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StellarWallet
{
    class WalletConfig
    {
        public string NetworkPassphrase { get; set; }
        public string HorizonUrl { get; set; }
        public string AccountSeed { get; set; }
    }
    class Stellar
    {
        private string NetworkPassphrase = "";
        private string AccountSeed = "";
        private string HorizonUrl = "";

        private Server server;
        public Stellar()
        {

        }
        public Stellar(string _NetworkPassphrase, string _AccountSeed, string _HorizonUrl)
        {
            NetworkPassphrase = _NetworkPassphrase;
            AccountSeed = _AccountSeed;
            HorizonUrl = _HorizonUrl;
        }
        public bool Init()
        {
            Network.Use(new Network(NetworkPassphrase));
            server = new Server(HorizonUrl);
            return true;
        }
        public bool InitFromConfigFile(string configFile)
        {
            try
            {
                var config = Toml.ReadFile<WalletConfig>(configFile);
                NetworkPassphrase = config.NetworkPassphrase;
                AccountSeed = config.AccountSeed;
                HorizonUrl = config.HorizonUrl;
                Init();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        public KeyPair RandomAccount()
        {
            return KeyPair.Random();
        }
        public async Task GetAccountBalance(KeyPair keyPair)
        {
            var account = await server.Accounts.Account(keyPair);
            Console.WriteLine("Balance for account: " + account.KeyPair.AccountId);
            foreach (var b in account.Balances)
            {
                Console.WriteLine("Type : {0} , Code: {1} , Balance: {2}", b.AssetType, b.AssetCode, b.BalanceString);
            }
        }
        public async Task MakePayment(KeyPair destAccountKeyPair, string amount)
        {
            var destAccount = server.Accounts.Account(destAccountKeyPair);
            var sourceKeypair = KeyPair.FromSecretSeed(AccountSeed);
            var sourceAccountResp = await server.Accounts.Account(sourceKeypair);
            var sourceAccount = new Account(sourceKeypair, sourceAccountResp.SequenceNumber);
            var operation = new PaymentOperation.Builder(destAccountKeyPair, new AssetTypeNative(), amount).Build();
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

        public async Task CreateAccount(KeyPair destAccount)
        {
            var sourceKeypair = KeyPair.FromSecretSeed(AccountSeed);
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
                    foreach (var x in c.OperationsResultCodes)
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
    }

}
