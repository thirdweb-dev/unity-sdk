using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding;
using System.Linq;
using System;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.FunctionEncoding.Attributes;
using UnityEngine.Networking;
using Thirdweb.Redcode.Awaiting;

namespace Thirdweb
{
    /// <summary>
    /// Convenient wrapper to interact with any EVM contract
    /// </summary>
    public class Contract : Routable
    {
        public BigInteger Chain { get; private set; }
        public string Address { get; private set; }
        public string ABI { get; private set; }

        /// <summary>
        /// Call any ERC20 supported functions
        /// </summary>
        public ERC20 ERC20 { get; private set; }

        /// <summary>
        /// Call any ERC721 supported functions
        /// </summary>
        public ERC721 ERC721 { get; private set; }

        /// <summary>
        /// Call any ERC1155 supported functions
        /// </summary>
        public ERC1155 ERC1155 { get; private set; }

        /// <summary>
        /// Call any Marketplace supported functions
        /// </summary>
        public Marketplace Marketplace { get; private set; }

        /// <summary>
        /// Call any Pack supported functions
        /// </summary>
        public Pack Pack { get; private set; }

        /// <summary>
        /// Call any Contract Event functions
        /// </summary>
        public Events Events { get; private set; }

        internal readonly ThirdwebSDK _sdk;

        /// <summary>
        /// Convenient wrapper to interact with any EVM contract
        /// </summary>
        /// <param name="chain">The chain identifier.</param>
        /// <param name="address">The contract address.</param>
        /// <param name="abi">The contract ABI.</param>
        public Contract(ThirdwebSDK sdk, BigInteger chain, string address, string abi = null)
            : base(abi != null ? $"{address}{Routable.subSeparator}{abi}" : address)
        {
            this._sdk = sdk;
            this.Chain = chain;
            this.Address = address;
            this.ABI = abi;
            this.ERC20 = new ERC20(sdk, baseRoute, address);
            this.ERC721 = new ERC721(sdk, baseRoute, address);
            this.ERC1155 = new ERC1155(sdk, baseRoute, address);
            this.Marketplace = new Marketplace(sdk, baseRoute, address);
            this.Pack = new Pack(sdk, address);
            this.Events = new Events(sdk, baseRoute);
        }

