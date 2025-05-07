namespace Kafka;

public static class Utils
{
    public static Response Deserialize(byte[] buffer)
    {
        int correlationId = ParseGetCorrelationId(buffer);
        short requestApiKey = ParseApiKey(buffer);
        int requestApiVersion = ParseApiVersion(buffer);
       
        Response response = new(); 
        
        response.Header.CorrelationId = correlationId;
        if (requestApiKey == ApiKey.ApiVersions.Key)
            if (requestApiVersion >= ApiKey.ApiVersions.MinVersion && requestApiVersion <= ApiKey.ApiVersions.MaxVersion)
                response.Body = new ApiVersionsResponseBody();

        response.MessageSize = response.Header.Size() + response.Body.Size();
        return response;
    }

    public static byte[] Serialize(Response response)
    {
        int responseSize = response.Size(); //Size of response in B
        byte[] buffer = new byte[responseSize];
        byte[] messageSizeBuffer = GetBigEndianBytes(response.MessageSize);
        byte[] headerBuffer = response.Header.Serialize();
        byte[] bodyBuffer = response.Body.Serialize();

        int dstOffset = 0;
        Buffer.BlockCopy(messageSizeBuffer, 0, buffer, dstOffset, messageSizeBuffer.Length);
        dstOffset += messageSizeBuffer.Length;

        Buffer.BlockCopy(headerBuffer, 0, buffer, dstOffset, headerBuffer.Length);
        dstOffset += headerBuffer.Length;

        Buffer.BlockCopy(bodyBuffer, 0, buffer, dstOffset, bodyBuffer.Length);
        dstOffset += bodyBuffer.Length;

        return buffer;
    }
    
    private static short ParseApiKey(byte[] reqBuffer)
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

    static int ParseGetCorrelationId(byte[] buffer)
    {
        int correlationId = 0;
        for (int i = 8; i < 12; i++)
            correlationId += buffer[i] << (8 * (3 - (i - 8)));
        Console.WriteLine($"Request correlationId: {correlationId}");
        return correlationId;
    }

    public static byte[] GetBigEndianBytes<T>(T num)
    {
        byte[] bigEndianByteArr = null!;
        if (num is byte)
        {
            bigEndianByteArr = new byte[1];
            bigEndianByteArr[0] = (byte)(object)num;
        }
        if (num is Int16)
            bigEndianByteArr = BitConverter.GetBytes((Int16)(object)num);
        if(num is Int32)
            bigEndianByteArr = BitConverter.GetBytes((Int32)(object)num);
        if(BitConverter.IsLittleEndian)
            Array.Reverse(bigEndianByteArr!);
        return bigEndianByteArr!;
    }

}