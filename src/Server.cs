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

        byte[] buffer = new byte[1024]; //1KB buffer
        int bytesRead = await clientSocket.ReceiveAsync(buffer);

        int messageSize = GetMessageSize(buffer), correlationId = GetCorrelationId(buffer);
        
        byte[] messageSizeBytes = GetBigEndianBytes(messageSize), correlationIdBytes = GetBigEndianBytes(correlationId);

        byte[] result = new byte[messageSizeBytes.Length + correlationIdBytes.Length];
        Buffer.BlockCopy(messageSizeBytes, 0, result, 0, messageSizeBytes.Length);
        Buffer.BlockCopy(correlationIdBytes, 0, result, messageSizeBytes.Length, correlationIdBytes.Length);
        
        await clientSocket.SendAsync(result);
    }

    private static int GetMessageSize(byte[] buffer)
    {
        int messageSize = 0;
        for(int i=0; i<4; i++)
            messageSize += buffer[i] << (8*(3-i));
        Console.WriteLine($"Request messageSize: {messageSize}");
        return messageSize;
    }

    static int GetCorrelationId(byte[] buffer)
    {
        int correlationId = 0;
        for (int i = 8; i < 12; i++)
            correlationId += buffer[i] << (8 * (3 - (i - 8)));
        Console.WriteLine($"Request correlationId: {correlationId}");
        return correlationId;
    }

    static byte[] GetBigEndianBytes(int num)
    {
        byte[] bigEndianByteArr = BitConverter.GetBytes(num);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bigEndianByteArr);
        return bigEndianByteArr;
    }
}
