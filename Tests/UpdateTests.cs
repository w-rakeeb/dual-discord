using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using DualDiscord;
static class UpdateTests {
 static int failures,total;static string root;
 static void Check(string name,Action test){total++;try{test();Console.WriteLine("PASS "+name);}catch(Exception e){failures++;Console.WriteLine("FAIL "+name+": "+e.Message);}}
 static void Assert(bool value){if(!value)throw new Exception("Assertion failed");}
 static void Reject(Action work){try{work();}catch(Exception){return;}throw new Exception("Expected rejection");}
 static string Zip(string name,params string[] files){string file=Path.Combine(root,name+".zip");using(var zip=ZipFile.Open(file,ZipArchiveMode.Create))foreach(string path in files){var entry=zip.CreateEntry(path);using(var writer=new StreamWriter(entry.Open()))writer.Write("fixture");}return file;}
 public static int Run(){root=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"DualDiscordUpdateTests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
  Check("Release version comparison is numeric",()=>Assert(UpdateService.ParseVersion("v2.10.0")>UpdateService.ParseVersion("2.9.0")));
  Check("Prerelease version tag rejected",()=>Reject(()=>UpdateService.ParseVersion("v2.2.0-beta")));
  Check("Asset URL pinned to this repository",()=>{Assert(UpdateService.TrustedAsset("https://github.com/w-rakeeb/dual-discord/releases/download/v2.1.0/Dual-Discord-Windows.zip"));Assert(!UpdateService.TrustedAsset("https://github.com/other/dual-discord/releases/download/v2.1.0/a.zip"));Assert(!UpdateService.TrustedAsset("http://github.com/w-rakeeb/dual-discord/releases/download/v2/a.zip"));});
  Check("Corrupt checksum is rejected",()=>{string file=Zip("hash","Dual Discord.exe");Reject(()=>UpdatePackage.VerifyHash(file,new string('0',64)));UpdatePackage.VerifyHash(file,UpdatePackage.Hash(file));});
  Check("Traversal archive is rejected",()=>Reject(()=>UpdatePackage.Validate(Zip("traversal","Dual Discord.exe","../escape.exe"))));
  Check("User settings cannot appear in update",()=>Reject(()=>UpdatePackage.Validate(Zip("settings","Dual Discord.exe","Settings/app.json"))));
  Check("Unexpected executable is rejected",()=>Reject(()=>UpdatePackage.Validate(Zip("exe","Dual Discord.exe","other.exe"))));
  Check("Duplicate case-insensitive entries rejected",()=>Reject(()=>UpdatePackage.Validate(Zip("duplicate","Dual Discord.exe","dual discord.EXE"))));
  Check("Missing required app files rejected",()=>Reject(()=>UpdatePackage.Validate(Zip("missing","README.md"))));
  Check("Incomplete portable launch check creates no support folders",()=>{string home=Path.Combine(root,"LoneExe");Assert(!UpdatePackage.RuntimeFilesPresent(home)&&!Directory.Exists(home));});
  Check("Complete portable folder passes startup file check",()=>{string home=Path.Combine(root,"CompletePortable");UpdatePackage.Extract(Zip("runtime","Dual Discord.exe","Dual Discord.exe.config","Dual Discord Updater.exe","Assets/Dual Discord.ico","Assets/Dual Discord.png","Assets/Discord.png","Assets/Dual Discord.svg","Assets/Discord.svg"),home);Assert(UpdatePackage.RuntimeFilesPresent(home));});
  Check("Missing runtime vector assets rejected",()=>Reject(()=>UpdatePackage.Validate(Zip("missing-vector","Dual Discord.exe","Dual Discord.exe.config","Dual Discord Updater.exe","Assets/Dual Discord.ico","Assets/Dual Discord.png","Assets/Discord.png"))));
  Check("Valid stable release metadata is accepted",()=>{var r=UpdateService.ParseRelease(Metadata("sha256:"+new string('a',64),false));Assert(r.Version==new Version(2,2,0)&&r.Sha256.Length==64&&r.Size==123);});
  Check("Release without a digest cannot be installed",()=>Reject(()=>UpdateService.ParseRelease(Metadata("",false))));
  Check("Prerelease releases are excluded",()=>Reject(()=>UpdateService.ParseRelease(Metadata("sha256:"+new string('a',64),true))));
  Check("Rollback preserves app and personal files",()=>{
   string target=Path.Combine(root,"Target"),stage=Path.Combine(root,"Stage");Directory.CreateDirectory(target);Directory.CreateDirectory(stage);Directory.CreateDirectory(Path.Combine(target,"Settings"));Directory.CreateDirectory(Path.Combine(target,"Data"));File.WriteAllText(Path.Combine(target,"Settings","app.json"),"personal-settings");File.WriteAllText(Path.Combine(target,"Data","keep.txt"),"profile-data");File.WriteAllText(Path.Combine(target,"Dual Discord.exe"),"old-app");File.WriteAllText(Path.Combine(target,"README.md"),"old-readme");File.WriteAllText(Path.Combine(stage,"Dual Discord.exe"),"new-app");File.WriteAllText(Path.Combine(stage,"README.md"),"new-readme");int copied=0;
   Reject(()=>UpdatePackage.Apply(stage,target,delegate(string file){if(++copied==2)throw new IOException("injected failure");}));Assert(File.ReadAllText(Path.Combine(target,"Dual Discord.exe"))=="old-app"&&File.ReadAllText(Path.Combine(target,"README.md"))=="old-readme");Assert(File.ReadAllText(Path.Combine(target,"Settings","app.json"))=="personal-settings"&&File.ReadAllText(Path.Combine(target,"Data","keep.txt"))=="profile-data");
  });
  Check("Successful update leaves private and unrelated files alone",()=>{string target=Path.Combine(root,"Success"),stage=Path.Combine(root,"SuccessStage");Directory.CreateDirectory(target);Directory.CreateDirectory(stage);File.WriteAllText(Path.Combine(target,"personal.txt"),"keep");File.WriteAllText(Path.Combine(stage,"Dual Discord.exe"),"new");UpdatePackage.Apply(stage,target,null);Assert(File.ReadAllText(Path.Combine(target,"personal.txt"))=="keep"&&File.ReadAllText(Path.Combine(target,"Dual Discord.exe"))=="new");});
  Console.WriteLine("UPDATE RESULT "+(total-failures)+" passed, "+failures+" failed");return failures;
 }
 static string Metadata(string digest,bool prerelease){return "{\"tag_name\":\"v2.2.0\",\"draft\":false,\"prerelease\":"+(prerelease?"true":"false")+",\"assets\":[{\"name\":\"Dual-Discord-Windows.zip\",\"browser_download_url\":\"https://github.com/w-rakeeb/dual-discord/releases/download/v2.2.0/Dual-Discord-Windows.zip\",\"digest\":\""+digest+"\",\"size\":123}]}";}
}
