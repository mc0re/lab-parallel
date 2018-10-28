using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskParallel
{
	internal class AttachedTask
	{
		internal static void Run(int nofChildren)
		{
			var parent = Task.Factory.StartNew(() =>
			{
				Console.WriteLine("Parent task beginning.");

				for (int ctr = 0; ctr < nofChildren; ctr++)
				{
					Task.Factory.StartNew((x) =>
						{
							Thread.SpinWait(5000000);
							Console.WriteLine("Attached child #{0} completed.", x);
						},
						ctr,
						TaskCreationOptions.AttachedToParent);
				}
			});

			parent.Wait();
			Console.WriteLine("Parent task completed.");
		}
	}
}