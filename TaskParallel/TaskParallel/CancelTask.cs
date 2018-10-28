using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskParallel
{
	internal class CancelTask
	{
		internal static void Run()
		{
			var cts = new CancellationTokenSource();
			var tsk = Task.Factory.StartNew(() => TaskMain(cts.Token), cts.Token);
			cts.CancelAfter(100);

			try
			{
				tsk.Wait();
			}
			catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
			{
				Console.WriteLine("Exception: {0}", ex.InnerException.Message);
			}

			Console.WriteLine("Task exception: {0}", tsk.Exception?.InnerException.Message ?? "null");
		}


		private static void TaskMain(CancellationToken ct)
		{
			while (true)
			{
				Console.WriteLine("Awaiting cancel...");
				Thread.Sleep(50);
				ct.ThrowIfCancellationRequested();
			}
		}
	}
}