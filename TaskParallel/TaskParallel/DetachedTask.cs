using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskParallel
{
	internal class DetachedTask
	{
		internal static void Run()
		{
			Task child = null;
			var outer = Task.Factory.StartNew(() =>
			{
				Console.WriteLine("Outer task beginning.");

				child = Task.Factory.StartNew(() =>
				{
					Console.WriteLine("Inner task beginning.");
					Thread.SpinWait(5000000);
					Console.WriteLine("Inner task completed.");
				});
			});

			outer.Wait();
			Console.WriteLine("Outer task completed.");
			child.Wait();
			Console.WriteLine("All tasks completed.");
		}
	}
}