using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;


namespace TaskDataFlow
{
	internal class FileReadChainLab
	{
		internal static void Run()
		{
			var tempFile = CreateRandomFile();

			// Create an ActionBlock<int> object that prints to the console 
			// the number of bytes read.
			var printResult = new ActionBlock<int>(zeroBytesRead =>
			{
				Console.WriteLine("{0} contains {1} zero bytes.", Path.GetFileName(tempFile), zeroBytesRead);
			});

			// Create a TransformBlock<string, int> object that calls the 
			// CountBytes function and returns its result.
			var countBytes = new TransformBlock<string, int>(new Func<string, Task<int>>(CountBytes));

			// Link the TransformBlock<string, int> object to the ActionBlock<int> object.
			countBytes.LinkTo(printResult);
			countBytes.Completion.ContinueWith(delegate { printResult.Complete(); });

			// Post the path to the temporary file to the 
			// TransformBlock<string, int> object.
			countBytes.Post(tempFile);

			// Requests completion of the TransformBlock<string, int> object.
			countBytes.Complete();

			// Wait for the ActionBlock<int> object to print the message.
			printResult.Completion.Wait();

			// Delete the temporary file.
			File.Delete(tempFile);
		}


		private static string CreateRandomFile()
		{
			// Create a temporary file on disk.
			string tempFile = Path.GetTempFileName();

			// Write random data to the temporary file.
			using (var fileStream = File.OpenWrite(tempFile))
			{
				var rand = new Random();
				byte[] buffer = new byte[1024];
				for (int i = 0; i < 512; i++)
				{
					rand.NextBytes(buffer);
					fileStream.Write(buffer, 0, buffer.Length);
				}
			}

			return tempFile;
		}


		// Computes the number of zero bytes that the provided file
		// contains.
		private static async Task<int> CountBytes(string path)
		{
			byte[] buffer = new byte[1024];
			int totalZeroBytesRead = 0;
			using (var fileStream = File.OpenRead(path))
			{
				int bytesRead = 0;
				do
				{
					bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
					totalZeroBytesRead += buffer.Count(b => b == 0);
				} while (bytesRead > 0);
			}

			return totalZeroBytesRead;
		}
	}
}