        /// <summary>
        /// Get the balance of the contract.
        /// </summary>
        /// <returns>The balance of the contract as a <see cref="CurrencyValue"/> object.</returns>
        public async Task<CurrencyValue> GetBalance()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<CurrencyValue>(getRoute("balance"), new string[] { });
            }
            else
            {
                var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
                BigInteger balance = await web3.Eth.GetBalance.SendRequestAsync(Address);
                var cv = new CurrencyValue { value = balance.ToString(), displayValue = balance.ToString().ToEth() };
                return cv;
            }
        }

        /// <summary>
        /// Prepare a transaction by creating a <see cref="Transaction"/> object.
        /// </summary>
        /// <param name="functionName">The name of the contract function.</param>
        /// <param name="args">Optional function arguments.</param>
        /// <returns>A <see cref="Transaction"/> object representing the prepared transaction.</returns>
        public async Task<Transaction> Prepare(string functionName, params object[] args)
        {
            return await Prepare(functionName, null, args);
        }

        /// <summary>
        /// Prepare a transaction by creating a <see cref="Transaction"/> object.
        /// </summary>
        /// <param name="functionName">The name of the contract function.</param>
        /// <param name="from">The address to send the transaction from.</param>
        /// <param name="args">Optional function arguments.</param>
        /// <returns>A <see cref="Transaction"/> object representing the prepared transaction.</returns>
        public async Task<Transaction> Prepare(string functionName, string from = null, params object[] args)
        {
            var initialInput = new TransactionInput();
            if (Utils.IsWebGLBuild())
            {
                initialInput.From = from ?? await _sdk.Wallet.GetAddress();
                initialInput.To = Address;
            }
            else
            {
                if (this.ABI == null)
                    this.ABI = await FetchAbi(this.Address, await _sdk.Wallet.GetChainId());

                var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
                var contract = web3.Eth.GetContract(this.ABI, this.Address);
                var function = Utils.GetFunctionMatchSignature(contract, functionName, args);
                var fromAddress = from ?? await _sdk.Wallet.GetAddress();
                initialInput = function.CreateTransactionInput(fromAddress, args);
            }

            return new Transaction(this, initialInput, functionName, args);
        }

        /// <summary>
        /// Encode the function call with the given arguments.
        /// </summary>
        /// <param name="functionName">The name of the contract function.</param>
        /// <param name="args">The function arguments.</param>
        /// <returns>The encoded function data as a string.</returns>
        public string Encode(string functionName, params object[] args)
        {
            var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            var contract = web3.Eth.GetContract(this.ABI, this.Address);
            var function = Utils.GetFunctionMatchSignature(contract, functionName, args);
            return function.GetData(args);
        }

        /// <summary>
        /// Decode the encoded arguments of a function call.
        /// </summary>
        /// <param name="functionName">The name of the contract function.</param>
        /// <param name="encodedArgs">The encoded arguments as a string.</param>
        /// <returns>A list of <see cref="ParameterOutput"/> objects representing the decoded arguments.</returns>
        public List<ParameterOutput> Decode(string functionName, string encodedArgs)
        {
            var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            var contract = web3.Eth.GetContract(this.ABI, this.Address);
            var function = Utils.GetFunctionMatchSignature(contract, functionName);
            return function.DecodeInput(encodedArgs);
        }

        /// <summary>
        /// Get the events of a contract. For WebGL use contract.Events class instead.
        /// <returns>A list of <see cref="EventLog"/> (extending IEventDTO) objects representing the events.</returns>
        /// </summary>
        public async Task<List<EventLog<TEventDTO>>> GetEventLogs<TEventDTO>(ulong? fromBlock = null, ulong? toBlock = null)
            where TEventDTO : IEventDTO, new()
        {
            var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            var transferEventHandler = web3.Eth.GetEvent<TEventDTO>(this.Address);
            var filter = transferEventHandler.CreateFilterInput(
                fromBlock: fromBlock == null ? BlockParameter.CreateEarliest() : new BlockParameter(fromBlock.Value),
                toBlock: toBlock == null ? BlockParameter.CreateLatest() : new BlockParameter(toBlock.Value)
            );
            var allTransferEventsForContract = await transferEventHandler.GetAllChangesAsync(filter);
            return allTransferEventsForContract;
        }

        /// <summary>
        /// Execute a write transaction on a contract.
        /// </summary>
        /// <param name="functionName">The name of the contract function to call.</param>
        /// <param name="args">Optional function arguments.</param>
        /// <returns>The transaction receipt as a <see cref="TransactionResult"/> object.</returns>
        public Task<TransactionResult> Write(string functionName, params object[] args)
        {
            return Write(functionName, null, args);
        }

        /// <summary>
        /// Execute a write transaction on a contract.
        /// </summary>
        /// <param name="functionName">The name of the contract function to call.</param>
        /// <param name="transactionOverrides">Overrides to pass with the transaction.</param>
        /// <param name="args">Optional function arguments.</param>
        /// <returns>The transaction receipt as a <see cref="TransactionResult"/> object.</returns>
        public async Task<TransactionResult> Write(string functionName, TransactionRequest? transactionOverrides, params object[] args)
        {
            if (Utils.IsWebGLBuild())
            {
                args ??= new object[0];
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("call"), Utils.ToJsonStringArray(functionName, args, transactionOverrides));
            }
            else
            {
                if (this.ABI == null)
                    this.ABI = await FetchAbi(this.Address, await _sdk.Wallet.GetChainId());

                var contract = new Nethereum.Contracts.Contract(null, this.ABI, this.Address);
                var function = Utils.GetFunctionMatchSignature(contract, functionName, args);
                var data = function.GetData(args);
                var input = new TransactionInput
                {
                    From = transactionOverrides?.from ?? await _sdk.Wallet.GetAddress(),
                    To = this.Address,
                    Data = data,
                    Value = transactionOverrides?.value != null ? new HexBigInteger(BigInteger.Parse(transactionOverrides?.value)) : new HexBigInteger(0),
                    Gas = transactionOverrides?.gasLimit != null ? new HexBigInteger(BigInteger.Parse(transactionOverrides?.gasLimit)) : null,
                    GasPrice = transactionOverrides?.gasPrice != null ? new HexBigInteger(BigInteger.Parse(transactionOverrides?.gasPrice)) : null,
                };

                var tx = new Transaction(_sdk, input);
                return await tx.SendAndWaitForTransactionResult();
            }
        }

        /// <summary>
        /// Read data from a contract.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the data into.</typeparam>
        /// <param name="functionName">The name of the contract function to call.</param>
        /// <param name="args">Optional function arguments.</param>
        /// <returns>The deserialized data of type <typeparamref name="T"/>.</returns>
        public async Task<T> Read<T>(string functionName, params object[] args)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<T>(getRoute("call"), Utils.ToJsonStringArray(functionName, args));
            }

            if (this.ABI == null)
                this.ABI = await FetchAbi(this.Address, await _sdk.Wallet.GetChainId());

            var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            var contract = web3.Eth.GetContract(this.ABI, this.Address);
            var function = Utils.GetFunctionMatchSignature(contract, functionName, args);
            var result = await function.CallDecodingToDefaultAsync(args);

            var rawResults = new List<object>();

            if (result[0].Result is List<ParameterOutput> parameterOutputs)
                rawResults.AddRange(parameterOutputs.Select(item => item.Result));
            else
                rawResults.AddRange(result.Select(item => item.Result));

            // Single
            if (rawResults.Count == 1)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(rawResults[0]));
                }
                catch
                {
                    return ConvertValue<T>(rawResults[0]);
                }
            }

            // List or array
            if ((typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>)) || typeof(T).IsArray)
            {
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(rawResults));
            }

            // Class or struct
            if (typeof(T).IsClass || typeof(T).IsValueType)
            {
                var targetType = typeof(T);
                var properties = targetType.GetProperties();
                var fields = targetType.GetFields();
                var combinedCount = properties.Length + fields.Length;

                var instance = Activator.CreateInstance<T>();

                if (rawResults.Count == combinedCount)
                {
                    // Assign values to properties
                    for (int i = 0; i < properties.Length; i++)
                    {
                        try
                        {
                            properties[i].SetValue(instance, rawResults[i]);
                        }
                        catch (ArgumentException ex)
                        {
                            throw new UnityException(
                                $"Type mismatch assigning value to property {properties[i].Name} of type {typeof(T).Name}: expected {rawResults[i].GetType().Name}, got {properties[i].PropertyType.Name}",
                                ex
                            );
                        }
                    }

                    // Assign values to fields
                    for (int i = 0; i < fields.Length; i++)
                    {
                        try
                        {
                            fields[i].SetValue(instance, rawResults[properties.Length + i]);
                        }
                        catch (ArgumentException ex)
                        {
                            throw new UnityException(
                                $"Type mismatch assigning value to field {fields[i].Name} of type {typeof(T).Name}: expected {rawResults[properties.Length + i].GetType().Name}, got {fields[i].FieldType.Name}",
                                ex
                            );
                        }
                    }

                    return instance;
                }
                else if (rawResults.Count == properties.Length) // Just Properties
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        try
                        {
                            properties[i].SetValue(instance, rawResults[i]);
                        }
                        catch (ArgumentException ex)
                        {
                            throw new UnityException(
                                $"Type mismatch assigning value to property {properties[i].Name} of type {typeof(T).Name}: expected {rawResults[i].GetType().Name}, got {properties[i].PropertyType.Name}",
                                ex
                            );
                        }
                    }

                    return instance;
                }
                else if (rawResults.Count == fields.Length) // Just Fields
                {
                    for (int i = 0; i < fields.Length; i++)
                    {
                        try
                        {
                            fields[i].SetValue(instance, rawResults[i]);
                        }
                        catch (ArgumentException ex)
                        {
                            throw new UnityException(
                                $"Type mismatch assigning value to field {fields[i].Name} of type {typeof(T).Name}: expected {rawResults[i].GetType().Name}, got {fields[i].FieldType.Name}",
                                ex
                            );
                        }
                    }

                    return instance;
                }
                else
                {
                    throw new UnityException(
                        $"The number of combined properties and fields in type {typeof(T).Name} does not match the number of results: expected {combinedCount}, got {properties.Length} properties and {fields.Length} fields with {rawResults.Count} results."
                    );
                }
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(rawResults));
        }

        public async Task<T> ReadRaw<T>(string functionName, params object[] args)
            where T : new()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<T>(getRoute("call"), Utils.ToJsonStringArray(functionName, args));
            }

            if (this.ABI == null)
                this.ABI = await FetchAbi(this.Address, await _sdk.Wallet.GetChainId());

            var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            var contract = web3.Eth.GetContract(this.ABI, this.Address);
            var function = Utils.GetFunctionMatchSignature(contract, functionName, args);
            return await function.CallDeserializingToObjectAsync<T>(args);
        }

        public static async Task<string> FetchAbi(string contractAddress, BigInteger chainId)
        {
            var url = $"https://contract.thirdweb.com/abi/{chainId}/{contractAddress}";
            using (var request = UnityWebRequest.Get(url))
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new UnityException($"Failed to fetch ABI! Error: {request.error}");
                }
                return request.downloadHandler.text;
            }
        }

        private T ConvertValue<T>(object value)
        {
            if (value is T result)
            {
                return result;
            }

            if (value == null)
            {
                return default;
            }

            var targetType = typeof(T);
            if (targetType.IsValueType && System.Nullable.GetUnderlyingType(targetType) == null)
            {
                return (T)System.Convert.ChangeType(value, targetType);
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
        }
    }
}
