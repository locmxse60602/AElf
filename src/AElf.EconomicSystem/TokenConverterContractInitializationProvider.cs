using System.Collections.Generic;
using AElf.Kernel.SmartContractInitialization;
using AElf.Types;
using Google.Protobuf;
using Volo.Abp.DependencyInjection;

namespace AElf.EconomicSystem
{
    public class TokenConverterContractInitializationProvider : IContractInitializationProvider, ITransientDependency
    {
        public Hash SystemSmartContractName { get; } = TokenConverterSmartContractAddressNameProvider.Name;
        public string ContractCodeName { get; } = "AElf.Contracts.TokenConverter";

        public Dictionary<string, ByteString> GetInitializeMethodMap(byte[] contractCode)
        {
            return new Dictionary<string, ByteString>();
        }
    }
}