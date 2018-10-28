using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskParallel
{
	internal class DefaultTaskFactory
	{
		internal static void Run(int nofTasks)
		{
			Task[] taskArray = new Task[nofTasks];

			for (int i = 0; i < taskArray.Length; i++)
			{
				taskArray[i] = Task.Factory.StartNew((object obj) =>
					{
						if (!(obj is ThreadData data)) throw new ArgumentNullException("AsyncState");

						data.ThreadNum = Thread.CurrentThread.ManagedThreadId;
						data.ExecutionTime = DateTime.Now.Ticks;
					},
					new ThreadData() { Number = i, CreationTime = DateTime.Now.Ticks });
			}

			Task.WaitAll(taskArray);

			foreach (var task in taskArray)
			{
				if (!(task.AsyncState is ThreadData data)) throw new ArgumentNullException("AsyncState");

				Console.WriteLine("Task #{0} created at {1}, ran on thread #{2} at {3}.",
								  data.Number, data.CreationTime, data.ThreadNum, data.ExecutionTime);
			}
		}
	}
}