using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;

namespace webserver
{
	public enum HttpRequestType
	{ Get, Post, Unknown }
	class HttpProcessor
	{
		public static HttpRequestType GetRequestType(string request)
		{
			if(request.StartsWith("GET "))
				return HttpRequestType.Get;
			if(request.StartsWith("POST "))
				return HttpRequestType.Post;
			return HttpRequestType.Unknown;
		}

		public static string GetPath(string request)
		{
			string[] split = request.Split(' ');
			if (split.Length < 2)
				throw new ArgumentException("The request was malformed");
			if (split[1] == "/")
				return "index.html";
			return split[1].TrimStart('/').Replace('/','\\');
		}

		public static string GetMimeType(string file)
		{
			string[] dots = file.Split('.');
			string extension = "";

			if (dots.Length > 1)
				extension = dots[dots.Length - 1];
			switch (extension)
			{
				case "html":
					return "text/html";
				case "png":
					return "image/png";
				default:
					return "application/octet-stream";
			}
		}

		public static string GetStatus(int statuscode)
		{
			switch (statuscode)
			{
				case 200:
					return "OK";
				case 404:
					return "Not Found";
				case 500:
					return "Internal Server Error";
				default:
					throw new ArgumentOutOfRangeException("Unknown Statuscode supplied");
			}
		}

		public static string BuildHeader(int statuscode, params string[] options)
		{
			string header = "HTTP/1.1 ";
			header += statuscode;
			try
			{
				header += GetStatus(statuscode);
			}
			catch (ArgumentException)
			{
				return "HTTP/1.1 500 Internal Server Error";
			}
			if (options.Length == 0) header += "\r\n";
			options.Aggregate(header, (current, s) => current + ("\r\n"+s));
			return header + "\r\n\r\n";
		}

		public static string GenerateResponse(string request)
		{
			switch (GetRequestType(request))
			{
				case HttpRequestType.Get:
					string path = GetPath(request);
					if (File.Exists(path))
					{
						Console.WriteLine("sending " + path);
						string mime = GetMimeType(path);
						string header = BuildHeader(200, "Content-Type: " + mime, "Content-Transfer-Encoding: binary");
						return header + "\r\n\r\n" + Encoding.Default.GetString(File.ReadAllBytes(path)) + "\r\n";
					}
					return "HTTP/1.1 404 Not Found\r\n\r\n\r\n";
				case HttpRequestType.Post:
					return "HTTP/1.1 501 Not Implemented\r\n\r\n";
				default:
					return "HTTP/1.1 500 Internal Server Error";
			}
		}
	}
}
