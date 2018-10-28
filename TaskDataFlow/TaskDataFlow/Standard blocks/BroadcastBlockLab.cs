using System;
using System.Threading.Tasks.Dataflow;

namespace TaskDataFlow
{
	internal class BroadcastBlockLab
	{
		internal static void Run(int v)
		{
			// Create a BroadcastBlock<double> object.
			var broadcastBlock = new BroadcastBlock<double>(null);

			// Post a message to the block.
			broadcastBlock.Post(Math.PI);

			// Receive the messages back from the block several times.
			for (int i = 0; i < 3; i++)
			{
				Console.WriteLine(broadcastBlock.Receive());
			}
		}
	}
}
