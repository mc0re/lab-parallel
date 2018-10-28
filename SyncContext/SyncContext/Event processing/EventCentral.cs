using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;


namespace SyncContext
{
	public class EventCentral
	{
		#region Subscribers dictionary

		private readonly ConcurrentDictionary<Type, List<ListenerInfo>> mListeners =
			new ConcurrentDictionary<Type, List<ListenerInfo>>();


		private List<ListenerInfo> GetListeners(Type eventType)
		{
			return mListeners.GetOrAdd(eventType, (t) => new List<ListenerInfo>());
		}

		#endregion


		#region Add a listener
		
		/// <summary>
		/// Add a listener as <see cref="WeakReference"/> and its <see cref="SynchronizationContext"/>.
		/// </summary>
		public void AddListener(object listener)
		{
			var eventTypes = listener.GetType().GetInterfaces().
				Where(inf => inf.IsGenericType && inf.GetGenericTypeDefinition() == typeof(IEventListener<>));

			foreach (var eventType in eventTypes)
			{
				this.GetListeners(eventType).Add(new ListenerInfo
				{
					Reference = new WeakReference(listener),
					Context = SynchronizationContext.Current ?? new SynchronizationContext()
				});
			}

		}

		#endregion


		#region Queue publisher

		public void AddQueuePublisher(INotifyCollectionChanged queue)
		{
			queue.CollectionChanged += QueueChangedHandler;
		}


		private void QueueChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				Publish(new ItemQueuedEvent { Queue = (INotifyCollectionChanged) sender });
			}
		}

		#endregion


		#region Generic event publisher

		public void Publish<TEvent>(TEvent @event)
		{
			var listeners = GetListeners(typeof(IEventListener<TEvent>));
			var listenersToRemove = new List<ListenerInfo>();

			foreach (var listenerInfo in listeners)
			{
				if (listenerInfo.Reference.IsAlive)
				{
					var listener = (IEventListener<TEvent>) listenerInfo.Reference.Target;
					listenerInfo.Context.Post(s => listener.OnEvent(@event), null);
				}
				else
				{
					listenersToRemove.Add(listenerInfo);
				}
			}

			foreach (var remove in listenersToRemove)
			{
				listeners.Remove(remove);
			}
		}

		#endregion
	}
}
