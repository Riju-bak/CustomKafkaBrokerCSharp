using System.Net;
using System.Net.Sockets;

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

        byte[] buffer = new byte[1024];
        int bytesRead = await clientSocket.ReceiveAsync(buffer);

        int messageSize = 0, correlationId = 7;

        byte[] messageSizeBytes = GetBigEndianBytes(messageSize), correlationIdBytes = GetBigEndianBytes(correlationId);

        byte[] result = new byte[messageSizeBytes.Length + correlationIdBytes.Length];
        Buffer.BlockCopy(messageSizeBytes, 0, result, 0, messageSizeBytes.Length);
        Buffer.BlockCopy(correlationIdBytes, 0, result, messageSizeBytes.Length, correlationIdBytes.Length);
        
        await clientSocket.SendAsync(result);
    }

    static byte[] GetBigEndianBytes(int num)
    {
        byte[] bigEndianByteArr = BitConverter.GetBytes(num);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bigEndianByteArr);
        return bigEndianByteArr;
    }
}
