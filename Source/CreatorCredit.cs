using System;
using System.Windows;
using System.Windows.Controls;

namespace DualDiscord {
 // Shared presentation for the Profiles footer and About credit.
 public sealed class CreatorCredit:Grid {
  public const string CreatorName="Wrakeeb",GitHubUrl="https://github.com/w-rakeeb";
  public CreatorCredit(Action<string> openLink):this(CreatorName,GitHubUrl,openLink){}
  public CreatorCredit(string name,string github,Action<string> openLink){
   HorizontalAlignment=HorizontalAlignment.Right;
   ColumnDefinitions.Add(new ColumnDefinition());
   ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
   var creator=Ui.Text("Created by "+name,Theme.NoteSize,"text.secondary");
   creator.VerticalAlignment=VerticalAlignment.Center;
   creator.TextWrapping=TextWrapping.NoWrap;
   creator.TextTrimming=TextTrimming.CharacterEllipsis;
   creator.MaxWidth=240;creator.ToolTip=creator.Text;
   creator.Margin=new Thickness(0,0,Theme.Space,0);
   Children.Add(creator);
   var link=Ui.Button("GitHub ↗",false,delegate{if(openLink!=null)openLink(github);});
   link.Style=Theme.Style("Quiet");
   link.FontSize=Theme.NoteSize;
   Theme.Bind(link,Control.ForegroundProperty,"accent.text");
   link.ToolTip=github;Ui.Name(link,"Open "+name+" on GitHub");
   Grid.SetColumn(link,1);Children.Add(link);
  }
 }
}
