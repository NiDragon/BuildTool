namespace BuildTool.Generators.VisualStudio
{
    public class VSToolsets
    {
        public readonly static VSToolsets Default = new("v143");

        public readonly static VSToolsets vs2019 = new("v142");
        public readonly static VSToolsets vs2022 = new("v143");

        private string ToolsetVersion;

        private VSToolsets(string ToolsetVersion)
        {
            this.ToolsetVersion = ToolsetVersion;
        }

        public static implicit operator string(VSToolsets right) => right.ToolsetVersion;
        public static explicit operator VSToolsets(string right) => new VSToolsets(right);

        public override string ToString() => $"{ToolsetVersion}";
    }
}
