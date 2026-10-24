using System;using System.IO;using System.Reflection;using System.Linq;
namespace DualDiscord {
 public static class Branding {
  public static void Ensure(string home){foreach(string ext in new[]{"svg","png","ico"}){string path=Path.Combine(home,"Assets","Dualcord."+ext);using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("DualDiscord.Dualcord."+ext))using(var memory=new MemoryStream()){if(stream==null)throw new IOException("Dualcord artwork is missing from this build.");stream.CopyTo(memory);byte[] bytes=memory.ToArray();if(File.Exists(path)&&File.ReadAllBytes(path).SequenceEqual(bytes))continue;Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";File.WriteAllBytes(temp,bytes);if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);}}}
 }
}
