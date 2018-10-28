using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLinqExceptionLab
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("*** Exception stops the loop");
			ForAllStops();

			Console.WriteLine("*** Exception is caught in the loop");
			ForAllContinues();
		}


		private static void ForAllStops()
		{
			var numbers = Enumerable.Range(1, 100);

			// Produces exceptions
			var query =
				from n in numbers.AsParallel()
				select 1000 / (n % 5);

			try
			{
				// The exception stops the loop
				query.ForAll(n => Console.Write("{0}, ", n));
			}
			catch (AggregateException aex)
			{
				foreach (var ex in aex.Flatten().InnerExceptions)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}


		private static void ForAllContinues()
		{
			var numbers = Enumerable.Range(1, 100);

			int SafeDivide(int a, int b)
			{
				try
				{
					return a / b;
				}
				catch (Exception ex)
				{
					Console.Write(ex.Message + " =>");
					return 0;
				}
			}

			// Produces exceptions
			var query =
				from n in numbers.AsParallel()
				select SafeDivide(1000, (n % 5));

			query.ForAll(n => Console.Write("{0}, ", n));
			Console.WriteLine();
		}
	}
}
