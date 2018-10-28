using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace TaskDataFlow
{
	internal class TransformManyBlockLab
	{
		internal static void Run()
		{
			const string w1 = "Hello";
			const string w2 = "World";

			// Create a TransformManyBlock<string, char> object that splits
			// a string into its individual characters.
			var transformManyBlock = new TransformManyBlock<string, char>(s => s.ToCharArray());

			// Chained buffer block
			var buffer = new BufferBlock<char>();
			transformManyBlock.LinkTo(buffer);
			transformManyBlock.Completion.ContinueWith(_ => buffer.Complete());

			// Post two messages to the first block.
			transformManyBlock.SendAsync(w1);
			transformManyBlock.SendAsync(w2);
			transformManyBlock.Complete();

			transformManyBlock.Completion.Wait();

			// Receive all output values from the last block.
			IList<char> chars;
			while (! buffer.TryReceiveAll(out chars))
			{
				// Needed if we don't wait for transformManyBlock.Completion
				Console.Write(".");
				Thread.SpinWait(10000);
			}

			Console.WriteLine(chars == null ? "null" : string.Join(", ", chars));
		}
	}
}