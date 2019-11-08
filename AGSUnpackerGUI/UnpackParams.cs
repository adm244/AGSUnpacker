using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGSUnpackerGUI
{
  public class UnpackParams
  {
    public event Action<bool> UnpackFinished;

    public string FilePath;
    public string TargetFolder;

    public UnpackParams(string filePath, string targetFolder)
    {
      FilePath = filePath;
      TargetFolder = targetFolder;
    }

    public void OnUnpackFinished(bool success)
    {
      if (UnpackFinished != null)
        UnpackFinished.Invoke(success);
    }
  }
}
