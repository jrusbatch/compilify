using System.Text;
using System.Threading.Tasks;
using Compilify.LanguageServices;
using Compilify.Models;
using Compilify.Web.Models;

namespace Compilify.Web.Queries
{
    public class SamplePostQuery : IQuery
    {
        private readonly ICodeValidator validator;

        public SamplePostQuery(ICodeValidator codeValidator)
        {
            validator = codeValidator;
        }

        public Task<PostViewModel> Execute()
        {
            var post = new Post();
            var builder = new StringBuilder();

            builder.AppendLine("class Person")
                .AppendLine("{")
                .AppendLine("    public Person(string name)")
                .AppendLine("    {")
                .AppendLine("        Name = name;")
                .AppendLine("    }")
                .AppendLine()
                .AppendLine("    public string Name { get; private set; }")
                .AppendLine()
                .AppendLine("    public string Greet()")
                .AppendLine("    {")
                .AppendLine("        if (Name == null)")
                .AppendLine("            return \"Hello, stranger!\";")
                .AppendLine()
                .AppendLine("        return string.Format(\"Hello, {0}!\", Name);")
                .AppendLine("    }")
                .AppendLine("}");

            post.Classes = builder.ToString();

            builder.Clear()
                .AppendLine("var person = new Person(name: null);")
                .AppendLine()
                .AppendLine("return person.Greet();");

            post.Content = builder.ToString();

            var result = PostViewModel.Create(post);

            result.Errors = validator.GetCompilationErrors(post);

            return Task.FromResult(result);
        }
    }
}