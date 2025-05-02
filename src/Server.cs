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
        int requestApiVersion = ParseApiVersion(buffer);
        
        byte[] messageSizeBytes = GetBigEndianBytes(messageSize), correlationIdBytes = GetBigEndianBytes(correlationId);

        Int16 errorCode = 35;
        byte[] errorCodeBytes = GetBigEndianBytes(errorCode);

        byte[] result = new byte[messageSizeBytes.Length + correlationIdBytes.Length + errorCodeBytes.Length];
        Buffer.BlockCopy(messageSizeBytes, 0, result, 0, messageSizeBytes.Length);
        Buffer.BlockCopy(correlationIdBytes, 0, result, messageSizeBytes.Length, correlationIdBytes.Length);
        Buffer.BlockCopy(errorCodeBytes, 0, result, messageSizeBytes.Length + correlationIdBytes.Length, errorCodeBytes.Length);
        
        await clientSocket.SendAsync(result);
    }

    static int ParseApiVersion(byte[] reqBuffer)
    {
        int apiVersion = 0;
        for (int i = 6; i < 8; i++)
            apiVersion += reqBuffer[i] << (8 * (1 - (i-6)));
        Console.WriteLine($"Request apiVersion: {apiVersion}");
        return apiVersion;
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

    static byte[] GetBigEndianBytes<T>(T num)
    {
        byte[] bigEndianByteArr = null!;
        if (num is Int16)
            bigEndianByteArr = BitConverter.GetBytes((Int16)(object)num);
        if(num is Int32)
            bigEndianByteArr = BitConverter.GetBytes((Int32)(object)num);
        if(BitConverter.IsLittleEndian)
            Array.Reverse(bigEndianByteArr!);
        return bigEndianByteArr!;
    }
}
