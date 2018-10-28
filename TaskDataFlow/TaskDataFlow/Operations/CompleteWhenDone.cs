using System;
using System.Threading.Tasks.Dataflow;

namespace TaskDataFlow
{
	internal class CompleteWhenDone
	{
		internal static void Run()
		{
			// Create an ActionBlock<int> object that prints its input.
			var printMe = new ActionBlock<int>(n =>
			{
				Console.WriteLine("n = {0}", n);
			});

			// Create a continuation task that prints the overall 
			// task status to the console when the block finishes.
			var lastTask = printMe.Completion.ContinueWith(task =>
			{
				Console.WriteLine("The status of the completion task is '{0}'.", task.Status);
			});

			// Only good values, POst.
			printMe.Post(0);
			printMe.Post(1);
			printMe.Post(2);
			printMe.Post(3);
			printMe.Complete();
			lastTask.Wait();
		}
	}
}