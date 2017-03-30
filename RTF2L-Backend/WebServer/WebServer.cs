using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
/* 
 * Made by Francis Bio
 * Edited by Mister 4 Eyes
 */

namespace RandomTF2Loadout.WebServer
{
	public class WebServer
	{
		private readonly HttpListener _listener = new HttpListener();
		private readonly Func<HttpListenerContext, byte[]> _responderMethod;
		bool devMode;

		public WebServer(string[] prefixes, Func<HttpListenerContext, byte[]> method, bool devmode)
		{
			devMode = devmode;

			if (!HttpListener.IsSupported)
				throw new NotSupportedException(
					"Needs Windows XP SP2, Server 2003 or later.");

			// URI prefixes are required, for example 
			// "http://localhost:8080/index/".
			if (prefixes == null || prefixes.Length == 0)
				throw new ArgumentException("prefixes");

			// A responder method is required
			if (method == null)
				throw new ArgumentException("method");

			//Checks if the pattern is a URI
			//Group 1: Checks for "http://" or "https://"
			//Group 2: The URI-to-be
			//Group 3: Checks if it's terminated with a "/"
			string pattern = @"(https?:\/\/)?([0-9a-zA-Z$-_.+!*'(),]+)(\/)?";
			foreach (string s in prefixes)
			{
				MatchCollection mc = Regex.Matches(s, pattern);

				string uri;
				//THere is nothing to recover if there are more than 1 match which this thing can do.
				if(mc.Count == 1)
				{
					Match match = mc[0];
					if(!(match.Groups[1].Success && match.Groups[3].Success))
					{
						uri = string.Format("{0}{1}/", (match.Groups[1].Success) ? match.Groups[1].Value : "http://", match.Groups[2].Value);
					}
					else
					{
						uri = s;
					}

					Uri uriResult;//We do nothing with this
					if(Uri.TryCreate(uri, UriKind.Absolute, out uriResult))
					{
						_listener.Prefixes.Add(uri);
					}
				}
			}

			_responderMethod = method;
			_listener.Start();
		}

		public WebServer(Func<HttpListenerContext, byte[]> method, params string[] prefixes)
			: this(prefixes, method, false) { }

		public void Run()
		{
			ThreadPool.QueueUserWorkItem((o) =>
			{
				Console.WriteLine("Webserver running...");
				try
				{
					while (_listener.IsListening)
					{
						ThreadPool.QueueUserWorkItem((c) =>
						{
							Console.WriteLine("Request!");
							var ctx = c as HttpListenerContext;
							string errorMessage = "";
							try
							{
								byte[] buf = _responderMethod(ctx);
								ctx.Response.ContentLength64 = buf.Length;
								ctx.Response.OutputStream.Write(buf, 0, buf.Length);
							}
							catch (Exception e) { Console.WriteLine(e); errorMessage = e.ToString(); } // suppress any exceptions
							finally
							{
								if (devMode)
								{
									// Sends the stack trace to the browser
									ctx.Response.StatusCode = 500;
									string none = string.Format("<head><title>500</title></head><body>{0}</body>", errorMessage);
									byte[] buffer = Encoding.UTF8.GetBytes(none);
									ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
								}
								ctx.Response.OutputStream.Close();
							}
						}, _listener.GetContext());
					}
				}
				catch { } // suppress any exceptions
			});
		}

		public void Stop()
		{
			_listener.Stop();
			_listener.Close();
		}
	}
}
