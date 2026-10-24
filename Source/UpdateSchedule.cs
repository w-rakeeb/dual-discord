using System;
using System.Collections.Generic;
namespace DualDiscord {
 public sealed class UpdateSchedule {
  DateTime nextCheck=DateTime.MinValue,nextReopen=DateTime.MinValue;readonly HashSet<string> announced=new HashSet<string>();
  public bool TakeCheck(bool enabled,DateTime now,bool reopened=false){if(!enabled||(now<nextCheck&&(!reopened||now<nextReopen)))return false;Succeeded(now);return true;}
  public void Succeeded(DateTime now){nextCheck=now.AddMinutes(15);nextReopen=now.AddMinutes(1);}
  public void Failed(DateTime now,bool rateLimited=false){nextCheck=now.AddMinutes(rateLimited?15:1);nextReopen=nextCheck;}
  public bool Announce(string version){return announced.Add(version);}
 }
}
