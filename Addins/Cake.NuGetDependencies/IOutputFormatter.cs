namespace Cake.NuGetDependencies
{
    public interface IOutputFormatter
    {
        void AppendPackageInfo(string packageId, string licenseUrl, string description);

        string GetOutput();
    }
}
