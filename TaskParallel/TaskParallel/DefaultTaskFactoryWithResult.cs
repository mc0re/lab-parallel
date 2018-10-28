using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaskParallel
{
	internal class DefaultTaskFactoryWithResult
	{
		internal static void Run(params double[] values)
		{
			var taskArray =
				(from v in values select Task<double>.Factory.StartNew(() => DoComputation(v)))
				.ToList();

			var results = new double[taskArray.Count];
			double sum = 0;

			for (int i = 0; i < taskArray.Count; i++)
			{
				// Blocks until the result is ready
				results[i] = taskArray[i].Result;
				Console.Write("{0:N1} {1}", results[i], i == taskArray.Count - 1 ? "= " : "+ ");
				sum += results[i];
			}

			Console.WriteLine("{0:N1}", sum);
		}


		private static double DoComputation(double start)
		{
			double sum = 0;

			for (var value = start; value <= start + 10; value += .1)
				sum += value;

			return sum;
		}
	}
}