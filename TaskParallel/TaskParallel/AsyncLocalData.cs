using System;
using System.Threading;
using System.Threading.Tasks;


namespace TaskParallel
{
	internal class AsyncLocalData
	{
		private static AsyncLocal<string> mAsyncLocalString = new AsyncLocal<string>();

		private static ThreadLocal<string> mThreadLocalString = new ThreadLocal<string>();


		private static async Task AsyncMethodA()
		{
			// Start multiple async method calls, with different AsyncLocal values.
			// We also set ThreadLocal values, to demonstrate how the two mechanisms differ.
			mAsyncLocalString.Value = "Value 1a";
			mThreadLocalString.Value = "Value 1t";

			var t1 = AsyncMethodB("Value 1");

			mAsyncLocalString.Value = "Value 2a";
			mThreadLocalString.Value = "Value 2t";

			var t2 = AsyncMethodB("Value 2");

			// Await both calls
			await t1;
			await t2;
		}


		private static async Task AsyncMethodB(string expectedValue)
		{
			Console.WriteLine("Entering AsyncMethodB.");
			Console.WriteLine("   Expected '{0}', AsyncLocal value is '{1}', ThreadLocal value is '{2}'",
							  expectedValue, mAsyncLocalString.Value, mThreadLocalString.Value);

			await Task.Delay(100);

			Console.WriteLine("Exiting AsyncMethodB.");
			Console.WriteLine("   Expected '{0}', got '{1}', ThreadLocal value is '{2}'",
							  expectedValue, mAsyncLocalString.Value, mThreadLocalString.Value);
		}

		public static void Run()
		{
			AsyncMethodA().Wait();
		}
	}
}
