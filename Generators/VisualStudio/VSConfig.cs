using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildTool.Generators.VisualStudio
{
    /// <summary>
    /// Some default configurations overriden for other instances of the Build Tool
    /// </summary>
    public static class VSConfig
    {
        // Version of Visual Studio we are generating for
        public static VSVersion Version = VSVersion.vs2022;
        public static VSToolsets Toolset = VSToolsets.vs2022;

        // Ingore generated directories
        public static string[] IgnoreDirectories = { ".vs", "Binaries", "Intermediate" };

        // Define Platforms and Configurations
        public static string[] Platforms = { "x64", "ARM64" };
        public static string[] Configurations = { "DebugGame", "DebugGame Editor", "Development", "Development Editor", "Shipping" };

        // The extensions we support
        public static string[] HeaderExtensions = { ".h", ".inl", ".hpp" };
        public static string[] SourceExtensions = { ".c", ".cc", ".cpp" };
        public static string[] ResourceExtensions = { ".rc" };

        // Extensions for content file types to keep them tracked in the project
        public static string[] ContentExtensions = { ".png", ".ico" };

        // Output file types
        public static string[] ConfigurationTypes = { "Application", "DynamicLibrary", "StaticLibrary" };

        // Project character set
        public static string[] CharacterSets = { "MultiByte", "Unicode" };

        // Compiler settings
        public static string CppStandard = "17";

        /// <summary>
        /// Check if the current configuration a debug config.
        /// </summary>
        /// <param name="Configuration">Configuration String</param>
        /// <returns>True if Configuration is debug.</returns>
        public static bool IsDebug(string Configuration)
        {
            return Configuration.ToLower().Contains("debug");
        }

        /// <summary>
        /// Create a new GUID for Solution/Project/Filters
        /// </summary>
        /// <returns></returns>
        public static string GetGUID() => Guid.NewGuid().ToString().ToUpper();
    }
}
