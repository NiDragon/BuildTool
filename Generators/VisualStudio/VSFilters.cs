using BuildTool.Generators.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace BuildTool.Generators.VisualStudio
{
    public partial class VSProject : IGenerator
    {
        class VSFilters : IGenerator
        {
            private string Header = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
            // 2022 - 17, 2019 - 16
            private string BeginFilterBody = $"<Project ToolsVersion=\"{0}.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n";
            private string EndFilterBody = "</Project>\n";

            // Containg tag for each diffrent item group (Sources, Includes, Resources, ...)
            private string BeginItemGroup = "  <ItemGroup>\n";
            private string EndItemGroup = "  </ItemGroup>\n";

            private string ClIncludeNoFilterT = ""
                + "    <ClInclude Include=\"{0}\" />\n";

            private string ClSourceNoFilterT = ""
                + "    <ClCompile Include=\"{0}\" />\n";

            private string ClResourceNoFilterT = ""
                + "    <ResourceCompile Include=\"{0}\" />\n";

            private string IncludesT = ""
                + "    <ClInclude Include=\"{0}\">\n"
                + "      <Filter>{1}</Filter>\n"
                + "    </ClInclude>\n";

            private string SourcesT = ""
                + "    <ClCompile Include=\"{0}\">\n"
                + "      <Filter>{1}</Filter>\n"
                + "    </ClCompile>\n";

            private string ResourceT = ""
                + "    <ResourceCompile Include=\"{0}\">\n"
                + "      <Filter>{1}</Filter>\n"
                + "    </ResourceCompile>\n";

            private string FilterT = ""
                + "    <Filter Include=\"{0}\">\n"
                + "      <UniqueIdentifier>{{{1}}}</UniqueIdentifier>\n"
                + "    </Filter>\n";

            private string Filename = string.Empty;

            private List<string> _filters = [];
            private List<string> _includes = [];
            private List<string> _sources = [];
            private List<string> _resources = [];

            public IReadOnlyList<string> Includes { get => _includes; set => _includes = [.. value]; }
            public IReadOnlyList<string> Sources { get => _sources; set => _sources = [.. value]; }
            public IReadOnlyList<string> Resources { get => _resources; set => _resources = [.. value]; }
            public IReadOnlyList<string> Filters { get => _filters; set => _filters = [.. value]; }

            // For filtered matching...
            public Uri? ProjectDirectory { get; internal set; }
            public Uri? SourceDirectory { get; internal set; }

            /// <summary>
            /// Create an instance of the VSFilters generator.
            /// </summary>
            /// <param name="Filename">A filename including path</param>
            /// <param name="Version">Project File Version</param>
            public VSFilters(string Filename, string Version)
            {
                this.Filename = Filename;
                BeginFilterBody = string.Format(BeginFilterBody, Version);
            }

            private static string GetFilter(string Filepath)
            {
                return Path.GetDirectoryName(Filepath)!.Replace('/', '\\').Replace("..\\", "").Replace(".\\", "");
            }

            private string ClIncludeNoFilter(string Path)
            {
                return string.Format(ClIncludeNoFilterT, Path);
            }

            private string ClSourceNoFilter(string Path)
            {
                return string.Format(ClSourceNoFilterT, Path);
            }

            private string ClResourceNoFilter(string Path)
            {
                return string.Format(ClResourceNoFilterT, Path);
            }

            private string Include(string Path, string Filter)
            {
                Filter = MakeForFilters(Filter);
                return string.Format(IncludesT, Path, Filter);
            }

            private string Source(string Path, string Filter)
            {
                Filter = MakeForFilters(Filter);
                return string.Format(SourcesT, Path, Filter);
            }

            private string Resource(string Path, string Filter)
            {
                Filter = MakeForFilters(Filter);
                return string.Format(ResourceT, Path, Filter);
            }

            private string Filter(string Path)
            {
                return string.Format(FilterT, Path, Guid.NewGuid().ToString().ToUpper());
            }

            /// <summary>
            /// The filters require FilePath matches on file and filter.
            /// Include="Path/To/File" Must match whatever the project has for a path on that file.
            /// <Filter>Path\To\File\Filter</Filter> has to match our filter string with opposite slashes.
            /// </summary>
            /// <param name="FilePath"></param>
            /// <returns>Adjusted path if SourceDirectory is not null otherwise FilePath.</returns>
            private string MakeForFilters(string FilePath)
            {
                if (SourceDirectory != null)
                {
                    Uri absolutePath = new(Path.Join(ProjectDirectory!.ToString(), FilePath));

                    return SourceDirectory
                        .MakeRelativeUri(absolutePath)
                        .ToString()
                        .Replace('/', '\\')
                        .Replace("..\\", "")
                        .Replace(".\\", "");
                }

                return FilePath;
            }

            /// <summary>
            /// This is going to require some refractoring!
            /// </summary>
            /// <returns>String containing the contents of vcxproj.filters</returns>
            private string BuildOutput()
            {
                string filters = string.Empty;

                filters += Header;
                filters += BeginFilterBody;

                // Filters
                filters += BeginItemGroup;
                foreach (string folder in Filters)
                {
                    filters += Filter(folder);
                }
                filters += EndItemGroup;

                filters += BeginItemGroup;
                foreach (string include in Includes)
                {
                    string filter = GetFilter(include);

                    if (filter != string.Empty)
                    {
                        filters += Include(include, filter);
                    }
                    else
                    {
                        filters += ClIncludeNoFilter(include);
                    }
                }
                filters += EndItemGroup;

                filters += BeginItemGroup;
                foreach (string source in Sources)
                {
                    string filter = GetFilter(source);

                    if (filter != string.Empty)
                    {
                        filters += Source(source, filter);
                    }
                    else
                    {
                        filters += ClSourceNoFilter(source);
                    }
                }
                filters += EndItemGroup;

                filters += BeginItemGroup;
                foreach (string resource in Resources)
                {
                    string filter = GetFilter(resource);

                    if (filter != string.Empty)
                    {
                        filters += Resource(resource, filter);
                    }
                    else
                    {
                        filters += ClResourceNoFilter(resource);
                    }
                }
                filters += EndItemGroup;

                filters += EndFilterBody;

                return filters;
            }

            public void Generate()
            {
                string output = BuildOutput();

                using (TextWriter tw = new StreamWriter(Filename))
                {
                    tw.Write(output);
                    tw.Flush();
                    tw.Close();
                }

                Tabify.Xml(Filename);
            }
        }
    }
}
