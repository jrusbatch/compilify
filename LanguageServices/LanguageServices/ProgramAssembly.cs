namespace Compilify.LanguageServices
{
    public class ProgramAssembly : ICodeAssembly
    {
        public string EntryPointClassName { get; set; }

        public string EntryPointMethodName { get; set; }

        public byte[] CompiledAssembly { get; set; }
    }
}
