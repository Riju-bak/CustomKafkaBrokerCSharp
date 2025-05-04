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

        byte[] buffer = new byte[1024]; //1KB buffer
        int bytesRead = await clientSocket.ReceiveAsync(buffer);

        int correlationId = GetCorrelationId(buffer);
        
        byte[] requestApiKeyBytes = GetBigEndianBytes((Int16)ParseApiKey(buffer));
        byte[] apiMinVersionBytes = GetBigEndianBytes((Int16)0);
        byte[] apiMaxVersionBytes = GetBigEndianBytes((Int16)4);

        int requestApiVersion = ParseApiVersion(buffer);
        byte[] correlationIdBytes = GetBigEndianBytes(correlationId);
       
        Int16 errorCode = 0;
        if (requestApiVersion < 0 || requestApiVersion > 4)
            errorCode = 35; //UNSUPPORTED_VERSION
        
        byte[] errorCodeBytes = GetBigEndianBytes(errorCode);
        
        int responseHeaderSize = correlationIdBytes.Length; //32 bits (4 B)

        int numberOfSupportedApiKeys = 1; //Currently only support api_key=18 aka ApiVersions
        int responseBodySize = 2 //error_code (2B)
                               + 1 //num_api_keys (1B) num_api_keys = 1 for now. We only support api_key 18
                               + (
                                   2 //api_key          (2B)
                                   + 2 //min_version    (2B)
                                   + 2 //max_version    (2B)
                                   + 1 //tagged_field/TAG_BUFFER (1B)
                               ) * numberOfSupportedApiKeys
                               + 4 //throttle_time_ms (4B)
                               + 1; //tagged_field/TAG_BUFFER     (1B)

        int responseMessageSize = responseHeaderSize + responseBodySize;  //4B + Body Size in B 
        byte[] responseMessageSizeBytes = GetBigEndianBytes(responseMessageSize);

        int responseTotalSize = 4 + responseHeaderSize + responseBodySize;
        
    //TODO: Improve this whole block using structs/classes. 
        byte[] response = new byte[responseTotalSize];
        
        int dstOffset = 0;
        //Message Size
        Buffer.BlockCopy(responseMessageSizeBytes, 0, response, 0, responseMessageSizeBytes.Length);
        dstOffset += responseMessageSizeBytes.Length;
        
        //Header
        Buffer.BlockCopy(correlationIdBytes, 0, response, dstOffset, correlationIdBytes.Length);
        dstOffset += correlationIdBytes.Length;
        
        // ----  Body  -----
        Buffer.BlockCopy(errorCodeBytes, 0, response, dstOffset, errorCodeBytes.Length);
        dstOffset += errorCodeBytes.Length;
            
            //num_api_keys
            response[dstOffset] = 0x02; //codecrafters tester is buggy, it decodes 2 as 1 
            dstOffset += 1;

            //api_key
            byte[] responseApiKeyBytes = requestApiKeyBytes;
            Buffer.BlockCopy(responseApiKeyBytes, 0, response, dstOffset, responseApiKeyBytes.Length);
            dstOffset += responseApiKeyBytes.Length;
            
            //min_version
            Buffer.BlockCopy(apiMinVersionBytes, 0, response, dstOffset, apiMinVersionBytes.Length);
            dstOffset += apiMinVersionBytes.Length;
            
            //max_version
            Buffer.BlockCopy(apiMaxVersionBytes, 0, response, dstOffset, apiMaxVersionBytes.Length);
            dstOffset += apiMaxVersionBytes.Length;
            
            // tag_buffer TODO: Figure out what the hell this is!!
             response[dstOffset] = 0x00;
             dstOffset++;

            //throttle_time_ms
            for (int i = dstOffset; i < dstOffset + 4; i++)
                response[i] = 0x00;
            dstOffset += 4;

            //tag_buffer
            response[dstOffset] = 0x00;
            dstOffset++;
        // --Body end ---

    //  TODO: Block to improve end /////////////////
     
            await clientSocket.SendAsync(response);
    }

    private static Int16 ParseApiKey(byte[] reqBuffer)
    {
        int apikeyInt32 = 0;
        for(int i=4; i<6; i++)
            apikeyInt32 += reqBuffer[i] << (8 * (1 - (i-4)));
        Int16 apiKey = (Int16)apikeyInt32;
        Console.WriteLine($"Request api-key: {apiKey}");
        return apiKey;
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
