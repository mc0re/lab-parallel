using System;
using System.Threading;
using System.Threading.Tasks;


namespace TaskParallel
{
	internal class TaskStart
	{
		internal static void Run()
		{
			// Create a task and supply a user delegate by using a lambda expression. 
			var taskA = new Task(() =>
				Console.WriteLine("Hello from taskA, thread {0}.", Thread.CurrentThread.ManagedThreadId));
			taskA.Start();

			// Output a message from the calling thread.
			Console.WriteLine("Hello from thread {0} '{1}'.",
							  Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name);
			taskA.Wait();

			Console.WriteLine("RunSynchronously");
			var taskB = new Task(() =>
				Console.WriteLine("Hello from taskB, thread {0}.", Thread.CurrentThread.ManagedThreadId));
			taskB.RunSynchronously();
		}
	}
}
