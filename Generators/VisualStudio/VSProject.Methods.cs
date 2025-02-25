using BuildTool.Generators.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BuildTool.Generators.VisualStudio
{
    public partial class VSProject : IGenerator
    {
        // 0 - Config, 1 - Platform
        private string Configuration(string Config, string Platform)
        {
            return string.Format(ConfigurationT, Config, Platform);
        }

        private string ConfigOutputs(string Config, string Platform, [Optional] string OutputDir, [Optional] string IntDir, [Optional] string IncludeDirectories)
        {
            StringBuilder sb = new();

            sb.Append(string.Format(ConfigOutputsStart, Config, Platform));

            if (OutputDir != default)
            {
                sb.Append(string.Format(ConfigOutputOutDir, OutputDir));
            }

            if (IntDir != default)
            {
                sb.Append(string.Format(ConfigOutputIntDir, IntDir));
            }

            if (IncludeDirectories != default)
            {
                sb.Append(string.Format(ConfigOutputIncludeDir, IncludeDirectories));
            }

            sb.Append(ConfigOutputsEnd);

            return sb.ToString();
        }

        // 0 - GUID, 1 - Root Namespace, 2 - Platform Target Version 10.0
        private string Globals(string ProjectGuid, string SDKVersion, string Name)
        {
            return string.Format(GlobalsT, ProjectGuid, Name, SDKVersion);
        }

        // 0 - Configuration, 1 - Architecture, 2 - Configuration Type, 3 - CharacterSet
        private string Property(string Configuration, string Architecture, string ConfigurationType, string CharacterSet)
        {
            // HACK: Needed to manually set toolset version to make arm compiles work out of box
            bool IsArm = Architecture.Contains("arm", StringComparison.CurrentCultureIgnoreCase);

            if (VSConfig.IsDebug(Configuration))
            {
                if (IsArm)
                {
                    string PropertyDebugT = ""
                     + "  <PropertyGroup Label=\"Configuration\" Condition=\"\'$(Configuration)|$(Platform)\'==\'{0}|{1}\'\">\n"
                     + "    <ConfigurationType>{2}</ConfigurationType>\n"
                     + "    <UseDebugLibraries>true</UseDebugLibraries>\n"
                     + $"   <PlatformToolset>{VSConfig.Toolset}</PlatformToolset>\n"
                     + "    <CharacterSet>{3}</CharacterSet>\n"
                     + "    <VCToolsVersion>14.43.34808</VCToolsVersion>\n"
                     + "  </PropertyGroup>\n";

                    return string.Format(PropertyDebugT, Configuration, Architecture, ConfigurationType, CharacterSet);
                }
                else
                    return string.Format(PropertyDebugT, Configuration, Architecture, ConfigurationType, CharacterSet);
            }
            else
            {
                if (IsArm)
                {
                    string PropertyReleaseT = ""
                     + "  <PropertyGroup Label=\"Configuration\" Condition=\"\'$(Configuration)|$(Platform)\'==\'{0}|{1}\'\">\n"
                     + "    <ConfigurationType>{2}</ConfigurationType>\n"
                     + "    <UseDebugLibraries>true</UseDebugLibraries>\n"
                     + $"   <PlatformToolset>{VSConfig.Toolset}</PlatformToolset>\n"
                     + "    <CharacterSet>{3}</CharacterSet>\n"
                     + "    <VCToolsVersion>14.43.34808</VCToolsVersion>\n"
                     + "  </PropertyGroup>\n";

                    return string.Format(PropertyReleaseT, Configuration, Architecture, ConfigurationType, CharacterSet);
                }
                else
                    return string.Format(PropertyReleaseT, Configuration, Architecture, ConfigurationType, CharacterSet);
            }
        }

        // 0 - Configuration, 1 - Architecture
        private string ItemDefinition(string Configuration, string Platform)
        {
            if (VSConfig.IsDebug(Configuration))
            {
                return string.Format(ItemDefenitionDebugT, Configuration, Platform);
            }
            else
            {
                return string.Format(ItemDefenitionReleaseT, Configuration, Platform);
            }
        }

        /// <summary>
        /// Add include directory string.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Relative"></param>
        public void AddIncludeFolder(string Path, string Configuration = "all", string Platform = "all", bool Relative = false)
        {
            string key = $"{Configuration}|{Platform}".ToLower();

             _includeDirectories.TryAdd(key, string.Empty);

            if (Relative)
            {
                Uri absolutePath = new(Path);
                string relPath = ProjectDirectory!.MakeRelativeUri(absolutePath).ToString();

                _includeDirectories[key] += $"{relPath};";
            }
            else
                _includeDirectories[key] += $"{Path};";
        }

        /// <summary>
        /// Get include directory string.
        /// </summary>
        /// <param name="Configuration">Specific configuration or all</param>
        /// <param name="Platform">Specific platform or all</param>
        /// <returns></returns>
        private string GetIncludeDirectories(string Configuration, string Platform)
        {
            StringBuilder sb = new();

            // Find any that match all|all
            string all = "all|all";
            if (_includeDirectories.TryGetValue(all, out string? value))
            {
                sb.Append(value);
            }

            // Find any that match all|Platform
            string platformAll = $"all|{Platform}".ToLower();
            if (_includeDirectories.TryGetValue(platformAll, out value))
            {
                sb.Append(value);
            }

            // Find any that match Configuration|all
            string configAll = $"{Configuration}|all".ToLower();
            if (_includeDirectories.TryGetValue(configAll, out value))
            {
                sb.Append(value);
            }

            // Find any that match Configuration|Platform
            string key = $"{Configuration}|{Platform}".ToLower();

            if (_includeDirectories.TryGetValue(key, out value))
            {
                sb.Append(value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Add a compile definition for the specific configuration and platform.
        /// </summary>
        /// <param name="Configuration">Specific configuration or all</param>
        /// <param name="Platform">Specific platform or all</param>
        /// <param name="Value">Value to be added to compile command line</param>
        public void AddCompileDefinition(string Value, string Configuration = "all", string Platform = "all")
        {
            string key = $"{Configuration}|{Platform}".ToLower();

            if (_compilerDefinitions.ContainsKey(key))
            {
                _compilerDefinitions[key] += $"{Value};";
            }
            else
            {
                _compilerDefinitions.Add(key, $"{Value};");
            }
        }

        /// <summary>
        /// Add a linker definition for the specific configuration and platform.
        /// </summary>
        /// <param name="Configuration">Specific configuration or all</param>
        /// <param name="Platform">Specific platform or all</param>
        /// <param name="Value">Value to be added to compile command line</param>
        public void AddLinkerDefinition(string Value, string Configuration = "all", string Platform = "all")
        {
            string key = $"{Configuration}|{Platform}".ToLower();

            if (_linkerDefinitions.ContainsKey(key))
            {
                _linkerDefinitions[key] += $"{Value};";
            }
            else
            {
                _linkerDefinitions.Add(key, $"{Value};");
            }
        }

        /// <summary>
        /// Get compile definitions for the target configuration/platform.
        /// </summary>
        /// <param name="Configuration">Specific configuration or alle</param>
        /// <param name="Platform">Specific platform or all</param>
        /// <returns></returns>
        public string GetCompileDefinitions(string Configuration, string Platform)
        {
            StringBuilder sb = new();

            // Find any that match all|all
            string all = "all|all";
            if (_compilerDefinitions.TryGetValue(all, out string? value))
            {
                sb.Append(value);
            }

            // Find any that match all|Platform
            string platformAll = $"all|{Platform}".ToLower();
            if (_compilerDefinitions.TryGetValue(platformAll, out value))
            {
                sb.Append(value);
            }

            // Find any that match Configuration|all
            string configAll = $"{Configuration}|all".ToLower();
            if (_compilerDefinitions.TryGetValue(configAll, out value))
            {
                sb.Append(value);
            }

            // Find any that match Configuration|Platform
            string key = $"{Configuration}|{Platform}".ToLower();

            if (_compilerDefinitions.TryGetValue(key, out value))
            {
                sb.Append(value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get linker definitions for the target configuration/platform.
        /// </summary>
        /// <param name="Configuration">Specific configuration or all</param>
        /// <param name="Platform">Specific platform or all</param>
        /// <returns></returns>
        public string GetLinkerDefinitions(string Configuration, string Platform)
        {
            StringBuilder sb = new();

            // Find any that match all|all
            string all = "all|all";
            if (_linkerDefinitions.TryGetValue(all, out string? value))
            {
                sb.Append(value);
            }

            // Find any that match all|Platform
            string platformAll = $"all|{Platform}".ToLower();
            if (_linkerDefinitions.TryGetValue(platformAll, out value))
            {
                sb.Append(value);
            }

            // Find any that match Configuration|all
            string configAll = $"{Configuration}|all".ToLower();
            if (_linkerDefinitions.TryGetValue(configAll, out value))
            {
                sb.Append(value);
            }

            // Find any that match Configuration|Platform
            string key = $"{Configuration}|{Platform}".ToLower();

            if (_linkerDefinitions.TryGetValue(key, out value))
            {
                sb.Append(value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Output formatted Include string.
        /// </summary>
        /// <param name="Name">Include filepath</param>
        private string Include(string Name)
        {
            return string.Format(IncludesT, Name);
        }

        /// <summary>
        /// Output formatted Source string.
        /// </summary>
        /// <param name="Name">Source filepath</param>
        private string Source(string Name)
        {
            return string.Format(SourcesT, Name);
        }

        /// <summary>
        /// Output formatted Resource string.
        /// </summary>
        /// <param name="Name">Resource filepath</param>
        private string Resource(string Name)
        {
            return string.Format(ResourcesT, Name);
        }

        /// <summary>
        /// Add filter path for project.
        /// </summary>
        /// <param name="Path"></param>
        public void AddFilter(string Path)
        {
            // We need to find a work around for manual filters
            if (SourceDirectory == null)
                return;

            Uri absolutePath = new(Path);
            string relPath = SourceDirectory!.MakeRelativeUri(absolutePath).ToString();

            string filt = relPath.Replace('/', '\\').Replace("..\\", "").Replace(".\\", "");

            if (filt == string.Empty || filt.Contains('.'))
                return;

            if (!Filters.Contains(filt))
                _filters.Add(filt);
        }

        /// <summary>
        /// Add header file to project.
        /// </summary>
        /// <param name="Path">Header filepath</param>
        public void AddHeader(string Path)
        {
            Uri absolutePath = new(Path);
            string relPath = ProjectDirectory!.MakeRelativeUri(absolutePath).ToString();

            _includes.Add(relPath);
        }

        /// <summary>
        /// Add source file to project.
        /// </summary>
        /// <param name="Path">Source filepath</param>
        public void AddSource(string Path)
        {
            Uri absolutePath = new(Path);
            string relPath = ProjectDirectory!.MakeRelativeUri(absolutePath).ToString();

            _sources.Add(relPath);
        }

        /// <summary>
        /// Add resource to project.
        /// </summary>
        /// <param name="Path">Resource filepath</param>
        public void AddResource(string Path)
        {
            Uri absolutePath = new(Path);
            string relPath = ProjectDirectory!.MakeRelativeUri(absolutePath).ToString();

            _resources.Add(relPath);
        }

        /// <summary>
        /// Use filters for file type add it if possible.
        /// </summary>
        /// <param name="Path">Filepath</param>
        public void AddFile(string Path)
        {
            string ext = System.IO.Path.GetExtension(Path);

            if (VSConfig.HeaderExtensions.Contains(ext))
            {
                AddHeader(Path);
            }
            else if (VSConfig.SourceExtensions.Contains(ext))
            {
                AddSource(Path);
            }
            else if (VSConfig.ResourceExtensions.Contains(ext))
            {
                AddResource(Path);
            }
        }

        // This might actually be for source directory
        private void SetProjectDirectory(string ProjectDirectory)
        {
            string finalDirectory = ProjectDirectory;

            if (!finalDirectory.EndsWith('\\'))
                finalDirectory += '\\';

            this.ProjectDirectory = new Uri(finalDirectory);
        }

        /// <summary>
        /// Set the output directory for a configuration.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Configuration"></param>
        /// <param name="Platform"></param>
        /// <param name="Relative"></param>
        public void SetOutputDir(string Path, string Configuration = "all", string Platform = "all", bool Relative = false)
        {
            string key = $"{Configuration}|{Platform}".ToLower();

            _ = _outputDirectory.TryAdd(key, string.Empty);

            if (Relative)
            {
                Uri absolutePath = new(Path);
                string relPath = ProjectDirectory!.MakeRelativeUri(absolutePath).ToString();

                _outputDirectory[key] = relPath;
            }
            else
                _outputDirectory[key] = Path;
        }

        /// <summary>
        /// Get the output directory for a configuration.
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="Platform"></param>
        /// <returns>Output Directory String</returns>
        public string GetOutputDirectory(string Configuration = "all", string Platform = "all")
        {
            string key = $"{Configuration}|{Platform}".ToLower();

            if (_outputDirectory.TryGetValue(key, out string? value))
                return value;

            // Try to see if we have an all value we can use
            string allKey = "all|all";

            if (_outputDirectory.TryGetValue(allKey, out value))
                return value;

            return string.Empty;
        }

        /// <summary>
        /// Set the intermediate directory for a configuration.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Configuration"></param>
        /// <param name="Platform"></param>
        /// <param name="Relative"></param>
        public void SetIntDir(string Path, string Configuration = "all", string Platform = "all", bool Relative = false)
        {
            string key = $"{Configuration}|{Platform}".ToLower();

            // Overwrite
            _ = _intDirectory.TryAdd(key, string.Empty);

            if (Relative)
            {
                Uri absolutePath = new(Path);
                string relPath = ProjectDirectory!.MakeRelativeUri(absolutePath).ToString();

                _intDirectory[key] = relPath;
            }
            else
                _intDirectory[key] = Path;
        }

        /// <summary>
        /// Get the intermediate directory for a configuration.
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="Platform"></param>
        /// <returns>Intermediate Directory String</returns>
        public string GetIntDirectory(string Configuration = "all", string Platform = "all")
        {
            string key = $"{Configuration}|${Platform}".ToLower();

            if (_intDirectory.TryGetValue(key, out string? value))
                return value;

            // Try to see if we have an all value we can use
            string allKey = "all|all";

            if (_intDirectory.TryGetValue(allKey, out value))
                return value;

            return string.Empty;
        }

        /// <summary>
        /// Walk the directory structure adding files that match our conditions.
        /// </summary>
        /// <param name="Path">Directory Path</param>
        private void Walk(string Path)
        {
            string folderName = new DirectoryInfo(Path).Name;
            if (VSConfig.IgnoreDirectories.Contains(folderName))
                return;

            string[] files = Directory.GetFiles(Path);

            foreach (string file in files)
            {
                AddFile(file);
            }

            string[] directories = Directory.GetDirectories(Path);

            foreach (string dir in directories)
            {
                folderName = new DirectoryInfo(dir).Name;
                if (VSConfig.IgnoreDirectories.Contains(folderName))
                    continue;

                AddFilter(dir);
                Walk(dir);
            }
        }

        /// <summary>
        /// Walk the entire source directory for the project.
        /// </summary>
        private void WalkProject()
        {
            Walk(SourceDirectory!.AbsolutePath);
        }
    }
}
