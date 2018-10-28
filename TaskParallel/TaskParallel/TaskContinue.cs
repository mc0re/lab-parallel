using System;
using System.Linq;
using System.Threading.Tasks;


namespace TaskParallel
{
	class TaskContinue
	{
		internal static void Run(int nofNumbers)
		{
			var displayData = Task.Factory
			.StartNew(() =>
			{
				var rnd = new Random();
				return Enumerable.Range(0, nofNumbers).Select(_ => rnd.Next(100)).ToArray();
			})
			.ContinueWith((x) =>
			{
				var n = x.Result.Length;
				var sum = x.Result.Select(a => (long) a).Sum();
				double mean = sum / (double) n;
				return Tuple.Create(n, sum, mean);
			})
			.ContinueWith((x) =>
			{
				return string.Format("N={0:N0}, Total = {1:N0}, Mean = {2:N2}",
									 x.Result.Item1, x.Result.Item2,
									 x.Result.Item3);
			});

			Console.WriteLine(displayData.Result);
		}
	}
}
