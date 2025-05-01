using System.Net;
using System.Net.Sockets;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Logs from your program will appear here!");
        
        TcpListener server = new TcpListener(IPAddress.Any, 9092);
        server.Start();
        server.AcceptSocket(); // wait for client
    }
}
