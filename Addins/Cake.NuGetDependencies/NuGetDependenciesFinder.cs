using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cake.Core.IO;

namespace Cake.NuGetDependencies
{
    public class NuGetDependenciesFinder
    {
        private readonly DirectoryPath _targetDirectoryPath;
        private readonly DirectoryPath _workingDirectoryPath;
        private readonly IFileSystem _fileSystem;
        private readonly IOutputFormatter _outputFormatter;

        public NuGetDependenciesFinder(
            DirectoryPath targetDirectoryPath, 
            DirectoryPath workingDirectoryPath,
            IFileSystem fileSystem, 
            IOutputFormatter outputFormatter)
        {
            _targetDirectoryPath = targetDirectoryPath;
            _workingDirectoryPath = workingDirectoryPath;
            _fileSystem = fileSystem;
            _outputFormatter = outputFormatter;
        }

        public void Run()
        {
            var referencedPackages = GetReferencedPackages();

            var workingDirectory = _fileSystem.GetDirectory(_workingDirectoryPath);

            var localNugetCacheDirectory = workingDirectory
                .GetDirectories("packages", SearchScope.Current)
                .First();

            foreach (var packageName in referencedPackages)
            {
                var searchFilter = $"{packageName}*";

                var packageDirectory = localNugetCacheDirectory.GetDirectories(searchFilter, SearchScope.Current).Last();
                var packageFile = packageDirectory.GetFiles(searchFilter, SearchScope.Current).Last();

                var fileStream = packageFile.Open(FileMode.Open, FileAccess.Read);
                var zipPackage = new NuGet.ZipPackage(fileStream);

                _outputFormatter.AppendPackageInfo(
                    zipPackage.Id, 
                    zipPackage.LicenseUrl.AbsolutePath, 
                    zipPackage.Description);
            }

            var outputFilePath = workingDirectory.Path.CombineWithFilePath(new FilePath("nuget-dependencies.md"));

            using (var fileStream = _fileSystem.GetFile(outputFilePath).OpenWrite())
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(_outputFormatter.GetOutput());
                }
            }
        }

        private HashSet<string> GetReferencedPackages()
        {
            HashSet<string> packageNames = new HashSet<string>();

            var rootDirectory = _fileSystem.GetDirectory(_targetDirectoryPath);

            var packageReferenceFiles = rootDirectory
                .GetFiles("packages.config", SearchScope.Recursive)
                .Select(it => new NuGet.PackageReferenceFile(it.Path.FullPath));

            foreach (var packageReference in 
                packageReferenceFiles.SelectMany(it => it.GetPackageReferences()))
            {
                packageNames.Add(packageReference.Id);
            }

            return packageNames;
        }
    }
}
