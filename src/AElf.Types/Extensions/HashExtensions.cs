using System;
using AElf.Types;

namespace AElf
{
    public static class HashExtensions
    {
        public static Hash ToHash(this int intValue)
        {
            return Hash.FromRawBytes(intValue.ToBytes());
        }

        public static Hash Xor(this Hash hash, Hash another)
        {
            return HashHelper.Xor(hash, another);
        }
    }
}