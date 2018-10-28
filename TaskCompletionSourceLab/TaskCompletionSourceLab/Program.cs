using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace TaskCompletionSourceLab
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var urlList = new[]
			{
				"http://www.gutenberg.org/cache/epub/10/pg10.txt",
				"http://www.gutenberg.org/cache/epub/28346/pg28346.txt",
				"http://www.pluralsight.com",
				"http://www.bbc.com",
				"http://www.msnbc.com",
				"http://www.yahoo.com",
				"http://www.nytimes.com",
				"http://www.washingtonpost.com",
				"http://www.latimes.com",
				"http://www.newsday.com"
			};

			var cts = new CancellationTokenSource();

			Console.WriteLine("Starting: {0}", DateTime.Now);

			// Create a UI thread from which to cancel the entire operation
			Task.Factory.StartNew(() =>
			{
				Console.WriteLine("Press c to cancel");
				if (Console.ReadKey().KeyChar == 'c')
					cts.Cancel();
			});

			try
			{
				var loader = new SimpleWebExample().GetWordCountsSimplified(urlList, "the", cts.Token);
				var counters = loader.Result;

				foreach (var cnt in counters)
				{
					Console.WriteLine(cnt);
				}
			}
			catch (AggregateException aex)
			{
				foreach (var ex in aex.Flatten().InnerExceptions)
				{
					Console.WriteLine($"Exception: {ex.Message}");
				}
			}

			Console.WriteLine("Finished: {0}", DateTime.Now);
			Console.ReadKey();
		}
	}


	public class SimpleWebExample
	{
		public Task<string[]> GetWordCountsSimplified(
			string[] urls, string name, CancellationToken token)
		{
			var tcs = new TaskCompletionSource<string[]>();
			var webClients = new WebClient[urls.Length];
			var m_lock = new object();
			var count = 0;
			var results = new List<string>();

			// If the user cancels the CancellationToken, then we can use the
			// WebClient's ability to cancel its own async operations.
			token.Register(() =>
			{
				foreach (var wc in webClients)
				{
					if (wc != null)
						wc.CancelAsync();
				}
			});


			for (int i = 0; i < urls.Length; i++)
			{
				webClients[i] = new WebClient();

				#region Callback

				// Specify the callback for the DownloadStringCompleted
				// event that will be raised by this WebClient instance.
				webClients[i].DownloadStringCompleted += (obj, args) =>
				{
					if (args.Cancelled)
					{
						tcs.TrySetCanceled();
						return;
					}

					if (args.Error != null)
					{
						tcs.TrySetException(args.Error);
						return;
					}

					// Split the string into an array of words,
					// then count the number of elements that match
					// the search term.
					var words = args.Result.Split(' ');
					var NAME = name.ToUpper();
					var nameCount = (from word in words.AsParallel().WithCancellation(token)
									 where word.ToUpper().Contains(NAME)
									 select word)
									.Count();

					// Associate the results with the url, and add new string to the array that
					// the underlying Task object will return in its Result property.
					lock (m_lock)
					{
						results.Add(string.Format("{0} has {1} instances of '{2}'", args.UserState, nameCount, name));

						// If this is the last async operation to complete,
						// then set the Result property on the underlying Task.
						count++;
						if (count == urls.Length)
						{
							tcs.TrySetResult(results.ToArray());
						}
					}
				};

				#endregion

				// Call DownloadStringAsync for each URL.
				Uri address = null;
				try
				{
					address = new Uri(urls[i]);
					// Pass the address, and also use it for the userToken 
					// to identify the page when the delegate is invoked.
					webClients[i].DownloadStringAsync(address, address);
				}

				catch (UriFormatException ex)
				{
					// Abandon the entire operation if one url is malformed.
					// Other actions are possible here.
					tcs.TrySetException(ex);
					return tcs.Task;
				}
			}

			// Return the underlying Task. The client code
			// waits on the Result property, and handles exceptions
			// in the try-catch block there.
			return tcs.Task;
		}
	}
}
