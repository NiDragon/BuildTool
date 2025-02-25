using BuildTool.Generators.Interfaces;
using System.Resources;
using System.Text;

namespace BuildTool.Generators.VisualStudio
{
    public class VSSln : IGenerator
    {
        private string Filename = string.Empty;
        private List<VSProject> _projects = [];

        private List<string> _configurations = [];
        private List<string> _platforms = [];

        public IReadOnlyList<VSProject> Projects { get => _projects; }
        public IReadOnlyList<string> Configurations { get => _configurations; }
        public IReadOnlyList<string> Platforms { get => _platforms; }

        private static string SlnVersion = "12.00";
        private static string VSVersion = "17.10.35027.167";

        // 0 - Solution Guid, 1 - Project Name (Second one is technically the path), 2 - Project Guid
        private static string ProjectT = "Project(\"{{{0}}}\") = \"{1}\", \"{2}\", \"{{{3}}}\"\n";

        // 0 - Configuration, 1 - Platform
        private static string SolutionConfigurationPlatformsT = "        {0}|{1} = {0}|{1}\n";

        // 0 - Project GUID, 1 - Configuration, 2 - Platform
        private static string ProjectConfigurationPlatformsT = "        {{{0}}}.{1}|{2}.ActiveCfg = {1}|{2}\n"
                                + "        {{{0}}}.{1}|{2}.Build.0 = {1}|{2}\n";

        // 0 - File Version,
        // 1 - Visual Studio Version,
        // 2 - Projects[],
        // 3 - Sln Configs[],
        // 4 - Project Configs[],
        // 5 - Solution Guid 
        public string Template = Encoding.UTF8.GetString(Resources.Resource.Sln_Template);

        public VSSln(string Filename, List<VSProject> Projects, List<string> Configurations, List<string> Platforms)
        {
            this.Filename = Filename;
            _projects = Projects;
            _configurations = Configurations;
            _platforms = Platforms;
        }

        private string SolutionProjects()
        {
            string output = string.Empty;

            Uri startPath = new(Filename);

            foreach (VSProject project in _projects)
            {
                Uri finalPath = startPath.MakeRelativeUri(new Uri(project.GetDirectory()));

                output += string.Format(ProjectT,
                    Guid.NewGuid().ToString().ToUpper(),
                    project.ProjectName,
                    finalPath.ToString() + project.ProjectName + ".vcxproj",
                    project.ProjectGuid);
            }

            output = output.TrimEnd('\r', '\n');

            return output;
        }

        private string SolutionConfigurePlatforms()
        {
            string output = string.Empty;

            foreach(string config in _configurations)
            {
                foreach(string platform in _platforms)
                {
                    output += string.Format(SolutionConfigurationPlatformsT, config, platform);
                }
            }

            output = output.TrimEnd('\r', '\n');

            return output;
        }

        private string ProjectConfigurePlatforms()
        {
            string output = string.Empty;

            foreach(VSProject project in _projects)
            {
                foreach(string config in _configurations)
                {
                    foreach(string platform in _platforms)
                    {
                        output += string.Format(ProjectConfigurationPlatformsT, project.ProjectGuid, config, platform);
                    }
                }
            }

            output = output.TrimEnd('\r', '\n');

            return output;
        }

        private string BuildOutput()
        {
            string output;

            output = string.Format(Template, SlnVersion,
                VSVersion,
                SolutionProjects(),
                SolutionConfigurePlatforms(),
                ProjectConfigurePlatforms(),
                Guid.NewGuid().ToString().ToUpper());

            return output;
        }

        public void Generate()
        {
            using (TextWriter tw = new StreamWriter(Filename))
            {
                tw.Write(BuildOutput());
                tw.Flush();
                tw.Close();
            }
        }
    }
}
