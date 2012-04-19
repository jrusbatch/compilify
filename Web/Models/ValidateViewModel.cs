using Newtonsoft.Json;
using Roslyn.Compilers;

namespace Compilify.Web.Models
{
    public class ValidateViewModel
    {
        public string Command { get; set; }
        public string Classes { get; set; }
    }
    
    [JsonObject]
    public class EditorError
    {
        public FileLinePositionSpan Location { get; set; }
        public string Message { get; set; }
    }

    [JsonObject]
    public class EditorLocation
    {
        [JsonProperty("line")]
        public int Line { get; set; }

        [JsonProperty("ch")]
        public int Character { get; set; }
    }
}