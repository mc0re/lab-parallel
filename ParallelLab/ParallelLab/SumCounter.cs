using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


internal class SumCounter
{
	public static void SequentialFor(int count)
	{
		var nums = Enumerable.Range(0, count);
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		var total = 0L;
		foreach (var n in nums)
		{
			total += n;
		}

		stopwatch.Stop();
		Console.WriteLine($"Sequential for: {stopwatch.ElapsedMilliseconds} ms, the total is {total:N0}");
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}


	public static void ParallelFor(int count)
	{
		var nums = Enumerable.Range(0, count);
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		var total = 0L;
		var tlsInit = 0;
		var tlsFinal = 0;

		Parallel.For(0, count,
			new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
			() =>
			{
				Interlocked.Increment(ref tlsInit); return 0L;
			},
			(n, loop, subtotal) =>
			{
				subtotal += n;
				return subtotal;
			},
			(x) =>
			{
				Interlocked.Increment(ref tlsFinal); Interlocked.Add(ref total, x);
			}
		);

		stopwatch.Stop();
		Console.WriteLine($"Parallel for: {stopwatch.ElapsedMilliseconds} ms, the total is {total:N0}");
		Console.WriteLine($"TLS inited {tlsInit} times, finalized {tlsFinal} times");
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}


	public static void ParallelForEach(int count)
	{
		var nums = Enumerable.Range(0, count).ToArray();
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		var total = 0L;
		var tlsInit = 0;
		var tlsFinal = 0;

		// First type parameter is the type of the source elements
		// Second type parameter is the type of the thread-local variable (partition subtotal)
		// The source collection must be of a type supporting Count
		Parallel.ForEach(
			nums, // source collection
			() =>
			{
				Interlocked.Increment(ref tlsInit); return 0L;
			},
			(j, loop, subtotal) => // method invoked by the loop on each iteration
			{
				subtotal += j; //modify local variable
				return subtotal; // value to be passed to next iteration
			},
			// Method to be executed when each partition has completed.
			// finalResult is the final value of subtotal for a particular partition.
			(finalResult) =>
			{
				Interlocked.Increment(ref tlsFinal); Interlocked.Add(ref total, finalResult);
			}
		);

		stopwatch.Stop();
		Console.WriteLine($"Parallel for-each: {stopwatch.ElapsedMilliseconds} ms, the total is {total:N0}");
		Console.WriteLine($"TLS inited {tlsInit} times, finalized {tlsFinal} times");
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}
}
