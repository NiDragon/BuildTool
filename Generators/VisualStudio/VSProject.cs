using BuildTool.Generators.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace BuildTool.Generators.VisualStudio
{
    public partial class VSProject : IGenerator
    {
        // Private Data
        private string _projectName = string.Empty;
        private string _projectGuid = VSConfig.GetGUID();

        private List<string> _platforms = [];
        private List<string> _configurations = [];

        private Dictionary<string, string> _outputDirectory = [];
        private Dictionary<string, string> _intDirectory = [];

        private Dictionary<string, string> _includeDirectories = [];
        private Dictionary<string, string> _libraryDirectories = [];

        private List<string> _filters = [];
        private List<string> _includes = [];
        private List<string> _sources = [];
        private List<string> _resources = [];

        // TODO: Process these
        private Dictionary<string, string> _compilerDefinitions = [];
        private Dictionary<string, string> _linkerDefinitions = [];

        // Public Properties
        public List<string> Platforms { get => _platforms; set => _platforms = value; }
        public List<string> Configurations { get => _configurations; set => _configurations = value; }

        public IReadOnlyDictionary<string, string> OutputDirectory { get => _outputDirectory; }
        public IReadOnlyDictionary<string, string> IntDirectory { get => _intDirectory; }

        public IReadOnlyDictionary<string, string> IncludeDirectories { get => _includeDirectories; }
        public IReadOnlyDictionary<string, string> LibraryDirectories { get => _libraryDirectories; }

        public IReadOnlyList<string> Includes { get => _includes; set => _includes = [.. value]; }
        public IReadOnlyList<string> Sources { get => _sources; set => _sources = [.. value]; }
        public IReadOnlyList<string> Resources { get => _resources; set => _resources = [.. value]; }
        public IReadOnlyList<string> Filters { get => _filters; set => _filters = [.. value]; }

        public IReadOnlyDictionary<string, string> CompilerDefinitions { get => _compilerDefinitions; }
        public IReadOnlyDictionary<string, string> LinkerDefinitions { get => _linkerDefinitions; }

        public string ProjectGuid { get => _projectGuid; }
        public string ProjectName { get => _projectName; }

        private Uri? ProjectDirectory = null;
        private Uri? SourceDirectory = null;

        /// <summary>
        /// Create project file contents.
        /// </summary>
        /// <returns>String containing project file contents</returns>
        private string BuildOutput()
        {
            string project = string.Empty;

            project += Header;
            project += Project0;

            project += ProjectConfigurations0;

            foreach (string config in Configurations)
            {
                foreach (string platform in Platforms)
                {
                    project += Configuration(config, platform);
                }
            }
            project += ProjectConfigurations1;

            // Hardcode these for now until we can figure out how to get installed SDKs
            // 10.0 might be okay it just tells Visual Studio to default to highest installed
            project += Globals(ProjectGuid.ToString(), "10.0", ProjectName);

            project += ImportDefaultProps;

            foreach (string config in Configurations)
            {
                foreach (string platform in Platforms)
                {
                    project += Property(config, platform, "Application", "MultiByte");
                }
            }

            project += ImportProps;

            foreach (string config in Configurations)
            {
                foreach (string platform in Platforms)
                {
                    string output = GetOutputDirectory(config, platform);
                    string intdir = GetIntDirectory(config, platform);

                    string includeDirectories = GetIncludeDirectories(config, platform);

                    project += ConfigOutputs(config, platform, output, intdir, includeDirectories);
                }
            }

            foreach (string config in Configurations)
            {
                foreach (string platform in Platforms)
                {
                    project += ItemDefinition(config, platform);
                }
            }

            if (Includes.Count != 0)
            {
                project += ItemGroup0;
                foreach (string include in Includes)
                {
                    project += Include(include);
                }
                project += ItemGroup1;
            }

            if (Sources.Count != 0)
            {
                project += ItemGroup0;
                foreach (string source in Sources)
                {
                    project += Source(source);
                }
                project += ItemGroup1;
            }

            if (Resources.Count != 0)
            {
                project += ItemGroup0;
                foreach (string resource in Resources)
                {
                    project += Resource(resource);
                }
                project += ItemGroup1;
            }

            project += ImportTargets;

            project += ImportGroup;

            project += Project1;

            return project;
        }

        /// <summary>
        /// Create a VSProject instance.
        /// </summary>
        /// <param name="ProjectName">Name of the vcxproj file.</param>
        /// <param name="ProjectDirectory">Where to write project files.</param>
        /// <param name="SourceDirectory">Soures to add to this project if empty sources will be added manually.</param>
        public VSProject(string ProjectName, string ProjectDirectory, string SourceDirectory = "")
        {
            // We have to allow the project files to be generated away from the sources
            _projectName = ProjectName;

            // We need to make sure paths have a tailing \
            if (!ProjectDirectory.EndsWith('\\'))
                ProjectDirectory += '\\';

            this.ProjectDirectory = new Uri(ProjectDirectory);

            if (SourceDirectory != string.Empty)
            {
                if (!SourceDirectory.EndsWith('\\'))
                    SourceDirectory += '\\';

                this.SourceDirectory = new Uri(SourceDirectory);

                WalkProject();
            }
        }

        /// <summary>
        /// Get the final path to the .vcxproj
        /// </summary>
        /// <returns>Absolute path to .vcxproj</returns>
        public string GetPath()
        {
            return Path.Join(ProjectDirectory!.AbsolutePath, ProjectName + ".vcxproj");
        }

        /// <summary>
        /// Get the final path to the .vcxproj
        /// </summary>
        /// <returns>Absolute path to .vcxproj</returns>
        public string GetDirectory()
        {
            return ProjectDirectory!.AbsolutePath;
        }

        /// <summary>
        /// Get the final path to the .vcxproj.filters
        /// </summary>
        /// <returns>Absolute path to .vcxproj.filters</returns>
        public string GetFilterPath()
        {
            return Path.Join(ProjectDirectory!.AbsolutePath, ProjectName + ".vcxproj.filters");
        }

        /// <summary>
        /// Generate and project file and write to disk.
        /// </summary>
        public void Generate()
        {
            string projectFile = GetPath();

            using (TextWriter tw = new StreamWriter(projectFile))
            {
                tw.Write(BuildOutput());
                tw.Flush();
                tw.Close();
            }

            Tabify.Xml(projectFile);

            string filterFile = GetFilterPath();

            VSFilters filters = new(filterFile, VSConfig.Version)
            {
                ProjectDirectory = ProjectDirectory,
                SourceDirectory = SourceDirectory,
                Includes = Includes,
                Sources = Sources,
                Resources = Resources,
                Filters = Filters
            };

            filters.Generate();
        }
    }
}
