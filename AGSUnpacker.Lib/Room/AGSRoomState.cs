
namespace AGSUnpacker.Room
{
  public class AGSRoomState
  {
    public byte StartupMusicID;
    public byte MusicVolume;
    public bool IsSaveLoadDisabled;
    public bool IsPlayerInvisible;
    public byte PlayerViewID;

    public AGSRoomState()
    {
      StartupMusicID = 0;
      MusicVolume = 0;
      IsSaveLoadDisabled = false;
      IsPlayerInvisible = false;
      PlayerViewID = 0;
    }
  }
}
