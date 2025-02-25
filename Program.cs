using BuildTool.Generators;
using BuildTool.Generators.VisualStudio;

using CommandLine;

namespace BuildTool
{
    internal class Program
    {
        public class Options
        {
            [Option('g', "generate", Required = false, HelpText = "Generate Visual Studio Project Files.", Group = "Generate")]
            public bool GenerateProjects { get; set; }

            [Option('s', "solution", Required = false, HelpText = "Name for a Visual Studio Solution.", Group = "Generate")]
            public string? SolutionName { get; set; }

            [Option('p', "projects", Required = false, HelpText = "Project Output Names.", Group = "Generate")]
            public IEnumerable<string>? ProjectNames { get; set; }

            [Option('o', "outputs", Required = false, HelpText = "Output Path For Project Files.", Group = "Generate")]
            public IEnumerable<string>? ProjectOutputDirectories { get; set; }

            [Option('i', "sources", Required = false, HelpText = "Project Source Directories.", Group = "Generate")]
            public IEnumerable<string>? ProjectSourceDirectories { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
               .WithParsed<Options>(o =>
               {
                   if(o.GenerateProjects) 
                   {
                       // We are going to generate a solution
                       if (o.SolutionName != null)
                       {
                           // Check that we have the required parameters
                           if(o.ProjectNames != null && o.ProjectOutputDirectories != null && o.ProjectSourceDirectories != null)
                           {
                               VSGenerator generator = new(o.SolutionName, VSConfig.Platforms, VSConfig.Configurations);

                               // Make sure our counts match
                               if (o.ProjectNames.Count() != o.ProjectOutputDirectories.Count())
                               {
                                   Console.WriteLine("Param Count Mismatch - [ProjectNames] [ProjectOutputDirectories].");
                                   return;
                               }

                               for (int i = 0; i < o.ProjectNames.Count(); i++)
                               {
                                   VSProject proj = new(o.ProjectNames.ElementAt(i),
                                                     o.ProjectOutputDirectories.ElementAt(i),
                                                     o.ProjectSourceDirectories.ElementAt(i));

                                   proj.SetOutputDir("$(SolutionDir)Binaries\\$(Platform)\\$(Configuration)\\");
                                   proj.SetIntDir("$(SolutionDir)Intermediate\\$(Platform)\\$(Configuration)\\$(ProjectName)\\");

                                   proj.AddIncludeFolder($"{o.ProjectSourceDirectories.ElementAt(i)}\\Public");
                                   proj.AddIncludeFolder($"{o.ProjectSourceDirectories.ElementAt(i)}\\Private");

                                   generator.AddProject(proj);
                               }

                               generator.Generate();
                           }
                       }
                       else
                       {
                       }
                   }
               });
        }
    }
}
