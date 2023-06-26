using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Core.Models.Expirer
{
    /// <summary>
    /// A class that converts a <see cref="Expiration.Target"/> >string to either a ID (long) or a Topic (string). Either the ID or Topic
    /// will be non-null while the other will be null.
    /// The format for the <see cref="Expiration.Target"/> can be either
    /// * id:123
    /// * topic:my_topic_string
    /// </summary>
    public class ExpirerTarget
    {
        /// <summary>
        /// The resulting ID from the given <see cref="Expiration.Target"/>. If the <see cref="Expiration.Target"/> did not include an Id, then
        /// this field will be null
        /// </summary>
        [JsonProperty("id")]
        public long? Id { get; set; }
        
        /// <summary>
        /// The resulting Topic from the given <see cref="Expiration.Target"/>. If the <see cref="Expiration.Target"/> did not include a Topic, then
        /// this field will be null
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }

        /// <summary>
        /// Create a new instance of this class with a given <see cref="Expiration.Target"/>. The given <see cref="Expiration.Target"/> will
        /// be converted and stored to either the ID field or Topic field
        /// </summary>
        /// <param name="target">The <see cref="Expiration.Target"/> to convert</param>
        /// <exception cref="Exception">If the format for the given <see cref="Expiration.Target"/> is invalid</exception>
        public ExpirerTarget(string target)
        {
            var values = target.Split(":");
            var type = values[0];
            var value = values[1];

            switch (type)
            {
                case "topic":
                    Topic = value;
                    break;
                case "id":
                {
                    long id;
                    var success = long.TryParse(value, out id);

                    if (!success)
                        throw new Exception($"Cannot parse id {value}");

                    Id = id;
                    break;
                }
                default:
                    throw new Exception($"Invalid target, expected id:number or topic:string, got {type}:{value}");
            }
        }
    }
}
