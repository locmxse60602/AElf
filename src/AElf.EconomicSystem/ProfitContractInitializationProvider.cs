using System.Collections.Generic;
using AElf.Kernel.SmartContractInitialization;
using AElf.Types;
using Google.Protobuf;
using Volo.Abp.DependencyInjection;

namespace AElf.EconomicSystem
{
    public class ProfitContractInitializationProvider : IContractInitializationProvider, ITransientDependency
    {
        public Hash SystemSmartContractName => ProfitSmartContractAddressNameProvider.Name;
        public string ContractCodeName => "AElf.Contracts.Profit";

        public Dictionary<string, ByteString> GetInitializeMethodMap(byte[] contractCode)
        {
            // Profit Contract doesn't need initial method call list.
            return new Dictionary<string, ByteString>();
        }
    }
}