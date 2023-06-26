using System.Collections.Generic;
using WalletConnectSharp.Common.Utils;

namespace WalletConnectSharp.Common.Model.Errors
{
    /// <summary>
    /// A helper class for generating error messages
    /// based on an ErrorType
    /// </summary>
    public static class SdkErrors
    {
        private static readonly Dictionary<string, object> DefaultParameters = new Dictionary<string, object>()
        {
            {"topic", "undefined"},
            {"message", "Something went wrong"},
            {"name", "parameter"},
            {"context", "session"},
            {"blockchain", "Ethereum"}
        };

        /// <summary>
        /// Generate an error message using an ErrorType code and a dictionary
        /// of parameters for the error message
        /// An anonymous type can also be passed, which will be converted to a
        /// dictionary
        /// </summary>
        /// <param name="type">The error type message to generate</param>
        /// <param name="params">A dictionary (or anonymous type) of parameters for the error message</param>
        /// <returns>The error message as a string</returns>
        public static string MessageFromType(ErrorType type, object @params = null)
        {
            return MessageFromType(type, null, @params.AsDictionary());
        }
        
        /// <summary>
        /// Generate an error message using an ErrorType code, a message parameters
        /// and a dictionary of parameters for the error message
        /// </summary>
        /// <param name="type">The error type message to generate</param>
        /// <param name="message">The message parameter</param>
        /// <param name="params">A dictionary of parameters for the error message</param>
        /// <returns>The error message as a string</returns>
        public static string MessageFromType(ErrorType type, string message = null, Dictionary<string, object> @params = null)
        {
            if (@params == null)
            {
                @params = new Dictionary<string, object>();
            }
            
            if (!string.IsNullOrWhiteSpace(message) && !@params.ContainsKey("message"))
                @params.Add("message", message);
            
            string errorMessage;
            switch (type)
            {
                default:
                case ErrorType.GENERIC:
                    errorMessage = "{message}";
                    break;
                case ErrorType.MISSING_OR_INVALID:
                    errorMessage = "Missing or invalid {name}";
                    break;
                case ErrorType.MISSING_RESPONSE:
                    errorMessage = "Response is required for approved {context} proposals";
                    break;
                case ErrorType.MISSING_DECRYPT_PARAMS:
                    errorMessage = "Decrypt params required for {context}";
                    break;
                case ErrorType.INVALID_UPDATE_REQUEST:
                    errorMessage = "Invalid {context} update request";
                    break;
                case ErrorType.INVALID_UPGRADE_REQUEST:
                    errorMessage = "Invalid {context} upgrade request";
                    break;
                case ErrorType.INVALID_EXTEND_REQUEST:
                    errorMessage = "Invalid {context} extend request";
                    break;
                case ErrorType.INVALID_STORAGE_KEY_NAME:
                    errorMessage = "Invalid storage key name: {name}";
                    break;
                case ErrorType.RECORD_ALREADY_EXISTS:
                    errorMessage = "Record already exists for {context} matching id: {id}";
                    break;
                case ErrorType.RESTORE_WILL_OVERRIDE:
                    errorMessage = "Restore will override already set {context}";
                    break;
                case ErrorType.NO_MATCHING_ID:
                    errorMessage = "No matching {context} with id: {id}";
                    break;
                case ErrorType.NO_MATCHING_TOPIC:
                    errorMessage = "No matching {context} with topic {topic}";
                    break;
                case ErrorType.NO_MATCHING_RESPONSE:
                    errorMessage = "No response found in pending {context} proposal";
                    break;
                case ErrorType.NO_MATCHING_KEY:
                    errorMessage = "No matching key with tag: {tag}";
                    break;
                case ErrorType.UNKNOWN_JSONRPC_METHOD:
                    errorMessage = "Unknown JSON-RPC Method Requested: {method}";
                    break;
                case ErrorType.MISMATCHED_TOPIC:
                    errorMessage = "Mismatched topic for {context} with id: {id}";
                    break;
                case ErrorType.MISMATCHED_ACCOUNTS:
                    errorMessage = "Invalid accounts with mismatched chains: {mismatched}";
                    break;
                case ErrorType.SETTLED:
                    errorMessage = "{context} settled";
                    break;
                case ErrorType.NOT_APPROVED:
                    errorMessage = "{context} not approved";
                    break;
                case ErrorType.PROPOSAL_RESPONDED:
                    errorMessage = "{context} proposal responded";
                    break;
                case ErrorType.RESPONSE_ACKNOWLEDGED:
                    errorMessage = "{context} response acknowledge";
                    break;
                case ErrorType.EXPIRED:
                    errorMessage = "{context} expired";
                    break;
                case ErrorType.DELETED:
                    errorMessage = "{context} deleted";
                    break;
                case ErrorType.RESUBSCRIBED:
                    errorMessage = "Subscription resubscribed with topic: {topic}";
                    break;
                case ErrorType.NOT_INITIALIZED:
                    errorMessage = "{params} was not initialized";
                    break;
                case ErrorType.SETTLE_TIMEOUT:
                    errorMessage = "{context} failed to settle after {timeout} seconds";
                    break;
                case ErrorType.JSONRPC_REQUEST_TIMEOUT:
                    errorMessage = "JSON-RPC Request timeout after {timeout} seconds: {method}";
                    break;
                case ErrorType.UNAUTHORIZED_TARGET_CHAIN:
                    errorMessage = "Unauthorized Target ChainId Requested: {chainId}";
                    break;
                case ErrorType.UNAUTHORIZED_JSON_RPC_METHOD:
                    errorMessage = "Unauthorized JSON-RPC Method Requested: {method}";
                    break;
                case ErrorType.UNAUTHORIZED_NOTIFICATION_TYPE:
                    errorMessage = "Unauthorized Notification Type Requested: {type}";
                    break;
                case ErrorType.UNAUTHORIZED_UPDATE_REQUEST:
                    errorMessage = "Unauthorized {context} update request";
                    break;
                case ErrorType.UNAUTHORIZED_UPGRADE_REQUEST:
                    errorMessage = "Unauthorized {context} upgrade request";
                    break;
                case ErrorType.UNAUTHORIZED_EXTEND_REQUEST:
                    errorMessage = "Unauthorized {context} extend request";
                    break;
                case ErrorType.UNAUTHORIZED_MATCHING_CONTROLLER:
                    errorMessage = "Unauthorized: method {method} not allowed";
                    break;
                case ErrorType.UNAUTHORIZED_METHOD:
                    errorMessage = "Unauthorized: peer is also {controller} controller";
                    break;
                case ErrorType.JSONRPC_REQUEST_METHOD_REJECTED:
                    errorMessage = "User rejected the request.";
                    break;
                case ErrorType.JSONRPC_REQUEST_METHOD_UNAUTHORIZED:
                    errorMessage = "The requested account and/or method has not been authorized by the user.";
                    break;
                case ErrorType.JSONRPC_REQUEST_METHOD_UNSUPPORTED:
                    errorMessage = "The requested method is not supported by this {blockchain} provider.";
                    break;
                case ErrorType.DISCONNECTED_ALL_CHAINS:
                    errorMessage = "The provider is disconnected from all chains.";
                    break;
                case ErrorType.DISCONNECTED_TARGET_CHAIN:
                    errorMessage = "The provider is disconnected from the specified chain.";
                    break;
                case ErrorType.DISAPPROVED_CHAINS:
                    errorMessage = "User disapproved requested chains";
                    break;
                case ErrorType.DISAPPROVED_JSONRPC:
                    errorMessage = "JSON-RPC disapproved request";
                    break;
                case ErrorType.DISAPPROVED_NOTIFICATION:
                    errorMessage = "User disapproved requested notification types";
                    break;
                case ErrorType.UNSUPPORTED_CHAINS:
                    errorMessage = "Requested chains are not supported: {chains}";
                    break;
                case ErrorType.UNSUPPORTED_JSONRPC:
                    errorMessage = "Requested json-rpc methods are not supported: {methods}";
                    break;
                case ErrorType.UNSUPPORTED_NOTIFICATION:
                    errorMessage = "Requested notification types are not supported: {types}";
                    break;
                case ErrorType.UNSUPPORTED_ACCOUNTS:
                    errorMessage = "{message}";
                    break;
                case ErrorType.USER_DISCONNECTED:
                    errorMessage = "User disconnected {context}";
                    break;
                case ErrorType.UNKNOWN:
                    errorMessage = "Unknown error {params}";
                    break;
                case ErrorType.NON_CONFORMING_NAMESPACES:
                    errorMessage = @params["message"].ToString();
                    break;
            }

            errorMessage = FormatErrorText(errorMessage, DefaultParameters, @params);

            return errorMessage;
        }

        private static string FormatErrorText(string formattedText, Dictionary<string, object> defaultArgs, Dictionary<string, object> args = null)
        {
            if (args == null)
                args = new Dictionary<string, object>();
            
            string text = formattedText;

            foreach (var key in args.Keys)
            {
                text = text.Replace("{" + key.ToLower() + "}", args[key].ToString());
            }
            
            foreach (var key in defaultArgs.Keys)
            {
                text = text.Replace("{" + key.ToLower() + "}", defaultArgs[key].ToString());
            }

            return text;
        }
    }
}