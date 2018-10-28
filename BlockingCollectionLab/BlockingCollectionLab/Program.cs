using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;


namespace BlockingCollectionLab
{
	internal class Program
	{
		private const int NofProducers = 20;
		private const int NofConsumers = 3;
		private const int QueueSize = 4;

		private static void Main(string[] args)
		{
			var queue = new BlockingCollection<int>(QueueSize);
			var factory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

			// Producers
			var producers = new Task[NofProducers];
			for (var idx = 0; idx < NofProducers; idx++)
			{
				producers[idx] = Task.Factory.StartNew(arg =>
					{
						var id = (int) arg + 1;
						Console.WriteLine("Task {0} started", id);
						queue.Add((int) id);
						Console.WriteLine("Task {0} finished", id);
					},
					idx);
			}

			// Consumers
			var consumers = new Task[NofConsumers];
			for (var idx = 0; idx < NofConsumers; idx++)
			{
				consumers[idx] = factory.StartNew(arg =>
					{
						var id = (int) arg + 1;
						while (!queue.IsCompleted)
						{
							try
							{
								var item = queue.Take();
								Thread.Sleep(50);
								Console.WriteLine("Consumer {0} took item {1}", id, item);
							}
							catch (ObjectDisposedException)
							{
								// ODE means that Take() was called on a completed collection.
								// Some other thread can call CompleteAdding after we pass the
								// IsCompleted check but before we call Take. 
								// In this example, we can simply catch the exception since the 
								// loop will break on the next iteration.
							}
							catch (InvalidOperationException)
							{
								// IOE means that Take() was called on a completed collection.
								// Some other thread can call CompleteAdding after we pass the
								// IsCompleted check but before we call Take. 
								// In this example, we can simply catch the exception since the 
								// loop will break on the next iteration.
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
						}
					},
				idx);
			}

			Task.WaitAll(producers);
			queue.CompleteAdding();
			Console.WriteLine("All added");
			Task.WaitAll(consumers);
		}
	}
}
