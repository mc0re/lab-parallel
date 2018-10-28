using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TaskDataFlow
{
	internal class NonGreedyJoinLab
	{
		#region Helper classes

		// Represents a resource. A derived class might represent 
		// a limited resource such as a memory, network, or I/O
		// device.
		abstract class Resource
		{
		}


		// Represents a memory resource. For brevity, the details of 
		// this class are omitted.
		class MemoryResource : Resource
		{
		}


		// Represents a network resource. For brevity, the details of 
		// this class are omitted.
		class NetworkResource : Resource
		{
		}


		// Represents a file resource. For brevity, the details of 
		// this class are omitted.
		class FileResource : Resource
		{
		}

		#endregion


		internal static void Run()
		{
			var cts = new CancellationTokenSource();

			// Create three BufferBlock<T> objects. Each object holds a different
			// type of resource.
			var networkResources = new BufferBlock<NetworkResource>(new DataflowBlockOptions { CancellationToken = cts.Token });
			var fileResources = new BufferBlock<FileResource>(new DataflowBlockOptions { CancellationToken = cts.Token });
			var memoryResources = new BufferBlock<MemoryResource>(new DataflowBlockOptions { CancellationToken = cts.Token });

			// Create two non-greedy JoinBlock<T1, T2> objects. 
			// The first join works with network and memory resources; 
			// the second pool works with file and memory resources.

			var joinNetworkAndMemoryResources =
			   new JoinBlock<NetworkResource, MemoryResource>(
				  new GroupingDataflowBlockOptions { Greedy = false });

			var joinFileAndMemoryResources =
			   new JoinBlock<FileResource, MemoryResource>(
				  new GroupingDataflowBlockOptions { Greedy = false });

			// Create two ActionBlock<Tuple<,>> objects. 
			// The first block acts on a network resource and a memory resource.
			// The second block acts on a file resource and a memory resource.

			var networkMemoryAction =
				new ActionBlock<Tuple<NetworkResource, MemoryResource>>(
					data => NetworkProcessor(data, networkResources, memoryResources),
					new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

			var fileMemoryAction =
				new ActionBlock<Tuple<FileResource, MemoryResource>>(
					data => FileProcessor(data, fileResources, memoryResources));

			// Link the resource pools to the JoinBlock<T1, T2> objects.
			// Because these join blocks operate in non-greedy mode, they do not
			// take the resource from a pool until all resources are available from
			// all pools.

			var completed = new DataflowLinkOptions { PropagateCompletion = true };
			networkResources.LinkTo(joinNetworkAndMemoryResources.Target1, completed);
			memoryResources.LinkTo(joinNetworkAndMemoryResources.Target2, completed);

			fileResources.LinkTo(joinFileAndMemoryResources.Target1, completed);
			memoryResources.LinkTo(joinFileAndMemoryResources.Target2, completed);

			// Link the JoinBlock<T1, T2> objects to the ActionBlock<T> objects.

			joinNetworkAndMemoryResources.LinkTo(networkMemoryAction, completed);
			joinFileAndMemoryResources.LinkTo(fileMemoryAction, completed);

			// Populate the resource pools. In this example, network and 
			// file resources are more abundant than memory resources.

			networkResources.Post(new NetworkResource());
			networkResources.Post(new NetworkResource());
			networkResources.Post(new NetworkResource());

			memoryResources.Post(new MemoryResource());
			memoryResources.Post(new MemoryResource());

			fileResources.Post(new FileResource());
			fileResources.Post(new FileResource());
			fileResources.Post(new FileResource());

			// Allow data to flow through the network for several seconds.
			Thread.Sleep(10000);
			cts.Cancel();
			try
			{
				Task.WaitAll(fileMemoryAction.Completion, networkMemoryAction.Completion);
			}
			catch (AggregateException aex)
			{
				// TaskCancelledException is expected and ignored, the rest will propagate.
				aex.Handle(ex => ex is TaskCanceledException);
			}
		}

		private static void FileProcessor(
			Tuple<FileResource, MemoryResource> data,
			BufferBlock<FileResource> fileResources,
			BufferBlock<MemoryResource> memoryResources)
		{
			Console.WriteLine("File worker: using resources...");

			// Simulate a lengthy operation that uses the resources.
			Thread.Sleep(new Random().Next(500, 2000));

			Console.WriteLine("File worker: finished using resources...");

			// Release the resources back to their respective pools.
			fileResources.Post(data.Item1);
			memoryResources.Post(data.Item2);
		}

		private static void NetworkProcessor(
			Tuple<NetworkResource, MemoryResource> data,
			BufferBlock<NetworkResource> networkResources,
			BufferBlock<MemoryResource> memoryResources)
		{
			Console.WriteLine("Network worker: using resources...");

			// Simulate a lengthy operation that uses the resources.
			Thread.Sleep(new Random().Next(500, 2000));

			Console.WriteLine("Network worker: finished using resources...");

			// Release the resources back to their respective pools.
			networkResources.Post(data.Item1);
			memoryResources.Post(data.Item2);
		}
	}
}