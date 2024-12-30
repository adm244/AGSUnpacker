using System.Reflection;

namespace AGSUnpacker.UI
{
  internal static class AppDescription
  {
    private static readonly Assembly RunningAssembly = typeof(App).Assembly;

    private static readonly AssemblyInformationalVersionAttribute AssemblyInfo
      = RunningAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

    private static readonly AssemblyConfigurationAttribute AssemblyConfiguration
      = RunningAssembly.GetCustomAttribute<AssemblyConfigurationAttribute>();

    private static readonly AssemblyTitleAttribute AssemblyTitle
      = RunningAssembly.GetCustomAttribute<AssemblyTitleAttribute>();

    private static readonly BuildInfoAttribute BuildInfo
      = RunningAssembly.GetCustomAttribute<BuildInfoAttribute>();

    private static readonly AssemblyFileVersionAttribute AssemblyVersion
      = RunningAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>();

    public static string ProgramName => AssemblyTitle.Title;

    public static string ProgramConfiguration => $"({AssemblyConfiguration.Configuration})";

    public static string ProgramVersion {
      get {
        if (string.IsNullOrEmpty(BuildInfo?.PublicReleaseVersion)) {
          return $"v{AssemblyInfo.InformationalVersion}-{BuildInfo.Architecture}";
        } else {
          string[] parts = AssemblyVersion.Version.Split('.');
          return $"v{parts[0]}.{parts[1]}-{BuildInfo.Architecture}";
        }
      }
    }
  }
}
