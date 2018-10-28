using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskParallel
{
	internal class TaskRun
	{
		internal static void Run()
		{
			// Define and run the task.
			var taskA = Task.Run(() =>
				Console.WriteLine("Hello from taskA, thread {0}.", Thread.CurrentThread.ManagedThreadId));

			Console.WriteLine("Hello from thread {0} '{1}'.",
							  Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name);
			taskA.Wait();
		}
	}
}