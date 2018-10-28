using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TaskDataFlow
{
	internal class BufferBlockLab
	{
		internal static void Run(int count)
		{
			// Create a BufferBlock<int> object.
			var bufferBlock = new BufferBlock<int>();

			// Post several messages to the block.
			Enumerable.Range(1, count).AsParallel().ForAll(n => bufferBlock.SendAsync(n));

			// Receive the messages back from the block.
			bufferBlock.TryReceiveAll(out var items);
			Console.WriteLine("{0} items: {1}", items.Count, string.Join(", ", items));
		}
	}
}
