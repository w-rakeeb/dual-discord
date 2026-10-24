using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Web.Script.Serialization;

namespace DualDiscord {
public static class UiQa {
 static readonly List<string> layoutIssues=new List<string>();
 public static void Run(string home,string output){
  Directory.CreateDirectory(output);var app=new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};Theme.Apply("Dark");Theme.InitStyles();
  File.WriteAllText(Path.Combine(output,"contrast.txt"),Theme.Audit());var results=new List<object>();
  foreach(string mode in new[]{"Dark","Light","HighContrast"}){
   Theme.Apply(mode=="Light"?"Light":"Dark",mode=="HighContrast");
   foreach(double scale in new[]{1.0,1.25,1.5,2.0})foreach(bool wide in new[]{false,true}){
    var shell=Fixture(home);double width=wide?1280:720,height=wide?800:520;shell.Width=width;shell.Height=height;shell.LayoutForWidth(width);
    foreach(string page in new[]{"Profiles","Settings","Files","About"}){shell.Navigate(page,false);string name=mode+"-"+(int)(scale*100)+"-"+(wide?"wide":"min")+"-"+page;Render(shell,width,height,scale,Path.Combine(output,name+".png"));results.Add(new{File=name+".png",Theme=mode,Scale=scale,Width=width,Height=height,Page=page,Kind="Native WPF offscreen render"});}
    shell.Close();
   }
  }
  Theme.Apply("Dark",false);
  foreach(string state in new[]{"empty","error","disabled","loading","hover","pressed","keyboard-focus"}){
   var shell=Fixture(home);shell.LayoutForWidth(900);
   if(state=="empty")shell.SecondProfile.Update(new Instance{Second=true},false);
   if(state=="error")shell.Missing(true);
   if(state=="disabled"){shell.Navigate("Settings",false);shell.SetOptions(new Options(),true);}
   if(state=="loading")shell.SecondProfile.Loading(true);
   string stateFile=Path.Combine(output,"state-"+state+".png");Render(shell,900,620,1,stateFile);
   if(state=="hover"||state=="pressed"){shell.SecondProfile.Open.ApplyTemplate();var plate=(Border)shell.SecondProfile.Open.Template.FindName("Plate",shell.SecondProfile.Open);Theme.Bind(plate,Border.BackgroundProperty,state=="hover"?"accent.hover":"accent.pressed");Render(shell,900,620,1,stateFile);}
   if(state=="keyboard-focus"){var button=shell.SecondProfile.Open;var bounds=button.TransformToAncestor(shell.Root).TransformBounds(new Rect(button.RenderSize));var ring=new Border{Width=bounds.Width+8,Height=bounds.Height+8,Margin=new Thickness(bounds.Left-4,bounds.Top-4,0,0),HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Top,BorderThickness=new Thickness(2),CornerRadius=new CornerRadius(8),IsHitTestVisible=false};Theme.Bind(ring,Border.BorderBrushProperty,"focus.ring");Grid.SetRowSpan(ring,2);shell.Root.Children.Add(ring);Render(shell,900,620,1,stateFile);}
   shell.Close();
  }
  Before(home,output);
  File.WriteAllText(Path.Combine(output,"matrix.json"),new JavaScriptSerializer().Serialize(results));
  File.WriteAllText(Path.Combine(output,"layout-checks.txt"),layoutIssues.Count==0?"PASS: visible, arranged after-state controls meet the 32-DIP hit-target minimum; after-state text is at least 12 DIP. Scrollable content is intentionally allowed beyond the viewport.\r\n":String.Join(Environment.NewLine,layoutIssues));
  File.WriteAllText(Path.Combine(output,"QA-method.txt"),"96 native WPF renders: dark/light/high-contrast token mapping × 100/125/150/200% pixel density × 720×520 and 1280×800 × four pages. Windows global DPI, OS theme and OS high-contrast settings were not changed. These are application-owned render fixtures, not claims of physical monitor tests. State fixtures are simulated and contain no account data. Hover/focus fixture limitations are documented in the review report. Engine.cs and Settings/app.json must retain their baseline hashes.\r\n");
  app.Shutdown();
 }
 public static AppShell Fixture(string home){
  var shell=new AppShell(home);var main=new Instance{Count=6,Memory=440*1048576L};main.Roots.Add(new Proc{Id=12000});var second=new Instance{Second=true};shell.SetStats(main,second,true);shell.SetOptions(new Options(),false);
  shell.SetFiles(new[]{new KeyValuePair<string,string>("Main profile data",Path.Combine(home,"Profiles","Main")),new KeyValuePair<string,string>("Second profile data",Path.Combine(home,"Profiles","Second","Second Account","discord")),new KeyValuePair<string,string>("Main Vencord data",Path.Combine(home,"Vencord","Main")),new KeyValuePair<string,string>("Installed Discord",Path.Combine(home,"Runtime","Discord")),new KeyValuePair<string,string>("Activity logs",Path.Combine(home,"Logs")),new KeyValuePair<string,string>("App and source files",home),new KeyValuePair<string,string>("Second Vencord data",Path.Combine(home,"Data","Vencord"))});return shell;
 }
 static void Render(Window shell,double width,double height,double scale,string file){
  var root=(FrameworkElement)shell.Content;var border=root as Border;if(border!=null&&border.Background==null)border.Background=shell.Background;root.Width=width;root.Height=height;root.Measure(new Size(width,height));root.Arrange(new Rect(0,0,width,height));root.UpdateLayout();
  var target=new RenderTargetBitmap((int)Math.Ceiling(width*scale),(int)Math.Ceiling(height*scale),96*scale,96*scale,PixelFormats.Pbgra32);target.Render(root);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(target));using(var stream=File.Create(file))encoder.Save(stream);
  if(shell is AppShell)Probe(root,Path.GetFileName(file));
 }
 static void Probe(DependencyObject node,string file){var element=node as FrameworkElement;if(element!=null&&element.Visibility==Visibility.Visible&&element.Opacity>0&&element.ActualWidth>0&&element.ActualHeight>0){if(element is System.Windows.Controls.Primitives.ButtonBase&&(element.ActualHeight<31.9||element.ActualWidth<31.9))layoutIssues.Add(file+": small hit target "+element.GetType().Name+" "+element.ActualWidth+"×"+element.ActualHeight);var text=element as TextBlock;if(text!=null&&text.FontSize<12)layoutIssues.Add(file+": text below 12 DIP");}for(int i=0;i<VisualTreeHelper.GetChildrenCount(node);i++)Probe(VisualTreeHelper.GetChild(node,i),file);}
 static void Before(string home,string output){
  string backup=Path.Combine(home,"Backups","Before design blueprint","Assets");
  foreach(string page in new[]{"Profiles","Settings","Files","About"}){
   string file=Path.Combine(backup,page=="Profiles"?"Main.xaml":"Details.xaml");if(!File.Exists(file))continue;
   try{Window window;using(var stream=File.OpenRead(file))window=(Window)XamlReader.Load(stream);if(page!="Profiles")foreach(string name in new[]{"Instances","Settings","Files","About"}){var node=window.FindName(name+"Page") as FrameworkElement;if(node!=null)node.Visibility=name==page?Visibility.Visible:Visibility.Collapsed;}
    var rows=window.FindName("FileRows") as StackPanel;if(rows!=null){rows.Children.Add(new TextBlock{Text="Main Discord data\n"+Engine.MainData+"\n\nSecond Discord data\n"+Engine.SecondData,FontSize=12,Foreground=Brushes.White,TextWrapping=TextWrapping.Wrap});}
    Render(window,page=="Profiles"?448:690,page=="Profiles"?240:550,1,Path.Combine(output,"before-"+page+".png"));window.Close();
   }catch(Exception ex){File.AppendAllText(Path.Combine(output,"before-render-notes.txt"),page+": "+ex.Message+Environment.NewLine);}
  }
 }
}
}
