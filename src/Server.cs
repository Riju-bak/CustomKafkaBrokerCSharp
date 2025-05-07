using System.Net;
using System.Net.Sockets;

namespace Kafka;

public class Server
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Logs from your program will appear here!");

        int port = 9092;
        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();
        Console.WriteLine($"Started server on port {port} ...");
        
        Socket clientSocket = await server.AcceptSocketAsync(); // wait for client

        byte[] requestBuffer = new byte[1024]; //1KB buffer
        int bytesRead = await clientSocket.ReceiveAsync(requestBuffer);

        Response response = Utils.Deserialize(requestBuffer);

        byte[] responseBuffer = Utils.Serialize(response);
        
        await clientSocket.SendAsync(responseBuffer);
    }

}
