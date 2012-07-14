using Newtonsoft.Json;
using Roslyn.Compilers;

namespace Compilify.Web.Models
{
    [JsonObject]
    public class EditorError
    {
        public FileLinePositionSpan Location { get; set; }

        public string Message { get; set; }
    }
}