using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskParallel
{
	internal class TaskWaitAll
	{
		internal static void Run(int nofTasks)
		{
			var tasks = new List<Task>();

			for (var taskNr = 0; taskNr < nofTasks; taskNr++)
			{
				var taskId = taskNr;
				var tsk = Task.Factory.StartNew(() => Console.WriteLine("Running task {0}", taskId));
				tasks.Add(tsk);
			}

			Task.WaitAll(tasks.ToArray());
		}
	}
}