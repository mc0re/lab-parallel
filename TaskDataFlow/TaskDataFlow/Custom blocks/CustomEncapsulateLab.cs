using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace TaskDataFlow
{
	internal class CustomEncapsulateLab
	{
		internal static void Run()
		{
			var src = CreateSlidingWindow<int>(3);
			var printer = new ActionBlock<int[]>(arr => Console.WriteLine(string.Join(", ", arr)));
			src.LinkTo(printer, new DataflowLinkOptions { PropagateCompletion = true });

			for (var i = 1; i <= 5; i++)
			{
				src.Post(i);
			}

			src.Complete();
			printer.Completion.Wait();
		}


		// Creates a IPropagatorBlock<T, T[]> object propagates data in a 
		// sliding window fashion.
		public static IPropagatorBlock<T, T[]> CreateSlidingWindow<T>(int windowSize)
		{
			// Create a queue to hold messages.
			var queue = new Queue<T>();

			// The source part of the propagator holds arrays of size windowSize
			// and propagates data out to any connected targets.
			var sender = new BufferBlock<T[]>();

			// The target part receives data and adds them to the queue.
			var receiver = new ActionBlock<T>(item =>
			{
				// Add the item to the queue.
				queue.Enqueue(item);
				// Remove the oldest item when the queue size exceeds the window size.
				if (queue.Count > windowSize)
					queue.Dequeue();
				// Post the data in the queue to the source block when the queue size
				// equals the window size.
				if (queue.Count == windowSize)
					sender.Post(queue.ToArray());
			});

			// When the target is set to the completed state, propagate out any
			// remaining data and set the source to the completed state.
			receiver.Completion.ContinueWith(delegate
			{
				if (queue.Count > 0 && queue.Count < windowSize)
					sender.Post(queue.ToArray());
				sender.Complete();
			});

			// Return a IPropagatorBlock<T, T[]> object that encapsulates the 
			// target and source blocks.
			return DataflowBlock.Encapsulate(receiver, sender);
		}
	}
}