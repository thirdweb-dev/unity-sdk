using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// Indicates an error
    /// </summary>
    public class Error
    {
        /// <summary>
        /// The error code of this error
        /// </summary>
        [JsonProperty("code")]
        public long Code;

        /// <summary>
        /// The message for this error
        /// </summary>
        [JsonProperty("message")]
        public string Message;

        /// <summary>
        /// Any extra data for this error
        /// </summary>
        [JsonProperty("data")]
        public string Data;

        /// <summary>
        /// Create an ErrorResponse with a given ErrorType and (optional) parameters
        /// </summary>
        /// <param name="type">The error type of the ErrorResponse to create</param>
        /// <param name="params">Extra parameters for the error message</param>
        /// <param name="extraData">Extra data that is stored in the Data field of the newly created ErrorResponse</param>
        /// <returns>A new ErrorResponse</returns>
        public static Error FromErrorType(ErrorType type, object @params = null, string extraData = null)
        {
            string message = SdkErrors.MessageFromType(type, @params);

            return new Error()
            {
                Code = (long) type,
                Message = message,
                Data = extraData
            };
        }

        /// <summary>
        /// Create an ErrorResponse from a WalletConnectException
        /// </summary>
        /// <param name="walletConnectException">The exception to grab error values from</param>
        /// <returns>A new ErrorResponse object using values from the given exception</returns>
        public static Error FromException(WalletConnectException walletConnectException)
        {
            return new Error()
            {
                Code = walletConnectException.Code,
                Message = walletConnectException.Message,
                Data = walletConnectException.ToString()
            };
        }

        /// <summary>
        /// Convert this ErrorResponse to a WalletConnectException
        /// </summary>
        /// <returns>A new WalletConnectException using values from this ErrorResponse</returns>
        public WalletConnectException ToException()
        {
            return WalletConnectException.FromType((ErrorType)Code, Message);
        }

        private sealed class CodeMessageDataEqualityComparer : IEqualityComparer<Error>
        {
            public bool Equals(Error x, Error y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.Code == y.Code && x.Message == y.Message && x.Data == y.Data;
            }

            public int GetHashCode(Error obj)
            {
                return HashCode.Combine(obj.Code, obj.Message, obj.Data);
            }
        }

        public static IEqualityComparer<Error> CodeMessageDataComparer { get; } = new CodeMessageDataEqualityComparer();

        protected bool Equals(Error other)
        {
            return Code == other.Code && Message == other.Message && Data == other.Data;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Error)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Message, Data);
        }
    }
}
