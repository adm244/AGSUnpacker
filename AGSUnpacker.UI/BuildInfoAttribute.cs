using System;

namespace AGSUnpacker.UI
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    internal sealed class BuildInfoAttribute : Attribute
    {
        public BuildInfoAttribute(string publicReleaseVersion, string architecture)
        {
            PublicReleaseVersion = publicReleaseVersion;
            Architecture = architecture;
        }

        public string PublicReleaseVersion { get; }
        public string Architecture { get; }
    }
}
