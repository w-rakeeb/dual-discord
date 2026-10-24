using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DualDiscord {
 public sealed class ProcessOverview:StackPanel {
  readonly ProfileActivity main=new ProfileActivity("Main Discord");
  readonly ProfileActivity second=new ProfileActivity("Second Discord");
  public ProcessOverview(){
   var profiles=new Grid();
   profiles.ColumnDefinitions.Add(new ColumnDefinition());
   profiles.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
   profiles.ColumnDefinitions.Add(new ColumnDefinition());
   main.Margin=new Thickness(0,0,Theme.CardPadding,0);
   second.Margin=new Thickness(Theme.CardPadding,0,0,0);
   profiles.Children.Add(main);
   var divider=new Border{Width=1};Theme.Bind(divider,Border.BackgroundProperty,"border.subtle");
   Grid.SetColumn(divider,1);profiles.Children.Add(divider);
   Grid.SetColumn(second,2);profiles.Children.Add(second);Children.Add(profiles);
   var footer=new Grid{Margin=new Thickness(0,Theme.CardPadding,0,0)};
   footer.ColumnDefinitions.Add(new ColumnDefinition());
   footer.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
   var help=Ui.Text("Find a process ID in Task Manager → Details.",Theme.NoteSize,"text.muted");
   help.Margin=new Thickness(0,0,Theme.Gap,0);footer.Children.Add(help);
   var live=Ui.Text("Updates automatically",Theme.NoteSize,"text.muted");
   live.VerticalAlignment=VerticalAlignment.Bottom;Grid.SetColumn(live,1);footer.Children.Add(live);
   Children.Add(footer);
  }
  public void Update(Instance first,Instance other){main.Update(first);second.Update(other);}
 }

 public sealed class ProfileActivity:StackPanel {
  readonly string profile;
  readonly StatusPill status=new StatusPill();
  readonly TextBlock processes,memory,pid;readonly TextBlock activity=Ui.Text("Not running",Theme.NoteSize,"text.secondary");
  public ProfileActivity(string name){
   profile=name;
   var heading=new Grid();heading.ColumnDefinitions.Add(new ColumnDefinition());
   heading.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
   var title=Ui.Text(name,Theme.TitleSize,"text.primary",true);title.VerticalAlignment=VerticalAlignment.Center;
   title.Margin=new Thickness(0,0,Theme.Space,0);heading.Children.Add(title);
   Grid.SetColumn(status,1);heading.Children.Add(status);Children.Add(heading);
   var metrics=new UniformGrid{Columns=3,Margin=new Thickness(0,Theme.Gap,0,0)};
   processes=Metric(metrics,"Processes");memory=Metric(metrics,"Memory");pid=Metric(metrics,"Process ID");
   pid.ToolTip="The main process for this profile. Find this ID in Task Manager → Details.";
   Children.Add(metrics);activity.Margin=new Thickness(0,Theme.Gap,0,0);Children.Add(activity);Update(new Instance());
  }
  static TextBlock Metric(Panel parent,string name){
   var column=new StackPanel{Margin=new Thickness(0,0,Theme.Space,0)};
   column.Children.Add(Ui.Text(name,Theme.NoteSize,"text.muted"));
   var value=Ui.Text("—",16,"text.primary",true);
   value.Margin=new Thickness(0,Theme.Space,0,0);value.TextWrapping=TextWrapping.NoWrap;
   value.TextTrimming=TextTrimming.CharacterEllipsis;column.Children.Add(value);parent.Children.Add(column);return value;
  }
  public void Update(Instance instance){
   bool running=instance.Count>0;
   if(instance.Started.HasValue){var elapsed=DateTime.Now-instance.Started.Value;activity.Text="Started "+instance.Started.Value.ToString("HH:mm")+" · Running "+(elapsed.TotalHours>=1?((int)elapsed.TotalHours)+"h ":"")+elapsed.Minutes+"m";}else activity.Text=running?"Running · start time unavailable":"Not running";
   status.Set(running?"Running":"Closed",running?"success":"neutral",profile);
   processes.Text=instance.Count.ToString("N0");memory.Text=(instance.Memory/1048576).ToString("N0")+" MB";
   string ids=String.Join(", ",instance.Roots.Select(p=>p.Id.ToString()));
   pid.Text=running&&ids.Length>0?ids:"—";
   pid.ToolTip=running&&ids.Length>0?"Find "+ids+" in Task Manager → Details.":"No main process is running.";
   Ui.Name(processes,profile+", "+processes.Text+" processes");
   Ui.Name(memory,profile+", memory "+memory.Text);Ui.Name(pid,profile+", process ID "+pid.Text);
  }
 }
}
