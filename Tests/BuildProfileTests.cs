using System;
using System.Web.Script.Serialization;
using DualDiscord;
class BuildProfileTests {
 static void Assert(bool value,string name){if(!value)throw new Exception(name);Console.WriteLine("PASS "+name);}
 static int Main(){try{
#if PERSONAL
  bool personal=true;
#else
  bool personal=false;
#endif
  Assert(BuildProfile.Personal==personal,"Compiled flavor matches expected edition");
  var settings=new Customization();Assert(settings.KeepSecondRunning==personal,"Fresh lifetime matches edition");
  settings.KeepSecondRunning=true;Assert(settings.KeepSecondRunning==personal,"Public setter cannot enable standalone");
  var old=new JavaScriptSerializer().Deserialize<Customization>("{\"KeepSecondRunning\":true}");Assert(old.KeepSecondRunning==personal,"Old standalone JSON respects edition");
  old.SecondRoot="retained";var reset=Customization.DefaultsKeepingStorage(old);Assert(reset.KeepSecondRunning==personal&&reset.SecondRoot=="retained","Reset retains storage and edition lifetime");
  Assert(settings.SeparateTray,"Dedicated Dualcord tray enabled by default");
  Assert(BuildProfile.CanInstallPublicUpdate==!personal,"Personal build cannot be replaced by public updater");
  return 0;
 }catch(Exception e){Console.WriteLine("FAIL "+e.Message);return 1;}}
}
