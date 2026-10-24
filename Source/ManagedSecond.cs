using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
namespace DualDiscord {
 // Windows closes this job's assigned second-profile processes if the manager exits.
 public sealed class ManagedSecond:IDisposable {
  [StructLayout(LayoutKind.Sequential)]struct Basic {public long PerProcessUserTimeLimit,PerJobUserTimeLimit;public uint LimitFlags;public UIntPtr MinimumWorkingSetSize,MaximumWorkingSetSize;public uint ActiveProcessLimit;public UIntPtr Affinity;public uint PriorityClass,SchedulingClass;}
  [StructLayout(LayoutKind.Sequential)]struct Counters {public ulong ReadOperationCount,WriteOperationCount,OtherOperationCount,ReadTransferCount,WriteTransferCount,OtherTransferCount;}
  [StructLayout(LayoutKind.Sequential)]struct Limits {public Basic BasicLimitInformation;public Counters IoInfo;public UIntPtr ProcessMemoryLimit,JobMemoryLimit,PeakProcessMemoryUsed,PeakJobMemoryUsed;}
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern SafeFileHandle CreateJobObject(IntPtr security,string name);
  [DllImport("kernel32.dll",SetLastError=true)]static extern bool SetInformationJobObject(SafeFileHandle job,int info,ref Limits limits,uint size);
  [DllImport("kernel32.dll",SetLastError=true)]static extern bool AssignProcessToJobObject(SafeFileHandle job,IntPtr process);
  [DllImport("kernel32.dll",SetLastError=true)]static extern bool IsProcessInJob(IntPtr process,SafeFileHandle job,out bool contained);
  readonly SafeFileHandle handle;readonly object gate=new object();
  public ManagedSecond(){handle=CreateJobObject(IntPtr.Zero,null);if(handle.IsInvalid)throw new Win32Exception();SetKill(true);}
  void SetKill(bool value){var limits=new Limits();limits.BasicLimitInformation.LimitFlags=value?0x2000u:0u;if(!SetInformationJobObject(handle,9,ref limits,(uint)Marshal.SizeOf(typeof(Limits))))throw new Win32Exception();}
  public void Assign(Process process){lock(gate){if(process.HasExited)return;bool contained;if(IsProcessInJob(process.Handle,handle,out contained)&&contained)return;if(!AssignProcessToJobObject(handle,process.Handle)&&!process.HasExited)throw new Win32Exception(Marshal.GetLastWin32Error(),"The second profile could not be bound to this manager.");}}
  public void PreserveForRestart(){lock(gate){if(!handle.IsClosed)SetKill(false);}}
  public void Dispose(){lock(gate)handle.Dispose();}
 }
}
