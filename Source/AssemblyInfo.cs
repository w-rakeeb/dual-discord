using System.Reflection;
using System.Runtime.InteropServices;
[assembly:AssemblyTitle("Dual Discord")]
[assembly:AssemblyProduct("Dual Discord")]
[assembly:AssemblyCompany("Wrakeeb")]
[assembly:AssemblyVersion("2.7.0.0")]
[assembly:AssemblyFileVersion("2.7.0.0")]
[assembly:ComVisible(false)]

#if PERSONAL
[assembly:AssemblyMetadata("DualDiscord.Edition","Personal")]
#else
[assembly:AssemblyMetadata("DualDiscord.Edition","Public")]
#endif
