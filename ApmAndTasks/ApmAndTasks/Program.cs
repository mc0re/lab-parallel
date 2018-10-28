
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApmAndTasks
{
	class Program
	{
		static void Main(string[] args)
		{
			var req = WebRequest.Create("https://www.whatismyip.com") as HttpWebRequest;
			req.Method = "GET";
			req.UserAgent = "Edge";

			try
			{
				var t = Task.Factory
					.FromAsync(req.BeginGetResponse, req.EndGetResponse, null)
					.ContinueWith(getRespTask => ReadHtmlData(getRespTask.Result as HttpWebResponse))
					.ContinueWith(htmlTask => ProcessIpResult(htmlTask.Result))
					.ContinueWith(ipTask =>
						Console.WriteLine("{0} {1}", DateTime.Now.TimeOfDay.TotalMilliseconds, ipTask.Result));

				Console.WriteLine("{0} Started download", DateTime.Now.TimeOfDay.TotalMilliseconds);

				t.Wait();
			}
			catch (AggregateException aex)
			{
				foreach (var ex in aex.Flatten().InnerExceptions)
					Console.WriteLine(ex.Message);
			}
		}


		private static string ReadHtmlData(HttpWebResponse resp)
		{
			Console.WriteLine("{0} Received response", DateTime.Now.TimeOfDay.TotalMilliseconds);

			using (var htmlStream = new StreamReader(resp.GetResponseStream()))
			{
				Console.WriteLine("{0} Received stream", DateTime.Now.TimeOfDay.TotalMilliseconds);

				var str = htmlStream.ReadToEnd();

				Console.WriteLine("{0} Read stream", DateTime.Now.TimeOfDay.TotalMilliseconds);

				return str;
			}
		}


		private static string ProcessIpResult(string html)
		{
			Console.WriteLine("{0} Processing HTML", DateTime.Now.TimeOfDay.TotalMilliseconds);

			var ip4re = new Regex(@"Your Public IPv4 is: ([^<]+)");
			var m = ip4re.Match(html);
			var group = m.Groups[1];
			if (group.Success)
			{
				return group.Value;
			}

			var limitRe = new Regex("Your lookup limit has been reached");
			if (limitRe.Match(html).Success)
				return "Too many attempts during this day";

			return "IP not found";
		}
	}
}
