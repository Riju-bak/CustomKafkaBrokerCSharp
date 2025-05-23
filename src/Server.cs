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
        
        while (true)
        {
            Socket clientSocket = await server.AcceptSocketAsync(); // wait for client

            //Offload client connection to a thread from thread-pool enabling concurrency
            _ = Task.Run(() => HandleClientAsync(clientSocket));
        }
    }

    static async Task HandleClientAsync(Socket clientSocket)
    {
        while (clientSocket.Connected)
        {
            byte[] requestBuffer = new byte[1024]; //1KB buffer
            int bytesRead = await clientSocket.ReceiveAsync(requestBuffer);

            if (bytesRead == 0) clientSocket.Close();

            Response response = Utils.Deserialize(requestBuffer);

            //TCP is a byte stream protocol. Essentially we send an array of bytes that TCP divides into packets.
            byte[] responseBuffer = Utils.Serialize(response);
        
            await clientSocket.SendAsync(responseBuffer);
        }
    }
    
}
