using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.Kernel.Miner.Application;
using AElf.Kernel.SmartContract.Application;
using AElf.Kernel.Token;
using AElf.Kernel.Txn.Application;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AElf.Kernel.SmartContract.ExecutionPluginForResourceFee
{
    public class DonateResourceTransactionGenerator : ISystemTransactionGenerator
    {
        private readonly ISmartContractAddressService _smartContractAddressService;
        private readonly ITotalResourceTokensMapProvider _totalResourceTokensMapProvider;
        private readonly TransactionPackingOptions _transactionPackingOptions;

        public ILogger<DonateResourceTransactionGenerator> Logger { get; set; }


        public DonateResourceTransactionGenerator(ISmartContractAddressService smartContractAddressService,
            IOptionsMonitor<TransactionPackingOptions> transactionPackingOptions,
            ITotalResourceTokensMapProvider totalResourceTokensMapProvider)
        {
            _smartContractAddressService = smartContractAddressService;
            _totalResourceTokensMapProvider = totalResourceTokensMapProvider;
            _transactionPackingOptions = transactionPackingOptions.CurrentValue;
        }

        public async Task<List<Transaction>> GenerateTransactionsAsync(Address @from, long preBlockHeight,
            Hash preBlockHash)
        {
            var generatedTransactions = new List<Transaction>();
            if (!_transactionPackingOptions.IsTransactionPackable)
                return generatedTransactions;

            if (preBlockHeight < Constants.GenesisBlockHeight)
                return generatedTransactions;


            var tokenContractAddress = _smartContractAddressService.GetAddressByContractName(
                TokenSmartContractAddressNameProvider.Name);

            if (tokenContractAddress == null)
            {
                return generatedTransactions;
            }

            var totalResourceTokensMap = await _totalResourceTokensMapProvider.GetTotalResourceTokensMapAsync(
                new ChainContext
                {
                    BlockHash = preBlockHash,
                    BlockHeight = preBlockHeight
                });
            var bill = new TransactionFeeBill
            {
                FeesMap = {totalResourceTokensMap.Value}
            };
            generatedTransactions.AddRange(new List<Transaction>
            {
                new Transaction
                {
                    From = from,
                    MethodName = nameof(TokenContractContainer.TokenContractStub.DonateResourceToken),
                    To = tokenContractAddress,
                    RefBlockNumber = preBlockHeight,
                    RefBlockPrefix = ByteString.CopyFrom(preBlockHash.Value.Take(4).ToArray()),
                    Params = bill.ToByteString()
                }
            });

            Logger.LogInformation("Donate resource transaction generated.");
            return generatedTransactions;
        }
    }
}