using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
namespace DualDiscord {
 public static class ProfileStorage {
  public static string Resolve(string selected,string legacy,string install){if(!String.IsNullOrWhiteSpace(selected))return Path.GetFullPath(selected);return Directory.Exists(Path.Combine(legacy,"discord"))?legacy:Path.Combine(install,"Profiles","Second");}
  static string Canonical(string path){path=Path.GetFullPath(path).TrimEnd('\\');if(Directory.Exists(path)||File.Exists(path))return Installation.Physical(path);string parent=Path.GetDirectoryName(path);return String.IsNullOrEmpty(parent)?path:Path.Combine(Canonical(parent),Path.GetFileName(path));}
  static bool Overlaps(string a,string b){return a.Equals(b,StringComparison.OrdinalIgnoreCase)||a.StartsWith(b+"\\",StringComparison.OrdinalIgnoreCase)||b.StartsWith(a+"\\",StringComparison.OrdinalIgnoreCase);}
  static List<string> Files(string root){var result=new List<string>();Scan(root,root,result);return result;}
  static void Scan(string root,string folder,List<string> files){foreach(string entry in Directory.GetFileSystemEntries(folder)){var attributes=File.GetAttributes(entry);if((attributes&FileAttributes.ReparsePoint)!=0)throw new IOException("This profile contains a linked folder or file. Choose a regular folder for relocation.");if((attributes&FileAttributes.Directory)!=0)Scan(root,entry,files);else files.Add(entry.Substring(root.Length+1));}}
  static string Hash(string path){using(var hash=SHA256.Create())using(var stream=File.OpenRead(path))return Convert.ToBase64String(hash.ComputeHash(stream));}
  public static void CopyVerified(string source,string target,string[] protectedRoots){
   source=Canonical(source);target=Canonical(target);
   if(!Directory.Exists(source))throw new IOException("The original profile folder is unavailable. Its location has not been changed.");
   if(String.IsNullOrEmpty(Path.GetPathRoot(target))||target.Length<=3||Overlaps(source,target))throw new IOException("Choose a separate, empty folder for the second profile.");
   foreach(string path in protectedRoots)if(Overlaps(target,Canonical(path)))throw new IOException("Choose a folder outside the main profile and this app's files.");
   if(Directory.Exists(target)&&Directory.GetFileSystemEntries(target).Length>0)throw new IOException("The selected folder is not empty. Choose a new or empty folder.");
   if(File.Exists(target))throw new IOException("Choose a folder, not a file.");
   var files=Files(source);Directory.CreateDirectory(target);
   // Source remains intact on success or failure. A partial destination is never selected.
   foreach(string relative in files){string from=Path.Combine(source,relative),to=Path.Combine(target,relative);Directory.CreateDirectory(Path.GetDirectoryName(to));File.Copy(from,to,false);if(Hash(from)!=Hash(to))throw new IOException("Copy verification failed. The original profile is still selected.");}
   if(Files(source).Count!=files.Count)throw new IOException("The source changed during copying. The original profile is still selected.");
   Directory.CreateDirectory(Path.Combine(target,"discord"));
  }
 }
}
