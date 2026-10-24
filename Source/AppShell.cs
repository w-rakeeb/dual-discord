using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shell;

namespace DualDiscord {
public class AppShell:Window {
 public Action<bool> OpenProfile,QuitProfile;public Action<string,bool> OptionChanged;public Action<string> AppearanceChanged,OpenFolder,CopyPath,ExternalLink;public Action Locate,Detect;
 public readonly ProfileCard MainProfile=new ProfileCard("Main Discord","Default profile","\uE770"),SecondProfile=new ProfileCard("Second Discord","Separate login and data","\uE8A7");
 public readonly Dictionary<string,SettingRow> SettingsRows=new Dictionary<string,SettingRow>();
 public readonly Dictionary<string,FrameworkElement> Pages=new Dictionary<string,FrameworkElement>();
 public readonly Grid Root=new Grid();public readonly Toast Toast=new Toast();
 readonly Grid body=new Grid();readonly ColumnDefinition railColumn=new ColumnDefinition{Width=new GridLength(Theme.Rail)};
 readonly Dictionary<string,NavItem> nav=new Dictionary<string,NavItem>();readonly ContentControl content=new ContentControl();
 readonly TextBlock version=Ui.Text("v2.0",Theme.NoteSize,"text.muted");
 readonly ProcessOverview processDetails=new ProcessOverview();
 public readonly UpdateView Updates=new UpdateView();readonly TextBlock installationStatus=Ui.Text("Detecting Discord…",Theme.BodySize,"text.secondary");
 readonly FileLocations fileRows=new FileLocations();readonly Border errorHost=new Border();readonly InfoBanner missing=new InfoBanner("Discord wasn't found. Locate the installed app or repair the installation.",true);
 readonly string home;public string Page="Profiles";public bool LoadingOptions;
 public AppShell(string directory){
  home=directory;Title="Dual Discord";Width=900;Height=620;MinWidth=720;MinHeight=520;FontFamily=(FontFamily)Theme.Resources["Font.App"];FontSize=Theme.BodySize;WindowStartupLocation=WindowStartupLocation.CenterScreen;
  WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.CanResize;UseLayoutRounding=true;SnapsToDevicePixels=true;Theme.Bind(this,BackgroundProperty,"bg.window");Theme.Bind(this,ForegroundProperty,"text.primary");
  WindowChrome.SetWindowChrome(this,new WindowChrome{CaptionHeight=Theme.CaptionHeight,ResizeBorderThickness=new Thickness(6),CornerRadius=new CornerRadius(12),GlassFrameThickness=new Thickness(0)});
  try{Icon=BitmapFrame.Create(new Uri(Path.Combine(home,"Assets","Dual Discord.ico")));}catch(IOException){}
  Root.RowDefinitions.Add(new RowDefinition{Height=new GridLength(Theme.CaptionHeight)});Root.RowDefinitions.Add(new RowDefinition());
  var frame=new Border{BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(12),Child=Root};Theme.Bind(frame,Border.BorderBrushProperty,"border.subtle");Theme.Bind(frame,Border.BackgroundProperty,"bg.window");Content=frame;
  MainProfile.SetLogo(Path.Combine(home,"Assets","Discord.png"));SecondProfile.SetLogo(Path.Combine(home,"Assets","Dual Discord.png"));
  BuildTitleBar();body.ColumnDefinitions.Add(railColumn);body.ColumnDefinitions.Add(new ColumnDefinition());Grid.SetRow(body,1);Root.Children.Add(body);BuildRail();
  content.Margin=new Thickness(Theme.Padding,Theme.Padding,Theme.Space,Theme.Padding);content.HorizontalAlignment=HorizontalAlignment.Left;Grid.SetColumn(content,1);body.Children.Add(content);
  Pages["Profiles"]=Profiles();Pages["Settings"]=Settings();Pages["Files"]=Files();Pages["About"]=About();
  Grid.SetRowSpan(Toast,2);Root.Children.Add(Toast);Panel.SetZIndex(Toast,10);
  SizeChanged+=delegate{LayoutForWidth(ActualWidth);};
  PreviewKeyDown+=delegate(object sender,KeyEventArgs e){if(e.Key==Key.Escape){Toast.Dismiss();e.Handled=true;}};
  Navigate("Profiles",false);
 }
 public void LayoutForWidth(double width){bool narrow=width<Theme.NarrowAt;railColumn.Width=new GridLength(narrow?Theme.NarrowRail:Theme.Rail);content.Width=Math.Max(0,Math.Min(Theme.ContentMax+Theme.ScrollInset,width-(narrow?Theme.NarrowRail:Theme.Rail)-Theme.Padding-Theme.Space-2));foreach(var item in nav.Values)item.Compact(narrow);version.Visibility=narrow?Visibility.Collapsed:Visibility.Visible;}
 Button CaptionButton(string drawing,string label,Action action){
  var button=Ui.Button("",false,action);button.Style=Theme.Style("Quiet");button.Width=Theme.CaptionButtonWidth;button.Height=Theme.Hit;button.MinWidth=Theme.CaptionButtonWidth;button.MinHeight=Theme.Hit;button.Padding=new Thickness(0);button.TabIndex=90;button.ToolTip=label;Ui.Name(button,label);
  var canvas=new Canvas{Width=Theme.CaptionIconSize,Height=Theme.CaptionIconSize};var path=new System.Windows.Shapes.Path{Data=Geometry.Parse(drawing),StrokeThickness=1,SnapsToDevicePixels=true};Theme.Bind(path,System.Windows.Shapes.Shape.StrokeProperty,"text.secondary");canvas.Children.Add(path);button.Content=canvas;WindowChrome.SetIsHitTestVisibleInChrome(button,true);return button;
 }
 void BuildTitleBar(){
  var bar=new Grid();bar.ColumnDefinitions.Add(new ColumnDefinition());bar.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
  var brand=new StackPanel{Orientation=Orientation.Horizontal,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(Theme.CardPadding,0,0,0)};
  try{brand.Children.Add(new Image{Source=new BitmapImage(new Uri(Path.Combine(home,"Assets","Dual Discord.png"))),Width=Theme.BrandIconSize,Height=Theme.BrandIconSize,Margin=new Thickness(0,0,Theme.MarkerInset,0)});}catch(IOException){}
  brand.Children.Add(Ui.Text("Dual Discord",Theme.NoteSize,"text.secondary",true));bar.Children.Add(brand);
  var actions=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,Theme.Space,0)};
  var minimize=CaptionButton("M1,6 L11,6","Minimize",delegate{WindowState=WindowState.Minimized;});
  var maximize=CaptionButton("M1.5,1.5 H10.5 V10.5 H1.5 Z","Maximize",delegate{WindowState=WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized;});
  var close=CaptionButton("M2,2 L10,10 M10,2 L2,10","Close Dual Discord",Close);
  StateChanged+=delegate{bool restored=WindowState==WindowState.Maximized;var path=(System.Windows.Shapes.Path)((Canvas)maximize.Content).Children[0];path.Data=Geometry.Parse(restored?"M3,3 V1 H11 V9 H9 M1,3 H9 V11 H1 Z":"M1.5,1.5 H10.5 V10.5 H1.5 Z");maximize.ToolTip=restored?"Restore":"Maximize";Ui.Name(maximize,restored?"Restore":"Maximize");};
  foreach(var button in new[]{minimize,maximize,close})actions.Children.Add(button);Grid.SetColumn(actions,1);bar.Children.Add(actions);Root.Children.Add(bar);
 }
 void BuildRail(){
  var rail=new Grid();rail.RowDefinitions.Add(new RowDefinition());rail.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});var items=new StackPanel{Margin=new Thickness(0,Theme.Gap,0,0)};
  string[] names={"Profiles","Settings","Files","About"},icons={"\uE8A7","\uE713","\uE8B7","\uE946"};
  for(int i=0;i<names.Length;i++){string page=names[i];var item=new NavItem(page,icons[i]);item.TabIndex=i;item.Click+=delegate{Navigate(page);};item.PreviewKeyDown+=delegate(object sender,KeyEventArgs e){int offset=e.Key==Key.Down||e.Key==Key.Right?1:e.Key==Key.Up||e.Key==Key.Left?-1:0;if(offset==0)return;int next=(Array.IndexOf(names,page)+offset+names.Length)%names.Length;nav[names[next]].Focus();Navigate(names[next]);e.Handled=true;};nav[page]=item;if(page!="About")items.Children.Add(item);}
  rail.Children.Add(items);var footer=new Grid{Margin=new Thickness(0,0,0,Theme.Gap)};footer.Children.Add(nav["About"]);version.Margin=new Thickness(0,0,Theme.CardPadding,0);version.HorizontalAlignment=HorizontalAlignment.Right;version.VerticalAlignment=VerticalAlignment.Center;version.IsHitTestVisible=false;footer.Children.Add(version);Grid.SetRow(footer,1);rail.Children.Add(footer);var sidebar=new Border{BorderThickness=new Thickness(0,0,1,0),CornerRadius=new CornerRadius(0,0,0,12),Child=rail};Theme.Bind(sidebar,Border.BackgroundProperty,"bg.sidebar");Theme.Bind(sidebar,Border.BorderBrushProperty,"border.sidebar");body.Children.Add(sidebar);
 }
 public void Navigate(string page,bool motion=true){if(!Pages.ContainsKey(page))page="Profiles";bool changed=content.Content!=Pages[page];Page=page;content.Content=Pages[page];foreach(var item in nav)item.Value.Active(item.Key==page);if(motion&&changed&&Theme.Motion){content.BeginAnimation(OpacityProperty,new DoubleAnimation(0,1,TimeSpan.FromMilliseconds(160)));var slide=new TranslateTransform();content.RenderTransform=slide;slide.BeginAnimation(TranslateTransform.YProperty,new DoubleAnimation(8,0,TimeSpan.FromMilliseconds(160)){EasingFunction=new QuadraticEase{EasingMode=EasingMode.EaseOut}});}}
 StackPanel PageHeading(string title,string subtitle){var panel=new StackPanel();var heading=Ui.Text(title,Theme.PageSize,"text.primary",true);panel.Children.Add(heading);if(subtitle!=null){var sub=Ui.Text(subtitle,Theme.BodySize,"text.secondary");sub.Margin=new Thickness(0,4,0,Theme.Padding);panel.Children.Add(sub);}else heading.Margin=new Thickness(0,0,0,Theme.Padding);return panel;}
 ScrollViewer Scroll(StackPanel panel){return new ScrollViewer{Style=Theme.Style("ThinScrollViewer"),Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled,Padding=new Thickness(0,0,Theme.ScrollInset+Theme.Space,Theme.Space)};}
 CreatorCredit Credit(){return new CreatorCredit(delegate(string url){if(ExternalLink!=null)ExternalLink(url);});}
 FrameworkElement Profiles(){
  var panel=PageHeading("Profiles","Run two Discord accounts side by side.");
  var missingPanel=new StackPanel();missingPanel.Children.Add(missing);var locate=Ui.Button("Locate Discord…",false,delegate{if(Locate!=null)Locate();});locate.HorizontalAlignment=HorizontalAlignment.Left;locate.Margin=new Thickness(0,8,0,0);missingPanel.Children.Add(locate);errorHost.Child=missingPanel;errorHost.Margin=new Thickness(0,0,0,Theme.Gap);errorHost.Visibility=Visibility.Collapsed;panel.Children.Add(errorHost);
  MainProfile.Margin=new Thickness(0,0,0,Theme.Gap);panel.Children.Add(MainProfile);panel.Children.Add(SecondProfile);
  MainProfile.Open.Click+=delegate{if(OpenProfile!=null)OpenProfile(false);};SecondProfile.Open.Click+=delegate{if(OpenProfile!=null)OpenProfile(true);};MainProfile.Quit.Click+=delegate{if(QuitProfile!=null)QuitProfile(false);};SecondProfile.Quit.Click+=delegate{if(QuitProfile!=null)QuitProfile(true);};
  var advanced=new Expander{Header="Process details",IsExpanded=true,Margin=new Thickness(0,Theme.Padding,0,0),FontSize=Theme.BodySize};Theme.Bind(advanced,ForegroundProperty,"text.secondary");advanced.Content=processDetails;panel.Children.Add(advanced);
  var layout=new Grid();layout.RowDefinitions.Add(new RowDefinition());layout.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});layout.Children.Add(Scroll(panel));var credit=Credit();credit.Margin=new Thickness(0,Theme.Gap,Theme.ScrollInset+Theme.Space,0);Grid.SetRow(credit,1);layout.Children.Add(credit);return layout;
 }
 TextBlock Section(string title){var text=Ui.Text(title,Theme.NoteSize,"text.muted",true);text.Margin=new Thickness(0,Theme.Padding,0,Theme.Gap);return text;}
 Card Group(params SettingRow[] rows){var card=new Card{Padding=new Thickness(0)};var p=new StackPanel();for(int i=0;i<rows.Length;i++){if(i>0)p.Children.Add(Ui.Divider());p.Children.Add(rows[i]);}card.Child=p;return card;}
 SettingRow Row(string key,string title,string description){var row=new SettingRow(title,description);SettingsRows[key]=row;RoutedEventHandler changed=delegate{if(!LoadingOptions&&OptionChanged!=null)OptionChanged(key,row.Toggle.IsChecked==true);};row.Toggle.Checked+=changed;row.Toggle.Unchecked+=changed;return row;}
 FrameworkElement Settings(){
  var panel=PageHeading("Settings","Changes save automatically.");var startup=Section("STARTUP");startup.Margin=new Thickness(0,0,0,Theme.Gap);panel.Children.Add(startup);
  panel.Children.Add(Group(Row("StartApp","Start Dual Discord when I sign in to Windows","Opens this manager after you sign in."),Row("StartSecond","Start the second profile when I sign in","Runs the second Discord minimized until you quit it.")));
  panel.Children.Add(Section("SHARING"));var note=new InfoBanner("Quit the second profile to change sharing. Logins stay separate.");note.Margin=new Thickness(0,0,0,Theme.Gap);panel.Children.Add(note);
  panel.Children.Add(Group(Row("SyncDiscord","Sync local settings from the main profile","Copies audio and graphics settings when the second starts."),Row("SyncVencord","Share Vencord settings and themes","Both profiles use the main Vencord settings folder.")));
  panel.Children.Add(Section("APPEARANCE"));var appearance=new Card();var wrap=new StackPanel();wrap.Children.Add(Ui.Text("App theme",Theme.TitleSize,"text.primary",true));var radios=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(0,8,0,0)};
  foreach(string mode in new[]{"System","Dark","Light"}){string selected=mode;var radio=new RadioButton{Content=mode,GroupName="Appearance",IsChecked=Theme.Mode==mode,FontSize=Theme.BodySize,Margin=new Thickness(0,0,Theme.Padding,0),MinHeight=Theme.Hit,VerticalContentAlignment=VerticalAlignment.Center,FocusVisualStyle=Theme.Style("Focus")};Theme.Bind(radio,Control.ForegroundProperty,"text.primary");Ui.Name(radio,mode+" theme");radio.Checked+=delegate{if(AppearanceChanged!=null)AppearanceChanged(selected);};radios.Children.Add(radio);}wrap.Children.Add(radios);appearance.Child=wrap; panel.Children.Add(appearance);
 panel.Children.Add(Section("DISCORD INSTALLATION"));var connection=new Card();var setup=new StackPanel();setup.Children.Add(installationStatus);var setupActions=new WrapPanel{Margin=new Thickness(0,Theme.Gap,0,0)};setupActions.Children.Add(Ui.Button("Detect Discord",false,delegate{if(Detect!=null)Detect();}));var locate=Ui.Button("Locate Discord…",false,delegate{if(Locate!=null)Locate();});locate.Margin=new Thickness(Theme.MarkerInset,0,0,0);setupActions.Children.Add(locate);setup.Children.Add(setupActions);var setupHint=Ui.Text("The second profile is prepared automatically with its own sign-in and local files. Your main login is never copied.",Theme.NoteSize,"text.muted");setupHint.Margin=new Thickness(0,Theme.Gap,0,0);setup.Children.Add(setupHint);connection.Child=setup;panel.Children.Add(connection);return Scroll(panel);
 }
 FrameworkElement Files(){var p=PageHeading("Files","Your profiles and app folders.");p.Children.Add(fileRows);return Scroll(p);}
 public void SetFiles(IEnumerable<KeyValuePair<string,string>> entries){fileRows.OpenFolder=delegate(string path){if(OpenFolder!=null)OpenFolder(path);};fileRows.CopyPath=delegate(string path){if(CopyPath!=null)CopyPath(path);};fileRows.Update(entries);}
 Card GuideCard(params string[][] rows){
  var card=new Card{Padding=new Thickness(0)};var stack=new StackPanel();
  for(int i=0;i<rows.Length;i++){if(i>0)stack.Children.Add(Ui.Divider());var row=new StackPanel{Margin=new Thickness(Theme.CardPadding)};row.Children.Add(Ui.Text(rows[i][0],Theme.BodySize,"text.primary",true));var detail=Ui.Text(rows[i][1],Theme.BodySize,"text.secondary");detail.Margin=new Thickness(0,Theme.Space,0,0);row.Children.Add(detail);stack.Children.Add(row);}card.Child=stack;return card;
 }
 FrameworkElement About(){
  var panel=PageHeading("About","A quick guide to your two Discord profiles.");
  var hero=new Card();var heroContent=new StackPanel();var heading=new Grid();heading.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});heading.ColumnDefinitions.Add(new ColumnDefinition());
  try{var icon=new Image{Source=new BitmapImage(new Uri(Path.Combine(home,"Assets","Dual Discord.png"))),Width=48,Height=48,Margin=new Thickness(0,0,Theme.CardPadding,0)};heading.Children.Add(icon);}catch(IOException){}
  var identity=new StackPanel{VerticalAlignment=VerticalAlignment.Center};identity.Children.Add(Ui.Text("Dual Discord",Theme.PageSize,"text.primary",true));identity.Children.Add(Ui.Text("Version "+UpdateService.CurrentVersion+" · Windows desktop",Theme.NoteSize,"text.muted"));Grid.SetColumn(identity,1);heading.Children.Add(identity);heroContent.Children.Add(heading);
  var summary=Ui.Text("Run a separate Discord account beside your main one.",Theme.BodySize,"text.secondary");summary.Margin=new Thickness(0,Theme.Gap,0,0);heroContent.Children.Add(summary);var credit=Credit();credit.Margin=new Thickness(0,Theme.Gap,0,0);heroContent.Children.Add(credit);hero.Child=heroContent;panel.Children.Add(hero);panel.Children.Add(Section("UPDATES"));panel.Children.Add(Updates);
  panel.Children.Add(Section("GETTING STARTED"));
  panel.Children.Add(GuideCard(
   new[]{"1 · Open a profile","On Profiles, choose Open next to Main Discord or Second Discord. Sign in once in each window."},
   new[]{"2 · Switch between your accounts","Both windows can stay open together. Focus brings an already running profile back to the front."},
   new[]{"3 · Quit the profile you are finished with","Use Quit next to that profile. The other Discord window keeps running."}));
  panel.Children.Add(Section("SEPARATE PROFILES & SHARING"));
  panel.Children.Add(GuideCard(
   new[]{"One installation, two data folders","Both profiles use the same installed Discord program. The second keeps its own sign-in, cookies, cache and local files."},
   new[]{"Local settings sync is optional","When enabled, supported audio and graphics preferences copy from main to second when the second starts. This does not copy your login. Account and server preferences follow the signed-in account."},
   new[]{"Vencord can be shared or separate","Shared mode uses one Vencord settings and themes folder for both profiles. Separate mode gives the second its own folder. Quit the second profile before changing sharing in Settings."}));
  panel.Children.Add(Section("CLOSING & RESOURCES"));
  panel.Children.Add(GuideCard(
   new[]{"Quit closes the selected Discord fully","The app requests a normal exit, then clears remaining processes belonging to that profile. Its status changes to Closed when no profile processes remain."},
   new[]{"The X hides the second Discord","Its window closes, but the profile keeps running in the background. Use Focus to bring it back, or Quit to stop it fully. Closing this manager leaves Discord running. The main Discord follows its own Windows settings."},
   new[]{"Every open profile uses resources","Each running Discord has its own processes and uses RAM, CPU and GPU. Sharing the installed program does not share live memory. Keep the second closed when you do not need it."}));
  panel.Children.Add(Section("STARTUP"));
  panel.Children.Add(GuideCard(new[]{"Choose what starts with Windows","Settings has separate switches for starting this manager and starting the second profile minimized. These do not replace the main Discord's own startup setting."}));
  panel.Children.Add(Section("FILES & STORAGE"));
  var storage=new Card();var files=new StackPanel();files.Children.Add(Ui.Text("Your app and profile folders",Theme.BodySize,"text.primary",true));var pathText=Ui.Text(home,Theme.NoteSize,"text.secondary");pathText.FontFamily=(FontFamily)Theme.Resources["Font.Mono"];pathText.Margin=new Thickness(0,Theme.Space,0,Theme.Gap);files.Children.Add(pathText);
  files.Children.Add(Ui.Text("The Files page shows the actual folders for each profile, Vencord, the installed program and logs. Use its folder or copy buttons to find them.",Theme.BodySize,"text.secondary"));var redirect=Ui.Text("Windows folder redirects can make a path look different from its physical location. Files shows the actual folder when available.",Theme.NoteSize,"text.muted");redirect.Margin=new Thickness(0,Theme.Gap,0,Theme.Space);files.Children.Add(redirect);var viewFiles=Ui.Button("View folders",false,delegate{Navigate("Files");});viewFiles.Style=Theme.Style("Quiet");viewFiles.HorizontalAlignment=HorizontalAlignment.Left;Theme.Bind(viewFiles,Control.ForegroundProperty,"accent.text");files.Children.Add(viewFiles);storage.Child=files;panel.Children.Add(storage);
  var references=new Expander{Header="Design references",Margin=new Thickness(0,Theme.Padding,0,0),FontSize=Theme.NoteSize};var links=new WrapPanel();foreach(var pair in new[]{new[]{"PowerToys","https://github.com/microsoft/PowerToys"},new[]{"Files","https://github.com/files-community/Files"},new[]{"Flow Launcher","https://github.com/Flow-Launcher/Flow.Launcher"},new[]{"Raycast","https://www.raycast.com/"}}){string url=pair[1];var link=Ui.Button(pair[0],false,delegate{if(ExternalLink!=null)ExternalLink(url);});link.Style=Theme.Style("Quiet");Theme.Bind(link,Control.ForegroundProperty,"accent.text");links.Children.Add(link);}references.Content=links;panel.Children.Add(references);
  return Scroll(panel);
 }
 public void SetOptions(Options options,bool secondRunning){LoadingOptions=true;SettingsRows["StartApp"].Toggle.IsChecked=options.StartApp;SettingsRows["StartSecond"].Toggle.IsChecked=options.StartSecond;SettingsRows["SyncDiscord"].Toggle.IsChecked=options.SyncDiscord;SettingsRows["SyncVencord"].Toggle.IsChecked=options.SyncVencord;SettingsRows["SyncDiscord"].Locked(secondRunning);SettingsRows["SyncVencord"].Locked(secondRunning);LoadingOptions=false;}
 public void SetStats(Instance main,Instance second,bool created){if(!MainProfile.Busy)MainProfile.Update(main);if(!SecondProfile.Busy)SecondProfile.Update(second,created);processDetails.Update(main,second);}
 public void SetInstallation(string message){installationStatus.Text=message;}
 public void Missing(bool value){errorHost.Visibility=value?Visibility.Visible:Visibility.Collapsed;MainProfile.Open.IsEnabled=!value&&!MainProfile.Busy;SecondProfile.Open.IsEnabled=!value&&!SecondProfile.Busy;}
}
}
