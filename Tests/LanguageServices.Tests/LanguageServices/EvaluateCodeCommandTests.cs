using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using Xunit;

namespace Compilify.LanguageServices
{
    public class EvaluateCodeCommandTests
    {
        [Fact]
        public void CanBeSerialized()
        {
            var expected = new EvaluateCodeCommand
                          {
                              Documents = new List<Document>()
                          };

            EvaluateCodeCommand actual;
            using (var stream = new MemoryStream())
            {
                Assert.DoesNotThrow(() => Serializer.Serialize(stream, expected));

                stream.Seek(0, SeekOrigin.Begin);

                actual = Serializer.Deserialize<EvaluateCodeCommand>(stream);
            }

            Assert.NotNull(actual);
            Assert.Equal(0, actual.Documents.Count());
        }
    }
}
