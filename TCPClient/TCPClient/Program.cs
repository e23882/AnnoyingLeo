using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPClient
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPClient cli;
            Console.WriteLine("Target IP :");
            var ip = Console.ReadLine();
            while (true)
            {
                Console.WriteLine("Message :");
                var message = Console.ReadLine();
                cli = new TCPClient(ip, 5556, message);
            }
        }
    }
    class TCPClient
    {
        TcpClient client;
        IPEndPoint ipendpoint;
        NetworkStream stream;
        System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        public TCPClient(string ip, int port, string message)
        {
            client = new TcpClient(ip, port);
            ipendpoint = client.Client.RemoteEndPoint as IPEndPoint;
            stream = client.GetStream();
            try
            {
                byte[] messages = Encoding.Default.GetBytes(message);
                stream.Write(messages, 0, messages.Length);
                stream.Close();
                client.Close();
                
            }
            catch (Exception ie)
            {
                stream.Close();
                client.Close();
            }
        }
    }
}
