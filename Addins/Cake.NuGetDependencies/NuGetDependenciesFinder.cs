using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Cake.NuGetDependencies
{
    public class NuGetDependenciesFinder
    {
        private readonly DirectoryPath _targetDirectoryPath;
        private readonly DirectoryPath _workingDirectoryPath;
        private readonly IFileSystem _fileSystem;
        private readonly ICakeLog _log;

        public NuGetDependenciesFinder(
            DirectoryPath targetDirectoryPath, 
            DirectoryPath workingDirectoryPath,
            IFileSystem fileSystem, 
            ICakeLog log)
        {
            _targetDirectoryPath = targetDirectoryPath;
            _workingDirectoryPath = workingDirectoryPath;
            _fileSystem = fileSystem;
            _log = log;
        }

        public void Run()
        {
            var referencedPackages = GetReferencedPackages();

            var localNugetCacheDirectory = _fileSystem
                .GetDirectory(_workingDirectoryPath)
                .GetDirectories("packages", SearchScope.Current)
                .First();

            foreach (var packageName in referencedPackages)
            {
                var searchFilter = $"{packageName}*";

                var packageDirectory = localNugetCacheDirectory.GetDirectories(searchFilter, SearchScope.Current).Last();
                var packageFile = packageDirectory.GetFiles(searchFilter, SearchScope.Current).Last();

                DumpPackageInfo(packageFile);
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

        private void DumpPackageInfo(IFile packageFile)
        {
            var fileStream = packageFile.Open(FileMode.Open, FileAccess.Read);
            var zipPackage = new NuGet.ZipPackage(fileStream);

            _log.Information(zipPackage.Description);
            _log.Information(zipPackage.LicenseUrl);
        }
    }
}
