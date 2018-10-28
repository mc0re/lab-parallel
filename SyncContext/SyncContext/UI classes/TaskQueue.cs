using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Threading;


namespace SyncContext
{
	public class TaskQueue : ConcurrentQueue<string>, INotifyCollectionChanged
	{
		#region Events

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion


		#region Fields

		private SynchronizationContext mUiContext;

		#endregion


		#region Init and clean-up
		
		/// <summary>
		/// Must be called from UI thread.
		/// </summary>
		public TaskQueue()
		{
			mUiContext = SynchronizationContext.Current ?? new SynchronizationContext();
		}

		#endregion


		public void Add(string item)
		{
			base.Enqueue(item);

			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
		}


		public string RemoveLast()
		{
			string item = null;

			if (base.TryDequeue(out item))
			{
				// This event is also listened by UI thread
				mUiContext.Post(
					s => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, 0)),
					null
				);
			}

			return item;
		}
	}
}
