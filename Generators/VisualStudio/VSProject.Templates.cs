using BuildTool.Generators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildTool.Generators.VisualStudio
{
    public partial class VSProject : IGenerator
    {
        private string Header = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
        private string Project0 = $"<Project DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n";
        private string Project1 = "</Project>";
        private string ProjectConfigurations0 = "   <ItemGroup Label=\"ProjectConfigurations\">\n";
        private string ProjectConfigurations1 = "   </ItemGroup>\n";

        // 0 - Config, 1 - Platform
        private string ConfigurationT = ""
                                        + " <ProjectConfiguration Include=\"{0}|{1}\">\n"
                                        + "     <Configuration>{0}</Configuration>\n"
                                        + "     <Platform>{1}</Platform>\n"
                                        + " </ProjectConfiguration>\n";

        private string ConfigOutputsStart = " <PropertyGroup Condition=\"'$(Configuration)|$(Platform)'=='{0}|{1}'\" >\n";
        private string ConfigOutputIntDir = "   <IntDir>{0}</IntDir>\n";
        private string ConfigOutputOutDir = "   <OutDir>{0}</OutDir>\n";
        private string ConfigOutputIncludeDir = "   <IncludePath>{0}$(IncludePath)</IncludePath>\n";
        private string ConfigOutputsEnd = " </PropertyGroup>\n";

        // 0 - GUID, 1 - Root Namespace, 2 - Platform Target Version 10.0
        private string GlobalsT = ""
                                + " <PropertyGroup Label=\"Globals\">\n"
                                + "     <VCProjectVersion>17.0</VCProjectVersion>\n"
                                + "     <Keyword>Win32Proj</Keyword>\n"
                                + "     <ProjectGuid>{{{0}}}</ProjectGuid>\n"
                                + "     <RootNamespace>{1}</RootNamespace>\n"
                                + "     <WindowsTargetPlatformVersion>{2}</WindowsTargetPlatformVersion>\n"
                                + " </PropertyGroup>\n";

        // 0 - Configuration, 1 - Architecture, 2 - Configuration Type, 3 - CharacterSet
        private string PropertyDebugT = ""
                     + "  <PropertyGroup Label=\"Configuration\" Condition=\"\'$(Configuration)|$(Platform)\'==\'{0}|{1}\'\">\n"
                     + "    <ConfigurationType>{2}</ConfigurationType>\n"
                     + "    <UseDebugLibraries>true</UseDebugLibraries>\n"
                     + $"   <PlatformToolset>{VSConfig.Toolset}</PlatformToolset>\n"
                     + "    <CharacterSet>{3}</CharacterSet>\n"
                     + "  </PropertyGroup>\n";

        private string PropertyReleaseT = ""
                     + "  <PropertyGroup Label=\"Configuration\" Condition=\"\'$(Configuration)|$(Platform)\'==\'{0}|{1}\'\">\n"
                     + "    <ConfigurationType>{2}</ConfigurationType>\n"
                     + "    <UseDebugLibraries>false</UseDebugLibraries>\n"
                     + $"   <PlatformToolset>{VSConfig.Toolset}</PlatformToolset>\n"
                     + "    <CharacterSet>{3}</CharacterSet>\n"
                     + "  </PropertyGroup>\n";

        // TODO: Allow the C++ and C standard to be configurable
        // TODO: Also allow for preprocessor defs to be added
        // 0 - Configuration, 1 - Architecture
        private string ItemDefenitionDebugT = ""
                                            + " <ItemDefinitionGroup Condition=\"\'$(Configuration)|$(Platform)\'==\'{0}|{1}\'\">\n"
                                            + "     <ClCompile>\n"
                                            + "       <WarningLevel>Level3</WarningLevel>\n"
                                            + "       <SDLCheck>true</SDLCheck>\n"
                                            + "       <PreprocessorDefinitions>PLATFORM_WINDOWS;%(PreprocessorDefinitions)</PreprocessorDefinitions>\n"
                                            + "       <LanguageStandard>stdcpp17</LanguageStandard>\n"
                                            + "       <LanguageStandard_C>stdc17</LanguageStandard_C>\n"
                                            + "     </ClCompile>\n"
                                            + "     <Link>\n"
                                            + "         <GenerateDebugInformation>true</GenerateDebugInformation>\n"
                                            + "         <SubSystem>Console</SubSystem>\n"
                                            + "     </Link>\n"
                                            + " </ItemDefinitionGroup>\n";

        // 0 - Configuration, 1 - Architecture
        private string ItemDefenitionReleaseT = ""
                                              + " <ItemDefinitionGroup Condition=\"\'$(Configuration)|$(Platform)\'==\'{0}|{1}\'\">\n"
                                              + "   <ClCompile>\n"
                                              + "       <WarningLevel>Level3</WarningLevel>\n"
                                              + "       <Optimization>MaxSpeed</Optimization>\n"
                                              + "       <FunctionLevelLinking>true</FunctionLevelLinking>\n"
                                              + "       <SDLCheck>true</SDLCheck>\n"
                                              + "       <PreprocessorDefinitions>PLATFORM_WINDOWS;%(PreprocessorDefinitions)</PreprocessorDefinitions>\n"
                                              + "       <LanguageStandard>stdcpp17</LanguageStandard>\n"
                                              + "       <LanguageStandard_C>stdc17</LanguageStandard_C>\n"
                                              + "   </ClCompile>\n"
                                              + "   <Link>\n"
                                              + "       <GenerateDebugInformation>true</GenerateDebugInformation>\n"
                                              + "       <OptimizeReferences>true</OptimizeReferences>\n"
                                              + "       <SubSystem>Console</SubSystem>\n"
                                              + "   </Link>\n"
                                              + " </ItemDefinitionGroup>\n";

        private string ItemGroup0 = "   <ItemGroup>\n";
        private string ItemGroup1 = "   </ItemGroup>\n";

        private string IncludesT = " <ClInclude Include=\"{0}\" />\n";
        private string SourcesT = " <ClCompile Include=\"{0}\" />\n";
        private string ResourcesT = " <ResourceCompile Include=\"{0}\" />\n";

        private string ImportTargets = "    <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.targets\" />\n";
        private string ImportProps = "  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.props\" />\n";
        private string ImportDefaultProps = "  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.Default.props\" />\n";

        private string ImportGroup = "  <ImportGroup Label=\"ExtensionTargets\">\r\n  </ImportGroup>";
    }
}
