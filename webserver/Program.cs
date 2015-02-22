using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace webserver
{
	class Program
	{
		static void Main(string[] args)
		{
			TcpListener listen = new TcpListener(IPAddress.Any, 1234);
			listen.Start();
			while (true)
			{
				Socket client = listen.AcceptSocket();
				byte[] buffer = new byte[client.ReceiveBufferSize];
				client.Receive(buffer);

				string contents = Encoding.Default.GetString(buffer.ToArray());
				buffer = Encoding.Default.GetBytes(HttpProcessor.GenerateResponse(contents));
				client.Send(buffer);
				client.Disconnect(false);
				client.Close();
			}
		}
	}
}
