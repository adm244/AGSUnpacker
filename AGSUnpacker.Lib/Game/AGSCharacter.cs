using System;

using AGSUnpacker.Lib.Shared;
using AGSUnpacker.Lib.Shared.Interaction;

namespace AGSUnpacker.Lib.Game
{
  public class AGSCharacter
  {
    public Int32 view_default;
    public Int32 view_talk;
    public Int32 view_normal;
    public Int32 room;
    public Int32 room_previous;
    public Int32 x;
    public Int32 y;
    public Int32 wait;
    public Int32 flags;
    public Int16 following;
    public Int16 followinfo;
    public Int32 view_idle;
    public Int16 idle_time;
    public Int16 idle_left;
    public Int16 transparency;
    public Int16 baseline;
    public Int32 active_invitem;
    public Int32 talk_color;
    public Int32 view_think;
    public Int16 view_blink;
    public Int16 blink_interval;
    public Int16 blink_timer;
    public Int16 blink_frame;
    public Int16 walkspeed_y;
    public Int16 picture_offset_y;
    public Int32 z;
    public Int32 wait_walk;
    public Int16 speech_animation_speed;
    public Int16 reserved1;
    public Int16 blocking_width;
    public Int16 blocking_height;
    public Int32 index_id;
    public Int16 picture_offset_x;
    public Int16 walk_wait_counter;
    public Int16 loop;
    public Int16 frame;
    public Int16 walking;
    public Int16 animating;
    public Int16 walkspeed;
    public Int16 animspeed;
    public Int16[] inventory;
    public Int16 act_x;
    public Int16 act_y;
    public string name;
    public string name_script;
    public Int16 on;

    public AGSInteraction interactions_old;
    public AGSInteractionScript interactions;
    public AGSPropertyStorage properties;

    public AGSCharacter()
    {
      view_default = 0;
      view_talk = 0;
      view_normal = 0;
      room = 0;
      room_previous = 0;
      x = 0;
      y = 0;
      wait = 0;
      flags = 0;
      following = 0;
      followinfo = 0;
      view_idle = 0;
      idle_time = 0;
      idle_left = 0;
      transparency = 0;
      baseline = 0;
      active_invitem = 0;
      talk_color = 0;
      view_think = 0;
      view_blink = 0;
      blink_interval = 0;
      blink_timer = 0;
      blink_frame = 0;
      walkspeed_y = 0;
      picture_offset_y = 0;
      z = 0;
      wait_walk = 0;
      speech_animation_speed = 0;
      reserved1 = 0;
      blocking_width = 0;
      blocking_height = 0;
      index_id = 0;
      picture_offset_x = 0;
      walk_wait_counter = 0;
      loop = 0;
      frame = 0;
      walking = 0;
      animating = 0;
      walkspeed = 0;
      animspeed = 0;
      inventory = new Int16[0];
      act_x = 0;
      act_y = 0;
      name = string.Empty;
      name_script = string.Empty;
      on = 0;

      interactions_old = new AGSInteraction();
      interactions = new AGSInteractionScript();
      properties = new AGSPropertyStorage();
    }

    public void LoadFromStream(AGSAlignedStream ar)
    {
      view_default = ar.ReadInt32();
      view_talk = ar.ReadInt32();
      view_normal = ar.ReadInt32();
      room = ar.ReadInt32();
      room_previous = ar.ReadInt32();
      x = ar.ReadInt32();
      y = ar.ReadInt32();
      wait = ar.ReadInt32();
      flags = ar.ReadInt32();
      following = ar.ReadInt16();
      followinfo = ar.ReadInt16();
      // FIXME(adm244): is this int32 or  int16?
      view_idle = ar.ReadInt16();
      idle_time = ar.ReadInt16();
      idle_left = ar.ReadInt16();
      transparency = ar.ReadInt16();
      baseline = ar.ReadInt16();
      active_invitem = ar.ReadInt32();
      talk_color = ar.ReadInt32();
      view_think = ar.ReadInt32();
      view_blink = ar.ReadInt16();
      blink_interval = ar.ReadInt16();
      blink_timer = ar.ReadInt16();
      blink_frame = ar.ReadInt16();
      walkspeed_y = ar.ReadInt16();
      picture_offset_y = ar.ReadInt16();
      z = ar.ReadInt32();
      wait_walk = ar.ReadInt32();
      speech_animation_speed = ar.ReadInt16();
      reserved1 = ar.ReadInt16();
      blocking_width = ar.ReadInt16();
      blocking_height = ar.ReadInt16();
      index_id = ar.ReadInt32();
      picture_offset_x = ar.ReadInt16();
      walk_wait_counter = ar.ReadInt16();
      loop = ar.ReadInt16();
      frame = ar.ReadInt16();
      walking = ar.ReadInt16();
      animating = ar.ReadInt16();
      walkspeed = ar.ReadInt16();
      animspeed = ar.ReadInt16();
      inventory = ar.ReadArrayInt16(301);
      act_x = ar.ReadInt16();
      act_y = ar.ReadInt16();

      name = ar.ReadFixedString(40);
      name_script = ar.ReadFixedString(20);

      // FIXME(adm244): what's going on with this? investigate...
      //NOTE(adm244): in source it's a byte, but in the actual dta it's int16
      on = ar.ReadInt16();
    }

    public void WriteToStream(AGSAlignedStream aw)
    {
      aw.WriteInt32(view_default);
      aw.WriteInt32(view_talk);
      aw.WriteInt32(view_normal);
      aw.WriteInt32(room);
      aw.WriteInt32(room_previous);
      aw.WriteInt32(x);
      aw.WriteInt32(y);
      aw.WriteInt32(wait);
      aw.WriteInt32(flags);
      aw.WriteInt16(following);
      aw.WriteInt16(followinfo);
      aw.WriteInt16((Int16)view_idle);
      aw.WriteInt16(idle_time);
      aw.WriteInt16(idle_left);
      aw.WriteInt16(transparency);
      aw.WriteInt16(baseline);
      aw.WriteInt32(active_invitem);
      aw.WriteInt32(talk_color);
      aw.WriteInt32(view_think);
      aw.WriteInt16(view_blink);
      aw.WriteInt16(blink_interval);
      aw.WriteInt16(blink_timer);
      aw.WriteInt16(blink_frame);
      aw.WriteInt16(walkspeed_y);
      aw.WriteInt16(picture_offset_y);
      aw.WriteInt32(z);
      aw.WriteInt32(wait_walk);
      aw.WriteInt16(speech_animation_speed);
      aw.WriteInt16(reserved1);
      aw.WriteInt16(blocking_width);
      aw.WriteInt16(blocking_height);
      aw.WriteInt32(index_id);
      aw.WriteInt16(picture_offset_x);
      aw.WriteInt16(walk_wait_counter);
      aw.WriteInt16(loop);
      aw.WriteInt16(frame);
      aw.WriteInt16(walking);
      aw.WriteInt16(animating);
      aw.WriteInt16(walkspeed);
      aw.WriteInt16(animspeed);
      aw.WriteArrayInt16(inventory);
      aw.WriteInt16(act_x);
      aw.WriteInt16(act_y);

      aw.WriteFixedString(name, 40);
      aw.WriteFixedString(name_script, 20);

      // FIXME(adm244): possible incorrect size, check load func
      aw.WriteInt16(on);
    }
  }
}
