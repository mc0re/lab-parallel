using System;
using System.Threading.Tasks.Dataflow;


namespace TaskDataFlow
{
	class ActionBlockLab
	{
		internal static void Run(int count)
		{
			var sourceBlock = new BufferBlock<int>();

			// Create an ActionBlock<int> object that prints values
			// to the console.
			var actionBlock = new ActionBlock<int>(n => Console.Write("{0} ", n),
				new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded });

			// Create a link to only pass even values.
			sourceBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true }, a => a % 2 == 0);

			// Post several messages to the block.
			for (int n = 1; n <= count; n++)
			{
				sourceBlock.Post(n);
			}

			// Set the block to the completed state.
			sourceBlock.Complete();

			// To be ignored
			//sourceBlock.Post(1000);

			// Wait for all tasks to finish.
			if (! actionBlock.Completion.Wait(1000))
			{
				Console.WriteLine("Timeout, Completion is not propagated");
			}
			else
			{
				Console.WriteLine();
			}
		}
	}
}
