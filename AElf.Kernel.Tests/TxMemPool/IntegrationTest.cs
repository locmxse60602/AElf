﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AElf.Kernel.Crypto.ECDSA;
using AElf.Kernel.KernelAccount;
using AElf.Kernel.Managers;
using AElf.Kernel.Services;
using AElf.Kernel.TxMemPool;
using Akka.Actor;
using Akka.Util;
using Google.Protobuf;
using NLog;
using Xunit;
using Xunit.Frameworks.Autofac;

namespace AElf.Kernel.Tests.TxMemPool
{
    [UseAutofacTestFramework]
    public class IntegrationTest
    {
        private readonly IAccountContextService _accountContextService;
        private readonly ILogger _logger;
        private readonly ITransactionManager _transactionManager;
        private readonly ITransactionResultManager _transactionResultManager;
        
        public IntegrationTest(IAccountContextService accountContextService, ILogger logger,
            ITransactionManager transactionManager, ITransactionResultManager transactionResultManager)
        {
            _accountContextService = accountContextService;
            _logger = logger;
            _transactionManager = transactionManager;
            _transactionResultManager = transactionResultManager;
        }
        
        private TxPool GetPool()
        {
            return new TxPool(TxPoolConfig.Default, _logger);
        }

        public static Transaction BuildTransaction(Hash adrFrom = null, Hash adrTo = null, ulong nonce = 0)
        {
            
            ECKeyPair keyPair = new KeyPairGenerator().Generate();

            var tx = new Transaction();
            tx.From = adrFrom == null ? Hash.Generate() : adrFrom;
            tx.To = adrTo == null ? Hash.Generate() : adrTo;
            tx.IncrementId = nonce;
            tx.P = ByteString.CopyFrom(keyPair.PublicKey.Q.GetEncoded());
            tx.Fee = TxPoolConfig.Default.FeeThreshold + 1;
            tx.MethodName = "hello world";
            tx.Params = ByteString.CopyFrom(new Parameters
            {
                Params = { new Param
                {
                    IntVal = 1
                }}
            }.ToByteArray());

            // Serialize and hash the transaction
            Hash hash = tx.GetHash();
            
            // Sign the hash
            ECSigner signer = new ECSigner();
            ECSignature signature = signer.Sign(keyPair, hash.GetHashBytes());
            
            // Update the signature
            tx.R = ByteString.CopyFrom(signature.R);
            tx.S = ByteString.CopyFrom(signature.S);
            
            return tx;
        }
        
        
        [Fact]
        public async Task Start()
        {
            var pool = GetPool();

            var poolService = new TxPoolService(pool, _accountContextService, _transactionManager,
                _transactionResultManager);
            poolService.Start();
            var Num = 3;
            var threadNum = 5;

            int count = 0;
            
            var addrList = new List<Hash>();

            var sortedSet = new Dictionary<Hash, SortedSet<int>>();

            int r = 5;

            int i = 0;
            while (i < Num )
            {
                var addr = Hash.Generate();
                addrList.Add(addr);
                sortedSet[addr] = new SortedSet<int>();
                i++;
            }
            
            var txList = new List<ITransaction>();

            var txCount = threadNum * r;
            while (count++ < txCount)
            {
                var index = count % Num;
                var id =  new Random().Next(25);
                sortedSet[addrList[index]].Add(id);
                var tx = BuildTransaction(addrList[index], nonce: (ulong)id);
                txList.Add(tx);
            }

            

            var j1 = 0;
            while (j1 < txCount)
            {
                
                var tx = txList[j1];
                var res = await poolService.AddTxAsync(tx);

                if (j1 % txCount == 0)
                {
                    var txs = await poolService.GetReadyTxsAsync(2000);

                    var resLists =new List<TransactionResult>();
                    foreach (var t in txs)
                    {
                        resLists.Add(new TransactionResult
                        {
                            TransactionId = t.GetHash()
                        });
                    }
                    await poolService.ResetAndUpdate(resLists);
                }
                
                j1++;
            }
        }
        
        
        [Fact]
        public async Task StartMultiThread()
        {
            var pool = GetPool();

            var poolService = new TxPoolService(pool, _accountContextService, _transactionManager,
                _transactionResultManager);
            poolService.Start();
            ulong queued = 0;
            ulong exec = 0;
            
            var results = new List<TransactionResult>();

            var IdDict = new Dictionary<Hash, ulong>();
            int k = 0;
            var Num = 2;
            var r = 5;
            var threadNum = Num *r;

            int count = 0;
            
            var addrList = new List<Hash>();

            var sortedSet = new Dictionary<Hash, SortedSet<int>>();

            int i = 0;
            while (i < Num )
            {
                var addr = Hash.Generate();
                addrList.Add(addr);
                sortedSet[addr] = new SortedSet<int>();
                i++;
            }
            
            var txList = new List<ITransaction>();
            
            while (count++ < threadNum)
            {
                var index = count % Num;
                var id =  new Random().Next(r);
                sortedSet[addrList[index]].Add(id);
                var tx = BuildTransaction(addrList[index], nonce: (ulong)id);
                txList.Add(tx);
            }

            foreach (var addr in addrList)
            {
                ulong c = 0;
                foreach (var t in sortedSet[addr])
                {
                    if (t != (int)c)
                        break;
                    c++;
                }
                IdDict[addr] = c;
            }

            var rr = 0;
            while (rr< r)
            {
                var tasks = new List<Task>();

                for (var j = 0; j < Num; j++)
                {
                    var j1 = j;
                    var rr1 = rr;
                    var task = Task.Run(async () =>
                    {
                    
                        // sorted set for tx id
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                    
                        var res = await poolService.AddTxAsync(txList[j1 + Num * rr1]);
                        results.Add(new TransactionResult
                        {
                            TransactionId = txList[j1].GetHash()
                        });
                        stopwatch.Stop();
                        Debug.WriteLine(stopwatch.ElapsedMilliseconds);
                    
                        
                    });
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
                
                if (rr % r == 0)
                {
                    var txs = await poolService.GetReadyTxsAsync(2000);

                    var resLists =new List<TransactionResult>();
                    foreach (var t in txs)
                    {
                        resLists.Add(new TransactionResult
                        {
                            TransactionId = t.GetHash()
                        });
                    }
                    await poolService.ResetAndUpdate(resLists);
                }
                
                rr++;
            }
            
            

            

            var execSize = pool.GetExecutableSize();
            var waitingSize = pool.GetWaitingSize();
            var sortedCount = sortedSet.Values.Aggregate(0, (current, p) => current + p.Count);
            Assert.True(sortedCount >= (int)pool.Size);

            //await poolService.PromoteAsync();

            // executable list size 
            /*Assert.Equal(exec, await poolService.GetExecutableSizeAsync());
            Assert.Equal(queued - exec, await poolService.GetWaitingSizeAsync());*/

            var list = await poolService.GetReadyTxsAsync(2000);

            var txReuslts = list.Select(t => new TransactionResult
            {
                TransactionId = t.GetHash()
            }).ToList();
            
            await poolService.ResetAndUpdate(txReuslts);
            
            foreach (var address in addrList)
            {
                // pool state
                Assert.Equal(IdDict[address], pool.Nonces[address]);
                
                // account state
                Assert.Equal(IdDict[address],
                    (await _accountContextService.GetAccountDataContext(address, pool.ChainId)).IncrementId);
            }
        }
        
        
        
    }
}