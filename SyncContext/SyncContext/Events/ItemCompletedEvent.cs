using System.Collections.Specialized;

namespace SyncContext
{
	public class ItemCompletedEvent
	{
		public INotifyCollectionChanged Queue;

		public string Item;
	}
}
