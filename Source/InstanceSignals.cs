using System;
using System.Threading;
using System.Windows;
namespace DualDiscord {
 public sealed class InstanceSignals:IDisposable {
  readonly EventWaitHandle[] events=new EventWaitHandle[3];readonly RegisteredWaitHandle[] waits=new RegisteredWaitHandle[3];
  static string Name(int index){return "Local\\DualDiscordCommand3-"+index;}
  public InstanceSignals(Window window,Action restore,Action second,Action exit){var actions=new[]{restore,second,exit};for(int i=0;i<3;i++){events[i]=new EventWaitHandle(false,EventResetMode.AutoReset,Name(i));Action action=actions[i];waits[i]=ThreadPool.RegisterWaitForSingleObject(events[i],delegate(object state,bool timeout){if(!window.Dispatcher.HasShutdownStarted)window.Dispatcher.BeginInvoke(action);},null,Timeout.Infinite,false);}}
  public static bool Send(int command){for(int attempt=0;attempt<5;attempt++){try{using(var handle=EventWaitHandle.OpenExisting(Name(command))){handle.Set();return true;}}catch(WaitHandleCannotBeOpenedException){Thread.Sleep(100);}}return false;}
  public void Dispose(){for(int i=0;i<3;i++){waits[i].Unregister(null);events[i].Dispose();}}
 }
}
