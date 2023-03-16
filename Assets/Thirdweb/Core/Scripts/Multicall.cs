using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using TokenERC721 = Thirdweb.Contracts.TokenERC721.ContractDefinition;

namespace Thirdweb
{
    public class TokenData721
    {
        public string Contract;
        public string TokenId;
        public string Owner;
        public string Uri;
    }

    public static class Multicall
    {
        public static async Task<List<TokenData721>> GetAllTokenData721(string contractAddress, int startTokenId, int endTokenId)
        {
            var allOwnedTokens = await GetAllOwners721(contractAddress, startTokenId, endTokenId);
            var allTokenUris = await GetAllTokenUris721(contractAddress, startTokenId, endTokenId);
            for (int i = 0; i < allOwnedTokens.Count; i++)
                allOwnedTokens[i].Uri = allTokenUris[i].Uri;
            return allOwnedTokens;
        }

        public static async Task<List<TokenData721>> GetOwnedTokenData721(string contractAddress, string ownerAddress)
        {
            var ownedTokens = await GetOwnedTokenIds721(contractAddress, ownerAddress);
            var ownedTokenUris = await GetSpecificTokenUris721(contractAddress, ownedTokens.ToArray());
            foreach (var ownedToken in ownedTokenUris)
                ownedToken.Owner = ownerAddress;
            return ownedTokenUris;
        }

        public static async Task<List<TokenData721>> GetAllOwners721(string contractAddress, int startTokenId, int endTokenId)
        {
            MultiQueryHandler multiqueryHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetMultiQueryHandler();
            var calls = new List<MulticallInputOutput<TokenERC721.OwnerOfFunction, TokenERC721.OwnerOfOutputDTO>>();
            for (int i = startTokenId; i <= endTokenId; i++)
            {
                var tokenOfOwnerByIndex = new TokenERC721.OwnerOfFunction() { TokenId = i };
                calls.Add(new MulticallInputOutput<TokenERC721.OwnerOfFunction, TokenERC721.OwnerOfOutputDTO>(tokenOfOwnerByIndex, contractAddress));
            }
            var results = await multiqueryHandler.MultiCallAsync(MultiQueryHandler.DEFAULT_CALLS_PER_REQUEST, calls.ToArray()).ConfigureAwait(false);
            return calls
                .Select(
                    x =>
                        new TokenData721()
                        {
                            Contract = contractAddress,
                            TokenId = x.Input.TokenId.ToString(),
                            Owner = x.Output.ReturnValue1
                        }
                )
                .ToList();
        }

        public static async Task<List<TokenData721>> GetSpecificOwners721(string contractAddress, int[] tokenIds)
        {
            MultiQueryHandler multiqueryHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetMultiQueryHandler();
            var calls = new List<MulticallInputOutput<TokenERC721.OwnerOfFunction, TokenERC721.OwnerOfOutputDTO>>();
            for (int i = 0; i < tokenIds.Length; i++)
            {
                var tokenOfOwnerByIndex = new TokenERC721.OwnerOfFunction() { TokenId = i };
                calls.Add(new MulticallInputOutput<TokenERC721.OwnerOfFunction, TokenERC721.OwnerOfOutputDTO>(tokenOfOwnerByIndex, contractAddress));
            }
            var results = await multiqueryHandler.MultiCallAsync(MultiQueryHandler.DEFAULT_CALLS_PER_REQUEST, calls.ToArray()).ConfigureAwait(false);
            return calls
                .Select(
                    x =>
                        new TokenData721()
                        {
                            Contract = contractAddress,
                            TokenId = x.Input.TokenId.ToString(),
                            Owner = x.Output.ReturnValue1
                        }
                )
                .ToList();
        }

        public static async Task<List<TokenData721>> GetAllTokenUris721(string contractAddress, int startTokenId, int endTokenId)
        {
            MultiQueryHandler multiqueryHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetMultiQueryHandler();
            var calls = new List<MulticallInputOutput<TokenERC721.TokenURIFunction, TokenERC721.TokenURIOutputDTO>>();
            for (int i = startTokenId; i <= endTokenId; i++)
            {
                var tokenUriFunction = new TokenERC721.TokenURIFunction() { TokenId = i };
                calls.Add(new MulticallInputOutput<TokenERC721.TokenURIFunction, TokenERC721.TokenURIOutputDTO>(tokenUriFunction, contractAddress));
            }
            var results = await multiqueryHandler.MultiCallAsync(MultiQueryHandler.DEFAULT_CALLS_PER_REQUEST, calls.ToArray()).ConfigureAwait(false);
            return calls
                .Select(
                    x =>
                        new TokenData721()
                        {
                            Contract = contractAddress,
                            Uri = x.Output.ReturnValue1,
                            TokenId = x.Input.TokenId.ToString()
                        }
                )
                .ToList();
        }

        public static async Task<List<TokenData721>> GetSpecificTokenUris721(string contractAddress, int[] tokenIds)
        {
            MultiQueryHandler multiqueryHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetMultiQueryHandler();
            var calls = new List<MulticallInputOutput<TokenERC721.TokenURIFunction, TokenERC721.TokenURIOutputDTO>>();
            for (int i = 0; i < tokenIds.Length; i++)
            {
                var tokenUriFunction = new TokenERC721.TokenURIFunction() { TokenId = tokenIds[i] };
                calls.Add(new MulticallInputOutput<TokenERC721.TokenURIFunction, TokenERC721.TokenURIOutputDTO>(tokenUriFunction, contractAddress));
            }
            var results = await multiqueryHandler.MultiCallAsync(MultiQueryHandler.DEFAULT_CALLS_PER_REQUEST, calls.ToArray()).ConfigureAwait(false);
            return calls
                .Select(
                    x =>
                        new TokenData721()
                        {
                            Contract = contractAddress,
                            Uri = x.Output.ReturnValue1,
                            TokenId = x.Input.TokenId.ToString()
                        }
                )
                .ToList();
        }

        public static async Task<List<int>> GetOwnedTokenIds721(string contractAddress, string ownerAddress)
        {
            MultiQueryHandler multiqueryHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetMultiQueryHandler();
            var contract = ThirdwebManager.Instance.SDK.GetContract(contractAddress);
            var balance = BigInteger.Parse(await contract.ERC721.BalanceOf(ownerAddress));
            var calls = new List<MulticallInputOutput<TokenERC721.TokenOfOwnerByIndexFunction, TokenERC721.TokenOfOwnerByIndexOutputDTO>>();
            for (int i = 0; i < balance; i++)
            {
                var tokenOfOwnerByIndex = new TokenERC721.TokenOfOwnerByIndexFunction() { Owner = ownerAddress, Index = i };
                calls.Add(new MulticallInputOutput<TokenERC721.TokenOfOwnerByIndexFunction, TokenERC721.TokenOfOwnerByIndexOutputDTO>(tokenOfOwnerByIndex, contractAddress));
            }
            var results = await multiqueryHandler.MultiCallAsync(MultiQueryHandler.DEFAULT_CALLS_PER_REQUEST, calls.ToArray()).ConfigureAwait(false);
            return calls.Select(x => (int)x.Output.ReturnValue1).ToList();
        }
    }
}
