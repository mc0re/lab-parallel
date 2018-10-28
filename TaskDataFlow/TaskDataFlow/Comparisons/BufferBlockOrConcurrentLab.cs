using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TaskDataFlow
{
	internal class BufferBlockOrConcurrentLab
	{
		internal static void Run(int count)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			var stopwatch = new Stopwatch();

			// Variant 1: ConcurrentBag
			stopwatch.Start();

			var collection = new ConcurrentBag<int>();
			Parallel.For(0, count, a => collection.Add(a));
			var list1 = collection.ToList();

			stopwatch.Stop();
			var collTime = stopwatch.ElapsedMilliseconds;
			Console.WriteLine($"ConcurrentBag: {list1.Count} items, {collTime} ms");

			list1 = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();

			// Variant 2: Using Dataflow.Post
			stopwatch.Restart();

			var bl = new BufferBlock<int>(new DataflowBlockOptions { MaxMessagesPerTask = DataflowBlockOptions.Unbounded });
			Parallel.For(0, count, a => bl.Post(a));
			bl.Complete();
			bl.TryReceiveAll(out var list2);

			stopwatch.Stop();
			var postTime = stopwatch.ElapsedMilliseconds;
			Console.WriteLine($"Dataflow.Post: {list2.Count} items, {postTime} ms");

			list2 = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();

			// Variant 3: Using Dataflow.SendAsync
			stopwatch.Restart();

			var blAsync = new BufferBlock<int>(new DataflowBlockOptions { MaxMessagesPerTask = DataflowBlockOptions.Unbounded });
			Parallel.For(0, count, a => blAsync.SendAsync(a));
			blAsync.Complete();
			blAsync.TryReceiveAll(out var list3);

			stopwatch.Stop();
			var asyncTime = stopwatch.ElapsedMilliseconds;
			Console.WriteLine($"Dataflow.SendAsync: {list3.Count} items, {asyncTime} ms");

			list3 = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();

			// Variant 4: Locked list
			var lockObj = new object();
			stopwatch.Restart();

			var list4 = new List<int>();
			Parallel.For(0, count, a => { lock (lockObj) list4.Add(a); });

			stopwatch.Stop();
			var lockTime = stopwatch.ElapsedMilliseconds;
			Console.WriteLine($"Lock: {list4.Count} items, {lockTime} ms");

			list4 = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}