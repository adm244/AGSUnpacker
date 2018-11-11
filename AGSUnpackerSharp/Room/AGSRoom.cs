using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGSUnpackerSharp.Shared;

namespace AGSUnpackerSharp.Room
{
  public struct AGSRoomEdge
  {
    public Int16 top;
    public Int16 bottom;
    public Int16 left;
    public Int16 right;
  }

  public struct AGSRoom
  {
    public Int32 background_bpp;
    public AGSWalkBehindArea[] walkbehinds;
    public AGSHotspot[] hotspots;
    public AGSRoomEdge edge;
    public AGSObject[] objects;
    public AGSInteractionScript events;
    public AGSRegion[] regions;
    public Int16 width;
    public Int16 height;
    public Int16 resolution_type;
    public AGSWalkableArea[] walkareas;
    public byte[] password;
    public byte startup_music;
    public byte saveload_disabled;
    public byte player_invisible;
    public byte player_view;
    public byte music_volume;
    public AGSMessage[] messages;
    public Int32 game_id;
    public AGSScript script;
  }
}
