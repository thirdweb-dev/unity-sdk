using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts;
using System.Threading;
using Thirdweb.Contracts.Marketplace.ContractDefinition;

namespace Thirdweb.Contracts.Marketplace
{
    public partial class MarketplaceService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(
            Nethereum.Web3.Web3 web3,
            MarketplaceDeployment marketplaceDeployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            return web3.Eth.GetContractDeploymentHandler<MarketplaceDeployment>().SendRequestAndWaitForReceiptAsync(marketplaceDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, MarketplaceDeployment marketplaceDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<MarketplaceDeployment>().SendRequestAsync(marketplaceDeployment);
        }

        public static async Task<MarketplaceService> DeployContractAndGetServiceAsync(
            Nethereum.Web3.Web3 web3,
            MarketplaceDeployment marketplaceDeployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, marketplaceDeployment, cancellationTokenSource);
            return new MarketplaceService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.Web3 Web3 { get; }

        public ContractHandler ContractHandler { get; }

        public MarketplaceService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<byte[]> DEFAULT_ADMIN_ROLEQueryAsync(DEFAULT_ADMIN_ROLEFunction dEFAULT_ADMIN_ROLEFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DEFAULT_ADMIN_ROLEFunction, byte[]>(dEFAULT_ADMIN_ROLEFunction, blockParameter);
        }

        public Task<byte[]> DEFAULT_ADMIN_ROLEQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DEFAULT_ADMIN_ROLEFunction, byte[]>(null, blockParameter);
        }

        public Task<ulong> MAX_BPSQueryAsync(MAX_BPSFunction mAX_BPSFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MAX_BPSFunction, ulong>(mAX_BPSFunction, blockParameter);
        }

