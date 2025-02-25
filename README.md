# BuildTool

The goal of this project was to create a custom solution for creating Visual Studio and other projects for C/C++ tool chains and IDE without relying on CMake.

# Reasoning
While there exists many build systems, I felt that I was being done a disservice by build systems that tried to be flexible enough to accomplish any task. What I wanted to create was a no nonsense C/C++ build system that takes a lot of the leg work out of creating complex esoteric configuration scripts.

# Future Ideas
* Create other generators than Visual Studio such as XCode or CLion.
* Fully support project configuration through project files likely JSON based or C# build scripts.