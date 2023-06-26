using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;

namespace WalletConnectSharp.Common.Model.Errors
{
    /// <summary>
    /// An exception that is thrown internally by WalletConnectSharp. This
    /// type can also be JSON serialized
    /// </summary>
    public class WalletConnectException : Exception
    {
        /// <summary>
        /// The error code of this exception
        /// </summary>
        [JsonProperty("code")]
        public uint Code { get; private set; }
        
        /// <summary>
        /// The error tyep of this exception
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; private set; }

        /// <summary>
        /// The error code type as an ErrorType
        /// </summary>
        [JsonIgnore]
        public ErrorType CodeType
        {
            get
            {
                return (ErrorType)Code;
            }
        }

        /// <summary>
        /// Create a new exception with the given message and error type
        /// </summary>
        /// <param name="message">The message that is shown with the exception</param>
        /// <param name="type">The error type for this exception (determines error code)</param>
        public WalletConnectException(string message, ErrorType type) : base(message)
        {
            Code = (uint) type;
            Type = Enum.GetName(typeof(ErrorType), type);
        }

        /// <summary>
        /// Create a new exception with the given message and error type
        /// </summary>
        /// <param name="message">The message that is shown with the exception</param>
        /// <param name="type">The error type for this exception (determines error code)</param>
        /// <param name="innerException">The cause of this exception</param>
        public WalletConnectException(string message, Exception innerException, ErrorType type) : base(message, innerException)
        {
            Code = (uint) type;
            Type = Enum.GetName(typeof(ErrorType), type);
        }

        /// <summary>
        /// A helper function that creates an exception given an ErrorType, a message parameter,
        /// an (optional) dictionary of parameters for the error message and an (optional) inner
        /// exception
        /// </summary>
        /// <param name="type">The error type of the exception</param>
        /// <param name="message">The message parameter</param>
        /// <param name="params">Additional (optional) parameters for the generated error message</param>
        /// <param name="innerException">An (optional) inner exception that caused this exception</param>
        /// <returns>A new exception</returns>
        public static WalletConnectException FromType(ErrorType type, string message = null, Dictionary<string, object> @params = null, Exception innerException = null)
        {
            string errorMessage = SdkErrors.MessageFromType(type, message, @params);

            if (innerException != null)
                return new WalletConnectException(errorMessage, innerException, type);
            return new WalletConnectException(errorMessage, type);
        }

        /// <summary>
        /// A helper function that creates an exception given an ErrorType, and
        /// an (optional) dictionary of parameters for the error message
        /// </summary>
        /// <param name="type">The error type of the exception</param>
        /// <param name="params">Additional (optional) parameters for the generated error message</param>
        /// <returns>A new exception</returns>
        public static WalletConnectException FromType(ErrorType type, object @params = null)
        {
            return FromType(type, null, @params.AsDictionary());
        }
    }
}