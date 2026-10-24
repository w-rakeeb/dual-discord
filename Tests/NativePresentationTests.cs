using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Forms=System.Windows.Forms;
using DualDiscord;
class NativePresentationTests {
 static int count;
 static void Assert(bool value,string name){if(!value)throw new Exception(name);count++;Console.WriteLine("PASS "+name);}
 [STAThread]static int Main(string[] args){try{
  string home=Path.GetFullPath(args[0]);Branding.Ensure(home);
  var main=(DrawingImage)LogoAssets.LoadProfile(Path.Combine(home,"Assets","Discord.png"));var second=(DrawingImage)LogoAssets.LoadProfile(Path.Combine(home,"Assets","Dualcord.png"));
  Assert(main.Drawing.Bounds==second.Drawing.Bounds,"Profile logos have identical visible canvas bounds");
  var a=(DrawingGroup)main.Drawing;var b=(DrawingGroup)second.Drawing;
  Assert(a.Children[1].Bounds==b.Children[1].Bounds,"Discord symbols have identical size and placement");
  var settings=new Customization();var window=new Window();using(var control=new WindowLifecycle(window,home,()=>settings)){
   var field=typeof(WindowLifecycle).GetField("secondTray",BindingFlags.NonPublic|BindingFlags.Instance);var tray=(Forms.NotifyIcon)field.GetValue(control);
   Assert(!tray.Visible,"Dualcord tray absent while profile closed");control.UpdateSecond(true);Assert(tray.Visible&&tray.Text=="Dualcord","Running profile has named dedicated tray icon");
   int open=0,quit=0;control.SecondOpenRequested=()=>open++;control.SecondQuitRequested=()=>quit++;
   tray.ContextMenuStrip.Items[0].PerformClick();tray.ContextMenuStrip.Items[1].PerformClick();Assert(open==1&&quit==1,"Dualcord tray controls dispatch scoped profile actions");
   settings.SeparateTray=false;control.Apply();Assert(!tray.Visible,"Separate tray preference hides dedicated icon");settings.SeparateTray=true;settings.HideTray=true;control.Apply();Assert(!tray.Visible,"Hide tray preference hides both manager controls");settings.HideTray=false;control.Apply();Assert(tray.Visible,"Tray can be restored without relaunching profile");control.UpdateSecond(false);Assert(!tray.Visible,"Profile exit removes dedicated tray icon");
  }
  Console.WriteLine(count+" native presentation checks passed");return 0;
 }catch(Exception e){Console.WriteLine("FAIL "+e);return 1;}}
}
