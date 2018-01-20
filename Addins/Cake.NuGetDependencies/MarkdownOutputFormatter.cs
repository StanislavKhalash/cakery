using MarkdownLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cake.NuGetDependencies
{
    public sealed class MarkdownOutputFormatter : IOutputFormatter
    {
        private class PackageInfo
        {
            public string Name { get; set; }

            public string LicenseUrl { get; set; }

            public string ProjectUrl { get; set; }
        }

        private readonly List<PackageInfo> _packages = new List<PackageInfo>();

        public void AppendPackageInfo(NuGet.ZipPackage package)
        {
            _packages.Add(new PackageInfo
            {
                Name = package.Id,
                LicenseUrl = package.LicenseUrl.ToString(),
                ProjectUrl = package.ProjectUrl.ToString()
            });
        }

        public string GetOutput() => _packages.ToMarkdownTable().ToString();

        private static string Wrap(string longText)
        {
            const int maxLineLength = 30;

            var stringBuilder = new StringBuilder();

            for (int lineStart = 0; lineStart < longText.Length;)
            {
                int lineLength = Math.Min(maxLineLength, longText.Length - lineStart);

                stringBuilder.AppendLine(longText.Substring(lineStart, lineLength));

                lineStart += lineLength;
            }

            return stringBuilder.ToString();
        }
    }
}
