using System;
using Newtonsoft.Json;

namespace Compilify.Web.Models
{
    public class Envelope
    {
        [JsonProperty("status", Required = Required.Always)]
        public string Status { get; set; }

        [JsonProperty("exception", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Exception Exception { get; set; }

        [JsonProperty("message", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Message { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }
}