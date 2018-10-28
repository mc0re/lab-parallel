using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

internal class ParallelTime
{
	internal static void Run(int nofTasks, long countPerTask)
	{
		var sw = new Stopwatch();
		long hits = 0;

		// Sequential reference
		hits = 0;

		sw.Restart();
		for (var i = 0L; i < countPerTask * nofTasks; i++)
		{
			hits++;
		}
		sw.Stop();

		Console.WriteLine("Sequential: {0} hits in {1} ms", hits, sw.ElapsedMilliseconds);

		// Parallel reference
		hits = 0;

		sw.Restart();
		Parallel.For(0L, nofTasks, _ =>
			{
				for (var i = 0; i < countPerTask; i++)
					hits++;
			});
		sw.Stop();

		Console.WriteLine("No protection: {0} hits in {1} ms", hits, sw.ElapsedMilliseconds);

		// Interlocked
		hits = 0;

		sw.Restart();
		Parallel.For(0L, nofTasks, _ =>
			{
				for (var i = 0; i < countPerTask; i++)
					Interlocked.Increment(ref hits);
			});
		sw.Stop();

		Console.WriteLine("Interlocked.Increment: {0} hits in {1} ms", hits, sw.ElapsedMilliseconds);

		// lock{}
		hits = 0;
		var lockObj = new object();

		sw.Restart();
		Parallel.For(0L, nofTasks, _ =>
		{
			for (var i = 0; i < countPerTask; i++)
				lock (lockObj) hits++;
		});
		sw.Stop();

		Console.WriteLine("Lock: {0} hits in {1} ms", hits, sw.ElapsedMilliseconds);

		// lock-free, task-local data
		hits = 0;

		sw.Restart();
		Parallel.For(
			0L, nofTasks,
			() => 0L,
			(idx, state, prev) =>
				{
					for (var i = 0; i < countPerTask; i++)
						prev++;
					return prev;
				},
			res => Interlocked.Add(ref hits, res));
		sw.Stop();

		Console.WriteLine("Lock-free: {0} hits in {1} ms", hits, sw.ElapsedMilliseconds);
	}
}