using System;
using System.Net;
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

		public WebServer(string[] prefixes, Func<HttpListenerContext, byte[]> method)
		{
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

			foreach (string s in prefixes)
				_listener.Prefixes.Add(s);

			_responderMethod = method;
			_listener.Start();
		}

		public WebServer(Func<HttpListenerContext, byte[]> method, params string[] prefixes)
			: this(prefixes, method) { }

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
							try
							{
								byte[] buf = _responderMethod(ctx);
								ctx.Response.ContentLength64 = buf.Length;
								ctx.Response.OutputStream.Write(buf, 0, buf.Length);
							}
							catch { } // suppress any exceptions
							finally
							{
								// always close the stream
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