        public Task<ulong> MAX_BPSQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MAX_BPSFunction, ulong>(null, blockParameter);
        }

        public Task<string> AcceptOfferRequestAsync(AcceptOfferFunction acceptOfferFunction)
        {
            return ContractHandler.SendRequestAsync(acceptOfferFunction);
        }

        public Task<TransactionReceipt> AcceptOfferRequestAndWaitForReceiptAsync(AcceptOfferFunction acceptOfferFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(acceptOfferFunction, cancellationToken);
        }

        public Task<string> AcceptOfferRequestAsync(BigInteger listingId, string offeror, string currency, BigInteger pricePerToken)
        {
            var acceptOfferFunction = new AcceptOfferFunction();
            acceptOfferFunction.ListingId = listingId;
            acceptOfferFunction.Offeror = offeror;
            acceptOfferFunction.Currency = currency;
            acceptOfferFunction.PricePerToken = pricePerToken;

            return ContractHandler.SendRequestAsync(acceptOfferFunction);
        }

        public Task<TransactionReceipt> AcceptOfferRequestAndWaitForReceiptAsync(
            BigInteger listingId,
            string offeror,
            string currency,
            BigInteger pricePerToken,
            CancellationTokenSource cancellationToken = null
        )
        {
            var acceptOfferFunction = new AcceptOfferFunction();
            acceptOfferFunction.ListingId = listingId;
            acceptOfferFunction.Offeror = offeror;
            acceptOfferFunction.Currency = currency;
            acceptOfferFunction.PricePerToken = pricePerToken;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(acceptOfferFunction, cancellationToken);
        }

        public Task<ulong> BidBufferBpsQueryAsync(BidBufferBpsFunction bidBufferBpsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<BidBufferBpsFunction, ulong>(bidBufferBpsFunction, blockParameter);
        }

        public Task<ulong> BidBufferBpsQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<BidBufferBpsFunction, ulong>(null, blockParameter);
        }

        public Task<string> BuyRequestAsync(BuyFunction buyFunction)
        {
            return ContractHandler.SendRequestAsync(buyFunction);
        }

        public Task<TransactionReceipt> BuyRequestAndWaitForReceiptAsync(BuyFunction buyFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(buyFunction, cancellationToken);
        }

        public Task<string> BuyRequestAsync(BigInteger listingId, string buyFor, BigInteger quantityToBuy, string currency, BigInteger totalPrice)
        {
            var buyFunction = new BuyFunction();
            buyFunction.ListingId = listingId;
            buyFunction.BuyFor = buyFor;
            buyFunction.QuantityToBuy = quantityToBuy;
            buyFunction.Currency = currency;
            buyFunction.TotalPrice = totalPrice;

            return ContractHandler.SendRequestAsync(buyFunction);
        }

        public Task<TransactionReceipt> BuyRequestAndWaitForReceiptAsync(
            BigInteger listingId,
            string buyFor,
            BigInteger quantityToBuy,
            string currency,
            BigInteger totalPrice,
            CancellationTokenSource cancellationToken = null
        )
        {
            var buyFunction = new BuyFunction();
            buyFunction.ListingId = listingId;
            buyFunction.BuyFor = buyFor;
            buyFunction.QuantityToBuy = quantityToBuy;
            buyFunction.Currency = currency;
            buyFunction.TotalPrice = totalPrice;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(buyFunction, cancellationToken);
        }

        public Task<string> CancelDirectListingRequestAsync(CancelDirectListingFunction cancelDirectListingFunction)
        {
            return ContractHandler.SendRequestAsync(cancelDirectListingFunction);
        }

        public Task<TransactionReceipt> CancelDirectListingRequestAndWaitForReceiptAsync(CancelDirectListingFunction cancelDirectListingFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(cancelDirectListingFunction, cancellationToken);
        }

        public Task<string> CancelDirectListingRequestAsync(BigInteger listingId)
        {
            var cancelDirectListingFunction = new CancelDirectListingFunction();
            cancelDirectListingFunction.ListingId = listingId;

            return ContractHandler.SendRequestAsync(cancelDirectListingFunction);
        }

        public Task<TransactionReceipt> CancelDirectListingRequestAndWaitForReceiptAsync(BigInteger listingId, CancellationTokenSource cancellationToken = null)
        {
            var cancelDirectListingFunction = new CancelDirectListingFunction();
            cancelDirectListingFunction.ListingId = listingId;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(cancelDirectListingFunction, cancellationToken);
        }

        public Task<string> CloseAuctionRequestAsync(CloseAuctionFunction closeAuctionFunction)
        {
            return ContractHandler.SendRequestAsync(closeAuctionFunction);
        }

        public Task<TransactionReceipt> CloseAuctionRequestAndWaitForReceiptAsync(CloseAuctionFunction closeAuctionFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(closeAuctionFunction, cancellationToken);
        }

        public Task<string> CloseAuctionRequestAsync(BigInteger listingId, string closeFor)
        {
            var closeAuctionFunction = new CloseAuctionFunction();
            closeAuctionFunction.ListingId = listingId;
            closeAuctionFunction.CloseFor = closeFor;

            return ContractHandler.SendRequestAsync(closeAuctionFunction);
        }

        public Task<TransactionReceipt> CloseAuctionRequestAndWaitForReceiptAsync(BigInteger listingId, string closeFor, CancellationTokenSource cancellationToken = null)
        {
            var closeAuctionFunction = new CloseAuctionFunction();
            closeAuctionFunction.ListingId = listingId;
            closeAuctionFunction.CloseFor = closeFor;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(closeAuctionFunction, cancellationToken);
        }

        public Task<byte[]> ContractTypeQueryAsync(ContractTypeFunction contractTypeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractTypeFunction, byte[]>(contractTypeFunction, blockParameter);
        }

        public Task<byte[]> ContractTypeQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractTypeFunction, byte[]>(null, blockParameter);
        }

        public Task<string> ContractURIQueryAsync(ContractURIFunction contractURIFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractURIFunction, string>(contractURIFunction, blockParameter);
        }

        public Task<string> ContractURIQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractURIFunction, string>(null, blockParameter);
        }

        public Task<byte> ContractVersionQueryAsync(ContractVersionFunction contractVersionFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractVersionFunction, byte>(contractVersionFunction, blockParameter);
        }

        public Task<byte> ContractVersionQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractVersionFunction, byte>(null, blockParameter);
        }

        public Task<string> CreateListingRequestAsync(CreateListingFunction createListingFunction)
        {
            return ContractHandler.SendRequestAsync(createListingFunction);
        }

        public Task<TransactionReceipt> CreateListingRequestAndWaitForReceiptAsync(CreateListingFunction createListingFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(createListingFunction, cancellationToken);
        }

        public Task<string> CreateListingRequestAsync(ListingParameters @params)
        {
            var createListingFunction = new CreateListingFunction();
            createListingFunction.Params = @params;

            return ContractHandler.SendRequestAsync(createListingFunction);
        }

        public Task<TransactionReceipt> CreateListingRequestAndWaitForReceiptAsync(ListingParameters @params, CancellationTokenSource cancellationToken = null)
        {
            var createListingFunction = new CreateListingFunction();
            createListingFunction.Params = @params;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(createListingFunction, cancellationToken);
        }

        public Task<GetPlatformFeeInfoOutputDTO> GetPlatformFeeInfoQueryAsync(GetPlatformFeeInfoFunction getPlatformFeeInfoFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetPlatformFeeInfoFunction, GetPlatformFeeInfoOutputDTO>(getPlatformFeeInfoFunction, blockParameter);
        }

        public Task<GetPlatformFeeInfoOutputDTO> GetPlatformFeeInfoQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetPlatformFeeInfoFunction, GetPlatformFeeInfoOutputDTO>(null, blockParameter);
        }

        public Task<byte[]> GetRoleAdminQueryAsync(GetRoleAdminFunction getRoleAdminFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRoleAdminFunction, byte[]>(getRoleAdminFunction, blockParameter);
        }

        public Task<byte[]> GetRoleAdminQueryAsync(byte[] role, BlockParameter blockParameter = null)
        {
            var getRoleAdminFunction = new GetRoleAdminFunction();
            getRoleAdminFunction.Role = role;

            return ContractHandler.QueryAsync<GetRoleAdminFunction, byte[]>(getRoleAdminFunction, blockParameter);
        }

        public Task<string> GetRoleMemberQueryAsync(GetRoleMemberFunction getRoleMemberFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRoleMemberFunction, string>(getRoleMemberFunction, blockParameter);
        }

        public Task<string> GetRoleMemberQueryAsync(byte[] role, BigInteger index, BlockParameter blockParameter = null)
        {
            var getRoleMemberFunction = new GetRoleMemberFunction();
            getRoleMemberFunction.Role = role;
            getRoleMemberFunction.Index = index;

            return ContractHandler.QueryAsync<GetRoleMemberFunction, string>(getRoleMemberFunction, blockParameter);
        }

        public Task<BigInteger> GetRoleMemberCountQueryAsync(GetRoleMemberCountFunction getRoleMemberCountFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRoleMemberCountFunction, BigInteger>(getRoleMemberCountFunction, blockParameter);
        }

        public Task<BigInteger> GetRoleMemberCountQueryAsync(byte[] role, BlockParameter blockParameter = null)
        {
            var getRoleMemberCountFunction = new GetRoleMemberCountFunction();
            getRoleMemberCountFunction.Role = role;

            return ContractHandler.QueryAsync<GetRoleMemberCountFunction, BigInteger>(getRoleMemberCountFunction, blockParameter);
        }

        public Task<string> GrantRoleRequestAsync(GrantRoleFunction grantRoleFunction)
        {
            return ContractHandler.SendRequestAsync(grantRoleFunction);
        }

        public Task<TransactionReceipt> GrantRoleRequestAndWaitForReceiptAsync(GrantRoleFunction grantRoleFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(grantRoleFunction, cancellationToken);
        }

        public Task<string> GrantRoleRequestAsync(byte[] role, string account)
        {
            var grantRoleFunction = new GrantRoleFunction();
            grantRoleFunction.Role = role;
            grantRoleFunction.Account = account;

            return ContractHandler.SendRequestAsync(grantRoleFunction);
        }

        public Task<TransactionReceipt> GrantRoleRequestAndWaitForReceiptAsync(byte[] role, string account, CancellationTokenSource cancellationToken = null)
        {
            var grantRoleFunction = new GrantRoleFunction();
            grantRoleFunction.Role = role;
            grantRoleFunction.Account = account;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(grantRoleFunction, cancellationToken);
        }

        public Task<bool> HasRoleQueryAsync(HasRoleFunction hasRoleFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<HasRoleFunction, bool>(hasRoleFunction, blockParameter);
        }

        public Task<bool> HasRoleQueryAsync(byte[] role, string account, BlockParameter blockParameter = null)
        {
            var hasRoleFunction = new HasRoleFunction();
            hasRoleFunction.Role = role;
            hasRoleFunction.Account = account;

            return ContractHandler.QueryAsync<HasRoleFunction, bool>(hasRoleFunction, blockParameter);
        }

        public Task<string> InitializeRequestAsync(InitializeFunction initializeFunction)
        {
            return ContractHandler.SendRequestAsync(initializeFunction);
        }

        public Task<TransactionReceipt> InitializeRequestAndWaitForReceiptAsync(InitializeFunction initializeFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(initializeFunction, cancellationToken);
        }

        public Task<string> InitializeRequestAsync(string defaultAdmin, string contractURI, List<string> trustedForwarders, string platformFeeRecipient, BigInteger platformFeeBps)
        {
            var initializeFunction = new InitializeFunction();
            initializeFunction.DefaultAdmin = defaultAdmin;
            initializeFunction.ContractURI = contractURI;
            initializeFunction.TrustedForwarders = trustedForwarders;
            initializeFunction.PlatformFeeRecipient = platformFeeRecipient;
            initializeFunction.PlatformFeeBps = platformFeeBps;

            return ContractHandler.SendRequestAsync(initializeFunction);
        }

        public Task<TransactionReceipt> InitializeRequestAndWaitForReceiptAsync(
            string defaultAdmin,
            string contractURI,
            List<string> trustedForwarders,
            string platformFeeRecipient,
            BigInteger platformFeeBps,
            CancellationTokenSource cancellationToken = null
        )
        {
            var initializeFunction = new InitializeFunction();
            initializeFunction.DefaultAdmin = defaultAdmin;
            initializeFunction.ContractURI = contractURI;
            initializeFunction.TrustedForwarders = trustedForwarders;
            initializeFunction.PlatformFeeRecipient = platformFeeRecipient;
            initializeFunction.PlatformFeeBps = platformFeeBps;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(initializeFunction, cancellationToken);
        }

        public Task<bool> IsTrustedForwarderQueryAsync(IsTrustedForwarderFunction isTrustedForwarderFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<IsTrustedForwarderFunction, bool>(isTrustedForwarderFunction, blockParameter);
        }

        public Task<bool> IsTrustedForwarderQueryAsync(string forwarder, BlockParameter blockParameter = null)
        {
            var isTrustedForwarderFunction = new IsTrustedForwarderFunction();
            isTrustedForwarderFunction.Forwarder = forwarder;

            return ContractHandler.QueryAsync<IsTrustedForwarderFunction, bool>(isTrustedForwarderFunction, blockParameter);
        }

        public Task<ListingsOutputDTO> ListingsQueryAsync(ListingsFunction listingsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<ListingsFunction, ListingsOutputDTO>(listingsFunction, blockParameter);
        }

        public Task<ListingsOutputDTO> ListingsQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var listingsFunction = new ListingsFunction();
            listingsFunction.ReturnValue1 = returnValue1;

            return ContractHandler.QueryDeserializingToObjectAsync<ListingsFunction, ListingsOutputDTO>(listingsFunction, blockParameter);
        }

        public Task<string> MulticallRequestAsync(MulticallFunction multicallFunction)
        {
            return ContractHandler.SendRequestAsync(multicallFunction);
        }

        public Task<TransactionReceipt> MulticallRequestAndWaitForReceiptAsync(MulticallFunction multicallFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(multicallFunction, cancellationToken);
        }

        public Task<string> MulticallRequestAsync(List<byte[]> data)
        {
            var multicallFunction = new MulticallFunction();
            multicallFunction.Data = data;

            return ContractHandler.SendRequestAsync(multicallFunction);
        }

        public Task<TransactionReceipt> MulticallRequestAndWaitForReceiptAsync(List<byte[]> data, CancellationTokenSource cancellationToken = null)
        {
            var multicallFunction = new MulticallFunction();
            multicallFunction.Data = data;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(multicallFunction, cancellationToken);
        }

        public Task<string> OfferRequestAsync(OfferFunction offerFunction)
        {
            return ContractHandler.SendRequestAsync(offerFunction);
        }

        public Task<TransactionReceipt> OfferRequestAndWaitForReceiptAsync(OfferFunction offerFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(offerFunction, cancellationToken);
        }

        public Task<string> OfferRequestAsync(BigInteger listingId, BigInteger quantityWanted, string currency, BigInteger pricePerToken, BigInteger expirationTimestamp)
        {
            var offerFunction = new OfferFunction();
            offerFunction.ListingId = listingId;
            offerFunction.QuantityWanted = quantityWanted;
            offerFunction.Currency = currency;
            offerFunction.PricePerToken = pricePerToken;
            offerFunction.ExpirationTimestamp = expirationTimestamp;

            return ContractHandler.SendRequestAsync(offerFunction);
        }

        public Task<TransactionReceipt> OfferRequestAndWaitForReceiptAsync(
            BigInteger listingId,
            BigInteger quantityWanted,
            string currency,
            BigInteger pricePerToken,
            BigInteger expirationTimestamp,
            CancellationTokenSource cancellationToken = null
        )
        {
            var offerFunction = new OfferFunction();
            offerFunction.ListingId = listingId;
            offerFunction.QuantityWanted = quantityWanted;
            offerFunction.Currency = currency;
            offerFunction.PricePerToken = pricePerToken;
            offerFunction.ExpirationTimestamp = expirationTimestamp;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(offerFunction, cancellationToken);
        }

        public Task<OffersOutputDTO> OffersQueryAsync(OffersFunction offersFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<OffersFunction, OffersOutputDTO>(offersFunction, blockParameter);
        }

        public Task<OffersOutputDTO> OffersQueryAsync(BigInteger returnValue1, string returnValue2, BlockParameter blockParameter = null)
        {
            var offersFunction = new OffersFunction();
            offersFunction.ReturnValue1 = returnValue1;
            offersFunction.ReturnValue2 = returnValue2;

            return ContractHandler.QueryDeserializingToObjectAsync<OffersFunction, OffersOutputDTO>(offersFunction, blockParameter);
        }

        public Task<string> OnERC1155BatchReceivedRequestAsync(OnERC1155BatchReceivedFunction onERC1155BatchReceivedFunction)
        {
            return ContractHandler.SendRequestAsync(onERC1155BatchReceivedFunction);
        }

        public Task<TransactionReceipt> OnERC1155BatchReceivedRequestAndWaitForReceiptAsync(
            OnERC1155BatchReceivedFunction onERC1155BatchReceivedFunction,
            CancellationTokenSource cancellationToken = null
        )
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(onERC1155BatchReceivedFunction, cancellationToken);
        }

        public Task<string> OnERC1155BatchReceivedRequestAsync(string returnValue1, string returnValue2, List<BigInteger> returnValue3, List<BigInteger> returnValue4, byte[] returnValue5)
        {
            var onERC1155BatchReceivedFunction = new OnERC1155BatchReceivedFunction();
            onERC1155BatchReceivedFunction.ReturnValue1 = returnValue1;
            onERC1155BatchReceivedFunction.ReturnValue2 = returnValue2;
            onERC1155BatchReceivedFunction.ReturnValue3 = returnValue3;
            onERC1155BatchReceivedFunction.ReturnValue4 = returnValue4;
            onERC1155BatchReceivedFunction.ReturnValue5 = returnValue5;

            return ContractHandler.SendRequestAsync(onERC1155BatchReceivedFunction);
        }

        public Task<TransactionReceipt> OnERC1155BatchReceivedRequestAndWaitForReceiptAsync(
            string returnValue1,
            string returnValue2,
            List<BigInteger> returnValue3,
            List<BigInteger> returnValue4,
            byte[] returnValue5,
            CancellationTokenSource cancellationToken = null
        )
        {
            var onERC1155BatchReceivedFunction = new OnERC1155BatchReceivedFunction();
            onERC1155BatchReceivedFunction.ReturnValue1 = returnValue1;
            onERC1155BatchReceivedFunction.ReturnValue2 = returnValue2;
            onERC1155BatchReceivedFunction.ReturnValue3 = returnValue3;
            onERC1155BatchReceivedFunction.ReturnValue4 = returnValue4;
            onERC1155BatchReceivedFunction.ReturnValue5 = returnValue5;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(onERC1155BatchReceivedFunction, cancellationToken);
        }

        public Task<string> OnERC1155ReceivedRequestAsync(OnERC1155ReceivedFunction onERC1155ReceivedFunction)
        {
            return ContractHandler.SendRequestAsync(onERC1155ReceivedFunction);
        }

        public Task<TransactionReceipt> OnERC1155ReceivedRequestAndWaitForReceiptAsync(OnERC1155ReceivedFunction onERC1155ReceivedFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(onERC1155ReceivedFunction, cancellationToken);
        }

        public Task<string> OnERC1155ReceivedRequestAsync(string returnValue1, string returnValue2, BigInteger returnValue3, BigInteger returnValue4, byte[] returnValue5)
        {
            var onERC1155ReceivedFunction = new OnERC1155ReceivedFunction();
            onERC1155ReceivedFunction.ReturnValue1 = returnValue1;
            onERC1155ReceivedFunction.ReturnValue2 = returnValue2;
            onERC1155ReceivedFunction.ReturnValue3 = returnValue3;
            onERC1155ReceivedFunction.ReturnValue4 = returnValue4;
            onERC1155ReceivedFunction.ReturnValue5 = returnValue5;

            return ContractHandler.SendRequestAsync(onERC1155ReceivedFunction);
        }

        public Task<TransactionReceipt> OnERC1155ReceivedRequestAndWaitForReceiptAsync(
            string returnValue1,
            string returnValue2,
            BigInteger returnValue3,
            BigInteger returnValue4,
            byte[] returnValue5,
            CancellationTokenSource cancellationToken = null
        )
        {
            var onERC1155ReceivedFunction = new OnERC1155ReceivedFunction();
            onERC1155ReceivedFunction.ReturnValue1 = returnValue1;
            onERC1155ReceivedFunction.ReturnValue2 = returnValue2;
            onERC1155ReceivedFunction.ReturnValue3 = returnValue3;
            onERC1155ReceivedFunction.ReturnValue4 = returnValue4;
            onERC1155ReceivedFunction.ReturnValue5 = returnValue5;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(onERC1155ReceivedFunction, cancellationToken);
        }

        public Task<byte[]> OnERC721ReceivedQueryAsync(OnERC721ReceivedFunction onERC721ReceivedFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OnERC721ReceivedFunction, byte[]>(onERC721ReceivedFunction, blockParameter);
        }

        public Task<byte[]> OnERC721ReceivedQueryAsync(string returnValue1, string returnValue2, BigInteger returnValue3, byte[] returnValue4, BlockParameter blockParameter = null)
        {
            var onERC721ReceivedFunction = new OnERC721ReceivedFunction();
            onERC721ReceivedFunction.ReturnValue1 = returnValue1;
            onERC721ReceivedFunction.ReturnValue2 = returnValue2;
            onERC721ReceivedFunction.ReturnValue3 = returnValue3;
            onERC721ReceivedFunction.ReturnValue4 = returnValue4;

            return ContractHandler.QueryAsync<OnERC721ReceivedFunction, byte[]>(onERC721ReceivedFunction, blockParameter);
        }

        public Task<string> RenounceRoleRequestAsync(RenounceRoleFunction renounceRoleFunction)
        {
            return ContractHandler.SendRequestAsync(renounceRoleFunction);
        }

        public Task<TransactionReceipt> RenounceRoleRequestAndWaitForReceiptAsync(RenounceRoleFunction renounceRoleFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(renounceRoleFunction, cancellationToken);
        }

        public Task<string> RenounceRoleRequestAsync(byte[] role, string account)
        {
            var renounceRoleFunction = new RenounceRoleFunction();
            renounceRoleFunction.Role = role;
            renounceRoleFunction.Account = account;

            return ContractHandler.SendRequestAsync(renounceRoleFunction);
        }

        public Task<TransactionReceipt> RenounceRoleRequestAndWaitForReceiptAsync(byte[] role, string account, CancellationTokenSource cancellationToken = null)
        {
            var renounceRoleFunction = new RenounceRoleFunction();
            renounceRoleFunction.Role = role;
            renounceRoleFunction.Account = account;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(renounceRoleFunction, cancellationToken);
        }

        public Task<string> RevokeRoleRequestAsync(RevokeRoleFunction revokeRoleFunction)
        {
            return ContractHandler.SendRequestAsync(revokeRoleFunction);
        }

        public Task<TransactionReceipt> RevokeRoleRequestAndWaitForReceiptAsync(RevokeRoleFunction revokeRoleFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(revokeRoleFunction, cancellationToken);
        }

        public Task<string> RevokeRoleRequestAsync(byte[] role, string account)
        {
            var revokeRoleFunction = new RevokeRoleFunction();
            revokeRoleFunction.Role = role;
            revokeRoleFunction.Account = account;

            return ContractHandler.SendRequestAsync(revokeRoleFunction);
        }

        public Task<TransactionReceipt> RevokeRoleRequestAndWaitForReceiptAsync(byte[] role, string account, CancellationTokenSource cancellationToken = null)
        {
            var revokeRoleFunction = new RevokeRoleFunction();
            revokeRoleFunction.Role = role;
            revokeRoleFunction.Account = account;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(revokeRoleFunction, cancellationToken);
        }

        public Task<string> SetAuctionBuffersRequestAsync(SetAuctionBuffersFunction setAuctionBuffersFunction)
        {
            return ContractHandler.SendRequestAsync(setAuctionBuffersFunction);
        }

        public Task<TransactionReceipt> SetAuctionBuffersRequestAndWaitForReceiptAsync(SetAuctionBuffersFunction setAuctionBuffersFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setAuctionBuffersFunction, cancellationToken);
        }

        public Task<string> SetAuctionBuffersRequestAsync(BigInteger timeBuffer, BigInteger bidBufferBps)
        {
            var setAuctionBuffersFunction = new SetAuctionBuffersFunction();
            setAuctionBuffersFunction.TimeBuffer = timeBuffer;
            setAuctionBuffersFunction.BidBufferBps = bidBufferBps;

            return ContractHandler.SendRequestAsync(setAuctionBuffersFunction);
        }

        public Task<TransactionReceipt> SetAuctionBuffersRequestAndWaitForReceiptAsync(BigInteger timeBuffer, BigInteger bidBufferBps, CancellationTokenSource cancellationToken = null)
        {
            var setAuctionBuffersFunction = new SetAuctionBuffersFunction();
            setAuctionBuffersFunction.TimeBuffer = timeBuffer;
            setAuctionBuffersFunction.BidBufferBps = bidBufferBps;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setAuctionBuffersFunction, cancellationToken);
        }

        public Task<string> SetContractURIRequestAsync(SetContractURIFunction setContractURIFunction)
        {
            return ContractHandler.SendRequestAsync(setContractURIFunction);
        }

        public Task<TransactionReceipt> SetContractURIRequestAndWaitForReceiptAsync(SetContractURIFunction setContractURIFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setContractURIFunction, cancellationToken);
        }

        public Task<string> SetContractURIRequestAsync(string uri)
        {
            var setContractURIFunction = new SetContractURIFunction();
            setContractURIFunction.Uri = uri;

            return ContractHandler.SendRequestAsync(setContractURIFunction);
        }

        public Task<TransactionReceipt> SetContractURIRequestAndWaitForReceiptAsync(string uri, CancellationTokenSource cancellationToken = null)
        {
            var setContractURIFunction = new SetContractURIFunction();
            setContractURIFunction.Uri = uri;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setContractURIFunction, cancellationToken);
        }

        public Task<string> SetPlatformFeeInfoRequestAsync(SetPlatformFeeInfoFunction setPlatformFeeInfoFunction)
        {
            return ContractHandler.SendRequestAsync(setPlatformFeeInfoFunction);
        }

        public Task<TransactionReceipt> SetPlatformFeeInfoRequestAndWaitForReceiptAsync(SetPlatformFeeInfoFunction setPlatformFeeInfoFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setPlatformFeeInfoFunction, cancellationToken);
        }

        public Task<string> SetPlatformFeeInfoRequestAsync(string platformFeeRecipient, BigInteger platformFeeBps)
        {
            var setPlatformFeeInfoFunction = new SetPlatformFeeInfoFunction();
            setPlatformFeeInfoFunction.PlatformFeeRecipient = platformFeeRecipient;
            setPlatformFeeInfoFunction.PlatformFeeBps = platformFeeBps;

            return ContractHandler.SendRequestAsync(setPlatformFeeInfoFunction);
        }

        public Task<TransactionReceipt> SetPlatformFeeInfoRequestAndWaitForReceiptAsync(string platformFeeRecipient, BigInteger platformFeeBps, CancellationTokenSource cancellationToken = null)
        {
            var setPlatformFeeInfoFunction = new SetPlatformFeeInfoFunction();
            setPlatformFeeInfoFunction.PlatformFeeRecipient = platformFeeRecipient;
            setPlatformFeeInfoFunction.PlatformFeeBps = platformFeeBps;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setPlatformFeeInfoFunction, cancellationToken);
        }

        public Task<bool> SupportsInterfaceQueryAsync(SupportsInterfaceFunction supportsInterfaceFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SupportsInterfaceFunction, bool>(supportsInterfaceFunction, blockParameter);
        }

        public Task<bool> SupportsInterfaceQueryAsync(byte[] interfaceId, BlockParameter blockParameter = null)
        {
            var supportsInterfaceFunction = new SupportsInterfaceFunction();
            supportsInterfaceFunction.InterfaceId = interfaceId;

            return ContractHandler.QueryAsync<SupportsInterfaceFunction, bool>(supportsInterfaceFunction, blockParameter);
        }

        public Task<ulong> TimeBufferQueryAsync(TimeBufferFunction timeBufferFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TimeBufferFunction, ulong>(timeBufferFunction, blockParameter);
        }

        public Task<ulong> TimeBufferQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TimeBufferFunction, ulong>(null, blockParameter);
        }

        public Task<BigInteger> TotalListingsQueryAsync(TotalListingsFunction totalListingsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TotalListingsFunction, BigInteger>(totalListingsFunction, blockParameter);
        }

        public Task<BigInteger> TotalListingsQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TotalListingsFunction, BigInteger>(null, blockParameter);
        }

        public Task<string> UpdateListingRequestAsync(UpdateListingFunction updateListingFunction)
        {
            return ContractHandler.SendRequestAsync(updateListingFunction);
        }

        public Task<TransactionReceipt> UpdateListingRequestAndWaitForReceiptAsync(UpdateListingFunction updateListingFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(updateListingFunction, cancellationToken);
        }

        public Task<string> UpdateListingRequestAsync(
            BigInteger listingId,
            BigInteger quantityToList,
            BigInteger reservePricePerToken,
            BigInteger buyoutPricePerToken,
            string currencyToAccept,
            BigInteger startTime,
            BigInteger secondsUntilEndTime
        )
        {
            var updateListingFunction = new UpdateListingFunction();
            updateListingFunction.ListingId = listingId;
            updateListingFunction.QuantityToList = quantityToList;
            updateListingFunction.ReservePricePerToken = reservePricePerToken;
            updateListingFunction.BuyoutPricePerToken = buyoutPricePerToken;
            updateListingFunction.CurrencyToAccept = currencyToAccept;
            updateListingFunction.StartTime = startTime;
            updateListingFunction.SecondsUntilEndTime = secondsUntilEndTime;

            return ContractHandler.SendRequestAsync(updateListingFunction);
        }

        public Task<TransactionReceipt> UpdateListingRequestAndWaitForReceiptAsync(
            BigInteger listingId,
            BigInteger quantityToList,
            BigInteger reservePricePerToken,
            BigInteger buyoutPricePerToken,
            string currencyToAccept,
            BigInteger startTime,
            BigInteger secondsUntilEndTime,
            CancellationTokenSource cancellationToken = null
        )
        {
            var updateListingFunction = new UpdateListingFunction();
            updateListingFunction.ListingId = listingId;
            updateListingFunction.QuantityToList = quantityToList;
            updateListingFunction.ReservePricePerToken = reservePricePerToken;
            updateListingFunction.BuyoutPricePerToken = buyoutPricePerToken;
            updateListingFunction.CurrencyToAccept = currencyToAccept;
            updateListingFunction.StartTime = startTime;
            updateListingFunction.SecondsUntilEndTime = secondsUntilEndTime;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(updateListingFunction, cancellationToken);
        }

        public Task<WinningBidOutputDTO> WinningBidQueryAsync(WinningBidFunction winningBidFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<WinningBidFunction, WinningBidOutputDTO>(winningBidFunction, blockParameter);
        }

        public Task<WinningBidOutputDTO> WinningBidQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var winningBidFunction = new WinningBidFunction();
            winningBidFunction.ReturnValue1 = returnValue1;

            return ContractHandler.QueryDeserializingToObjectAsync<WinningBidFunction, WinningBidOutputDTO>(winningBidFunction, blockParameter);
        }
    }
}
