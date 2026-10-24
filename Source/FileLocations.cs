using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DualDiscord {
// Presentation only: paths and actions are supplied by the existing adapter.
public class FileLocations:StackPanel {
 public Action<string> OpenFolder,CopyPath;
 public void Update(IEnumerable<KeyValuePair<string,string>> entries){
  Children.Clear();var files=entries.ToList();
  var columns=new Grid();columns.ColumnDefinitions.Add(new ColumnDefinition());columns.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(Theme.Gap)});columns.ColumnDefinitions.Add(new ColumnDefinition());
  var main=Column("Main Discord","Your original profile.");var second=Column("Dualcord","The second profile and its active Vencord folder.");
  foreach(var entry in files){if(entry.Key.StartsWith("Main ",StringComparison.Ordinal))main.Children.Add(Location(entry));else if(entry.Key.StartsWith("Second ",StringComparison.Ordinal)||entry.Key=="Separate Vencord data")second.Children.Add(Location(entry));}
  columns.Children.Add(main);Grid.SetColumn(second,2);columns.Children.Add(second);Children.Add(columns);
  var shared=Column("Shared files","The Discord installation and Dual Discord utility.");shared.Margin=new Thickness(0,Theme.Gap,0,0);
  foreach(var entry in files.Where(e=>!e.Key.StartsWith("Main ",StringComparison.Ordinal)&&!e.Key.StartsWith("Second ",StringComparison.Ordinal)&&e.Key!="Separate Vencord data"))shared.Children.Add(Location(entry));
  Children.Add(shared);
 }
 StackPanel Column(string title,string detail){var p=new StackPanel();var heading=Ui.Text(title,Theme.TitleSize,"text.primary",true);p.Children.Add(heading);var hint=Ui.Text(detail,Theme.NoteSize,"text.secondary");hint.MinHeight=Theme.FileHintHeight;hint.Margin=new Thickness(0,Theme.Space,0,Theme.Gap);p.Children.Add(hint);return p;}
 Card Location(KeyValuePair<string,string> entry){
  string path=entry.Value;var card=new Card{MinHeight=Theme.FileRowHeight,Margin=new Thickness(0,0,0,Theme.Gap)};
  var layout=new Grid();layout.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});layout.RowDefinitions.Add(new RowDefinition());
  var header=new Grid();header.ColumnDefinitions.Add(new ColumnDefinition());header.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
  string label=entry.Key.Replace("Main ","").Replace("Second ","").Replace("Separate ","");label=Char.ToUpperInvariant(label[0])+label.Substring(1);var title=Ui.Text(label,Theme.BodySize,"text.primary",true);title.VerticalAlignment=VerticalAlignment.Center;title.Margin=new Thickness(0,0,Theme.Space,0);header.Children.Add(title);
  var actions=new StackPanel{Orientation=Orientation.Horizontal};actions.Children.Add(Ui.IconButton("\uE8B7","Open "+entry.Key+" folder",delegate{if(OpenFolder!=null)OpenFolder(path);}));actions.Children.Add(Ui.IconButton("\uE8C8","Copy "+entry.Key+" path",delegate{if(CopyPath!=null)CopyPath(path);}));Grid.SetColumn(actions,1);header.Children.Add(actions);layout.Children.Add(header);
  var text=Ui.Text(path,Theme.NoteSize,"text.secondary");text.FontFamily=(FontFamily)Theme.Resources["Font.Mono"];text.Margin=new Thickness(0,Theme.Space,0,0);text.ToolTip=path;Grid.SetRow(text,1);layout.Children.Add(text);card.Child=layout;return card;
 }
}
}
