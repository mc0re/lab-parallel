using System;

internal class Program
{
	private const int ItemCount = 100_000_000;

	private const long LoopCount = 100_000_000;

	private const int NofTasks = 1000;


	private static void Main()
	{
		SumCounter.SequentialFor(ItemCount);
		SumCounter.ParallelFor(ItemCount);
		SumCounter.ParallelForEach(ItemCount);

		Console.WriteLine("Measuring {0} counts", LoopCount);
		ParallelTime.Run(NofTasks, LoopCount / NofTasks);

		ParallelExceptions.Run();
		CustomPartitioner.Run();
	}
}
