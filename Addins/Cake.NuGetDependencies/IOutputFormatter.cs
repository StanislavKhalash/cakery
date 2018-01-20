namespace Cake.NuGetDependencies
{
    public interface IOutputFormatter
    {
        void AppendPackageInfo(NuGet.ZipPackage package);

        string GetOutput();
    }
}
