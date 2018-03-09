﻿using System;
using stellar_dotnetcore_sdk;
using System.Threading.Tasks;

namespace StellarWallet
{
    class Program
    {
        public static string seed = "SBKDCHYZHNANMNPFUKFKDIAHEL2K3GLNKKRARQWSE67TTEHTGNGGRKKH";
        //public static string address = "GCKVX5H5XVVDE653PU7SWDDZEAMBHRY6ECTDVLRCFFHNP33R2JOLRPXC";
        public static string horizonUrl = "http://47.91.208.241:8000";
        public static async Task Main(string[] args)
        {
            var keyPair = KeyPair.FromSecretSeed(seed);
            var address = keyPair.Address;
            Console.WriteLine(address);

            await GetAccountBalance(keyPair);
            await CreateRandomAccount();

        }

        public async static Task GetAccountBalance(KeyPair keyPair)
        {
            var server = new Server(horizonUrl);
            var account = await server.Accounts.Account(keyPair);
            Console.WriteLine("Balance for account: " + account.KeyPair.AccountId);
            foreach (var b in account.Balances)
            {
                Console.WriteLine("Type : {0} , Code: {1} , Balance: {2}", b.AssetType, b.AssetCode, b.BalanceString);
            }
        }

        //public static async Task CreateRandomAccount()
        public async static Task CreateRandomAccount()
        {
            Network.Use(new Network("Test Newton Network ; 2018-02-27"));
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