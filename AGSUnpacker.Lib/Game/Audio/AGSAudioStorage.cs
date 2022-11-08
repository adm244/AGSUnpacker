using System;
using System.IO;

namespace AGSUnpacker.Lib.Game
{
  public class AGSAudioStorage
  {
    public AGSAudioType[] audioTypes;
    public AGSAudioClip[] audioClips;
    public Int32 score_clip_id;

    public AGSAudioStorage()
    {
      audioTypes = new AGSAudioType[0];
      audioClips = new AGSAudioClip[0];
      score_clip_id = 0;
    }

    public void LoadFromStream(BinaryReader r)
    {
      // parse audio types
      Int32 audiotypes_count = r.ReadInt32();
      audioTypes = new AGSAudioType[audiotypes_count];
      for (int i = 0; i < audioTypes.Length; ++i)
      {
        audioTypes[i] = new AGSAudioType();
        audioTypes[i].LoadFromStream(r);
      }

      // parse audio clips info
      Int32 audioclips_count = r.ReadInt32();
      audioClips = new AGSAudioClip[audioclips_count];
      AGSAlignedStream ar = new AGSAlignedStream(r);
      for (int i = 0; i < audioClips.Length; ++i)
      {
        audioClips[i] = new AGSAudioClip();
        audioClips[i].LoadFromStream(ar);
        ar.Reset();
      }

      score_clip_id = r.ReadInt32();
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write((Int32)audioTypes.Length);
      for (int i = 0; i < audioTypes.Length; ++i)
        audioTypes[i].WriteToStream(writer);

      AGSAlignedStream aw = new AGSAlignedStream(writer);
      writer.Write((Int32)audioClips.Length);
      for (int i = 0; i < audioClips.Length; ++i)
      {
        audioClips[i].WriteToStream(aw);
        aw.Reset();
      }

      writer.Write((Int32)score_clip_id);
    }
  }
}
