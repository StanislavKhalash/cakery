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

            public string License { get; set; }

            public string Description { get; set; }
        }

        private readonly List<PackageInfo> _packages = new List<PackageInfo>();

        public void AppendPackageInfo(string packageId, string licenseUrl, string description)
        {
            _packages.Add(new PackageInfo
            {
                Name = packageId,
                License = licenseUrl,
                Description = Wrap(description)
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
