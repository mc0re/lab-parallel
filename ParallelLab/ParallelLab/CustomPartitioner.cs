using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

internal class CustomPartitioner
{
	internal static void Run()
	{
		// Source must be array or IList.
		var source = Enumerable.Range(0, 100000).ToArray();

		// Partition the entire source array into static chunks, no work-stealing
		var rangePartitioner = Partitioner.Create(0, source.Length);

		double[] results = new double[source.Length];

		// Loop over the partitions in parallel.
		Parallel.ForEach(rangePartitioner, (range, loopState) =>
		{
			Console.WriteLine($"Start range {range.Item1}-{range.Item2}");

			// Loop over each range element without a delegate invocation.
			for (int i = range.Item1; i < range.Item2; i++)
			{
				results[i] = source[i] * Math.PI;
			}
		});

		Console.WriteLine("Array processed");
	}
}