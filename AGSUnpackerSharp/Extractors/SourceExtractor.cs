using System;
using AGSUnpackerSharp.Extractors.SourceExtractors;

namespace AGSUnpackerSharp.Extractors
{
  //TODO(adm244): move to proper place
  public enum AGSVersion
  {
    AGS262
  }

  public abstract class SourceExtractor
  {
    protected SourceExtractor()
    {
    }

    public abstract bool Extract(string sourceFile);

    public static SourceExtractor Create(AGSVersion version)
    {
      switch (version)
      {
        case AGSVersion.AGS262:
          return new SourceExtractor262();

        default:
          throw new NotSupportedException();
      }
    }
  }
}
