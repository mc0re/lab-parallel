using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

class ParallelExceptions
{
	public static void Run()
	{
		var data = new byte[5000];
		var r = new Random();
		r.NextBytes(data);

		try
		{
			ProcessDataInParallel(data);
		}
		catch (AggregateException ae)
		{
			var ignoredExceptions = new List<Exception>();

			// This is where you can choose which exceptions to handle.
			foreach (var ex in ae.Flatten().InnerExceptions)
			{
				if (ex is ArgumentException)
					Console.WriteLine(ex.Message);
				else
					ignoredExceptions.Add(ex);
			}

			if (ignoredExceptions.Count > 0)
				throw new AggregateException(ignoredExceptions);
		}
	}


	private static void ProcessDataInParallel(byte[] data)
	{
		// Use ConcurrentQueue to enable safe enqueueing from multiple threads.
		var exceptions = new ConcurrentQueue<Exception>();

		Parallel.ForEach(data, d =>
			{
				try
				{
					// Cause a few exceptions, but not too many.
					if (d < 3)
						throw new ArgumentException($"Value is {d}. Value must be greater than or equal to 3.");
					else
						Console.Write($"{d} ");
				}
				catch (Exception e)
				{
					// Store the exception and continue with the loop.                    
					exceptions.Enqueue(e);
				}
			});
		Console.WriteLine();

		// Throw the exceptions here after the loop completes.
		if (exceptions.Count > 0)
			throw new AggregateException(exceptions);
	}
}