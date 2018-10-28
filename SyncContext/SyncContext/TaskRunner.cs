using System.Threading;

namespace SyncContext
{
	class TaskRunner : IEventListener<ItemQueuedEvent>, IEventListener<ItemCompletedEvent>
	{
		#region Constants

		private int MaxTasks = 3;

		#endregion


		#region Fields

		private EventCentral mEventCentral;

		private int mTaskCounter;

		#endregion


		#region Main

		public TaskRunner(EventCentral ec)
		{
			mEventCentral = ec;
		}


		public void Run()
		{
			mEventCentral.AddListener(this);

			while (true)
			{

			}
		}

		#endregion


		#region Event handlers

		void IEventListener<ItemQueuedEvent>.OnEvent(ItemQueuedEvent ev)
		{
			HandleQueue((TaskQueue) ev.Queue);
		}


		void IEventListener<ItemCompletedEvent>.OnEvent(ItemCompletedEvent ev)
		{
			mTaskCounter--;
			HandleQueue((TaskQueue) ev.Queue);
		}


		private void HandleQueue(TaskQueue queue)
		{
			// Too many - wait for completion
			if (mTaskCounter >= MaxTasks) return;

			// If this is from completion - check if there are any left
			var item = queue.RemoveLast();
			if (string.IsNullOrEmpty(item)) return;

			mTaskCounter++;
			mEventCentral.Publish(new ItemStartedEvent { Item = item });

			// Execute the task
			ThreadPool.QueueUserWorkItem((s) =>
			{
				Thread.Sleep(1000);
				mEventCentral.Publish(new ItemCompletedEvent { Queue = queue, Item = item });
			}, null);
		}

		#endregion
	}
}
