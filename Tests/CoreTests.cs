using System;
using System.IO;
using System.Linq;
using DualDiscord;
class CoreTests {
 static int failed,total;static string root;
 static void Check(string name,Action action){total++;try{action();Console.WriteLine("PASS "+name);}catch(Exception e){failed++;Console.WriteLine("FAIL "+name+": "+e.Message);}}
 static void Assert(bool value){if(!value)throw new Exception("Assertion failed");}
 static void Reject(Action action){try{action();}catch(Exception){return;}throw new Exception("Expected rejection");}
 static int Main(){root=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"DualDiscordTests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
  Check("Latest complete version, not lexical order",()=>{var install=Path.Combine(root,"Discord");foreach(string version in new[]{"1.0.9","1.0.10"}){string folder=Path.Combine(install,"app-"+version);Directory.CreateDirectory(Path.Combine(folder,"resources"));File.WriteAllText(Path.Combine(folder,"Discord.exe"),"");File.WriteAllText(Path.Combine(folder,"resources","app.asar"),"ready");}Directory.CreateDirectory(Path.Combine(install,"app-1.0.11"));Assert(Installation.FindExecutable(install).Contains("app-1.0.10"));});
  Check("Newer exe with unfinished resources is skipped",()=>{File.WriteAllText(Path.Combine(root,"Discord","app-1.0.11","Discord.exe"),"");Assert(Installation.FindExecutable(Path.Combine(root,"Discord")).Contains("app-1.0.10"));});
  Check("Selecting version exe follows future installed versions",()=>Assert(Installation.FindExecutable(Path.Combine(root,"Discord","app-1.0.9","Discord.exe")).Contains("1.0.10")));
  Check("Missing installation returns no executable",()=>Assert(Installation.FindExecutable(Path.Combine(root,"Missing"))==null));
  Check("Auto discovery accepts a running custom installation",()=>Assert(Installation.Discover(Path.Combine(root,"Missing"),new[]{Path.Combine(root,"Discord","app-1.0.9","Discord.exe")}).Contains("1.0.10")));
  Check("Installation settings persist independently",()=>{string file=Path.Combine(root,"installation.json");var value=new InstallationSettings{InstallRoot=Path.Combine(root,"Discord")};Installation.Save(file,value);Assert(Installation.Load(file).InstallRoot==value.InstallRoot);});
  Check("First setup creates empty separate data folders",()=>{string second=Path.Combine(root,"Second profile"),vencord=Path.Combine(root,"Private Vencord");Installation.PrepareSecond(second,vencord);Assert(Directory.Exists(Path.Combine(second,"discord"))&&Directory.Exists(vencord)&&Directory.GetFiles(Path.Combine(second,"discord")).Length==0);File.WriteAllText(Path.Combine(second,"discord","keep.txt"),"existing-login-placeholder");Installation.PrepareSecond(second,vencord);Assert(File.ReadAllText(Path.Combine(second,"discord","keep.txt"))=="existing-login-placeholder");});
  Console.WriteLine("RESULT "+(total-failed)+" passed, "+failed+" failed");return failed+UpdateTests.Run()+FlexibilityTests.Run()+LifecycleTests.Run()==0?0:1;
 }
}
