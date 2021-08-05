using System;
using System.IO;

namespace AGSUnpackerTests.Extensions
{
  public static class PathExtensions
  {
    //FIXME(adm244): probably bugged
    public static string GetLastDirectoryName(string path)
    {
      const string argPath = "path";

      if (path == null)
        throw new ArgumentNullException(argPath);

      if ((path == string.Empty) || string.IsNullOrWhiteSpace(path))
        throw new ArgumentException("Path is empty.", argPath);

      char[] separators = 
      {
        Path.DirectorySeparatorChar,
        Path.AltDirectorySeparatorChar
      };

      string[] parts = path.Split(separators, StringSplitOptions.RemoveEmptyEntries);
      return parts[parts.Length - 1];
    }
  }
}
