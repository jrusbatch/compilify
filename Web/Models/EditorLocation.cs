using Newtonsoft.Json;

namespace Compilify.Web.Models
{
    [JsonObject]
    public class EditorLocation
    {
        [JsonProperty("line")]
        public int Line { get; set; }

        [JsonProperty("ch")]
        public int Character { get; set; }
    }
}