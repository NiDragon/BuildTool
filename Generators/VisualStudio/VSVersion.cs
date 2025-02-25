namespace BuildTool.Generators.VisualStudio
{
    public class VSVersion
    {
        public readonly static VSVersion Default = new("17");

        public readonly static VSVersion vs2019 = new("16");
        public readonly static VSVersion vs2022 = new("17");

        private string Version;

        private VSVersion(string Version)
        {
            this.Version = Version;
        }

        public static implicit operator string(VSVersion right) => right.Version;
        public static explicit operator VSVersion(string right) => new VSVersion(right);

        public override string ToString() => $"{Version}";
    }
}
