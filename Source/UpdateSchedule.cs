using System;
using System.Collections.Generic;
namespace DualDiscord {
 public sealed class UpdateSchedule {
  DateTime nextCheck=DateTime.MinValue;readonly HashSet<string> announced=new HashSet<string>();
  public bool TakeCheck(bool enabled,DateTime now){if(!enabled||now<nextCheck)return false;nextCheck=now.AddHours(6);return true;}
  public bool Announce(string version){return announced.Add(version);}
 }
}
