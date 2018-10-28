using System;
using System.Threading.Tasks.Dataflow;

namespace TaskDataFlow
{
	internal class CustomPropagatorLab
	{
		internal static void Run()
		{
			var src = new SlidingWindowBlock<char>(4);
			Console.WriteLine($"Window size: {src.WindowSize}");
			var printer = new ActionBlock<char[]>(arr => Console.WriteLine(string.Join(", ", arr)));
			src.LinkTo(printer, new DataflowLinkOptions { PropagateCompletion = true });

			for (var ch = 'a'; ch <= 'f'; ch++)
			{
				src.Post(ch);
			}

			src.Complete();
			printer.Completion.Wait();
		}
	}
}