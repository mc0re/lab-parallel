using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLinqTakeLab
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Find two numbers dividable by 6 using an unordered parallel loop.");
			Console.WriteLine("The results are different in different runs.");

			var ans = new List<string>();

			for (var i = 0; i < 100; i++)
				ans.Add(RunLoops());

			foreach (var s in ans.Distinct().OrderBy(a => a))
			{
				Console.WriteLine(s);
			}
		}


		private static string RunLoops()
		{
			var seq = Enumerable.Range(1, 1000);

			var res2 = (
				from n in seq.AsParallel() //.AsOrdered()
				where n % 2 == 0
				select n
				).Take(20)
				.ToList();

			var res3 = (
				from n in res2.AsParallel()
				where n % 3 == 0
				select n
				).Take(2)
				.Reverse();

			return string.Join(", ", res3);
		}
	}
}
