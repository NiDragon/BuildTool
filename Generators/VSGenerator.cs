using BuildTool.Generators.Interfaces;
using BuildTool.Generators.VisualStudio;
using System.Collections.Generic;

namespace BuildTool.Generators {
    /// <summary>
    /// Class generates multiple Visual Studio Projects and a Solution
    /// </summary>
    public class VSGenerator : IGenerator
    {
        public string SolutionName = string.Empty;

        private List<string> _platforms = [];
        private List<string> _configurations = [];

        public List<string> Platforms { get => _platforms; set => _platforms = value; }
        public List<string> Configurations { get => _configurations; set => _configurations = value; }

        private readonly List<VSProject> Projects = [];

        public VSGenerator(string SolutionName, string[] Platforms, string[] Configurations)
        {
            this.SolutionName = SolutionName;
            this.Platforms.AddRange(Platforms);
            this.Configurations.AddRange(Configurations);
        }

        /// <summary>
        /// Add a project to this generator
        /// </summary>
        /// <param name="Project"></param>
        public void AddProject(VSProject Project)
        {
            Projects.Add(Project);
        }

        public void Generate()
        {
            foreach(VSProject project in Projects)
            {
                // Override project configuration to ensure the sln matches
                project.Configurations = Configurations;
                project.Platforms = Platforms;
                
                project.Generate();
            }

            // Create Sln
            VSSln sln = new VSSln(Projects.First().GetDirectory() + SolutionName + ".sln", Projects, Configurations, Platforms);

            sln.Generate();
        }
    }
}