using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;

namespace Cake.NuGetDependencies
{
    public static class NuGetDependenciesAliases
    {
        [CakeMethodAlias]
        public static void NuGetDependencies(this ICakeContext context, DirectoryPath tagetDirectoryPath)
        {
            var runner = new NuGetDependenciesFinder(
                tagetDirectoryPath, 
                context.Environment.WorkingDirectory,
                context.FileSystem, 
                new MarkdownOutputFormatter());
            runner.Run();
        }
    }
}
