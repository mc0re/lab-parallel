using System;
using System.Threading;


namespace SyncContext
{
	public class ListenerInfo
	{
		public WeakReference Reference;

		public SynchronizationContext Context;
	}
}
