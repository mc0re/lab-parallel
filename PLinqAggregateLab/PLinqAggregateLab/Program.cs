using System;
using System.Linq;


namespace PLinqAggregateLab
{
	class Program
	{
		static void Main(string[] args)
		{
			// Create a data source for demonstration purposes.
			int[] source = new int[100000];
			Random rand = new Random();
			for (int x = 0; x < source.Length; x++)
			{
				// Should result in a mean of approximately 15.0.
				source[x] = rand.Next(10, 20);
			}

			// Standard deviation calculation requires that we first
			// calculate the mean average. Average is a predefined
			// aggregation operator, along with Max, Min and Count.
			double mean = source.AsParallel().Average();

			// We use the overload that is unique to ParallelEnumerable. The 
			// third Func parameter combines the results from each thread.
			double standardDev = source.AsParallel().Aggregate(
				// initialize subtotal. Use decimal point to tell 
				// the compiler this is a type double. Can also use: 0d.
				0.0,

				// do this on each thread (partition)
				(subtotal, item) => subtotal + Math.Pow((item - mean), 2),

				// aggregate results after each thread is done
				(total, thisThread) => total + thisThread,

				// perform standard deviation calc on the aggregated result.
				(finalSum) => Math.Sqrt((finalSum / (source.Length - 1)))
			);
			Console.WriteLine("Mean value is = {0}", mean);
			Console.WriteLine("Standard deviation is {0}", standardDev);
			Console.ReadLine();
		}
	}
}