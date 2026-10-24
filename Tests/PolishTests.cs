using System;using System.IO;using DualDiscord;
static class PolishTests {
 static int failed,total;static void Check(string name,Action test){total++;try{test();Console.WriteLine("PASS "+name);}catch(Exception e){failed++;Console.WriteLine("FAIL "+name+": "+e.Message);}}static void Assert(bool value){if(!value)throw new Exception("Assertion failed");}
 public static int Run(){
  Check("Preference reset preserves chosen profile storage",()=>{var old=new Customization{SecondRoot="X:\\Example\\Profile",HideTaskbar=true,HideTray=true,KeepSecondRunning=false,CopyThemes=true,AutoCheckUpdates=false,CloseAction="Exit"};var value=Customization.DefaultsKeepingStorage(old);Assert(value.SecondRoot==old.SecondRoot&&!value.HideTaskbar&&!value.HideTray&&value.KeepSecondRunning==BuildProfile.Personal&&!value.CopyThemes&&value.AutoCheckUpdates&&value.CloseAction=="Tray");Assert(old.HideTaskbar&&old.CloseAction=="Exit");});
  Check("Dualcord gets a separate taskbar identity by default",()=>Assert(new Customization().SeparateTaskbar));
  Console.WriteLine("POLISH "+(total-failed)+" passed, "+failed+" failed");return failed;
 }
}
