using System;


namespace TaskDataFlow
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("--- Do some and complete ---");
			CompleteWhenDone.Run();

			Console.WriteLine("--- Complete by exception ---");
			CompleteByException.Run();

			Console.WriteLine("--- BufferBlock ---");
			BufferBlockLab.Run(100);

			Console.WriteLine("--- BufferBlock vs Concurrent ---");
			BufferBlockOrConcurrentLab.Run(50_000_000);

			Console.WriteLine("--- BroadcastBlock ---");
			BroadcastBlockLab.Run(100);

			Console.WriteLine("--- ActionBlock ---");
			ActionBlockLab.Run(20);

			Console.WriteLine("--- TransformManyBlock ---");
			TransformManyBlockLab.Run();

			Console.WriteLine("--- BatchBlock ---");
			BatchBlockLab.Run();

			Console.WriteLine("--- BatchBlock with DB ---");
			BatchBlockWithDbLab.Run();

			Console.WriteLine("--- JoinBlock ---");
			JoinBlockLab.Run();

			Console.WriteLine("--- Non-greedy join ---");
			NonGreedyJoinLab.Run();

			Console.WriteLine("--- File read chain ---");
			FileReadChainLab.Run();

			Console.WriteLine("--- Download chain ---");
			//DownloadChainLab.Run();

			Console.WriteLine("--- Custom encapsulated block ---");
			CustomEncapsulateLab.Run();

			Console.WriteLine("--- Custom propagator block ---");
			CustomPropagatorLab.Run();

			Console.WriteLine("--- Parallel processing ---");
			ParallelProcesingLab.Run();
		}
	}
}
