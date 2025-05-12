using System.Runtime.InteropServices;

namespace Kafka;

public class ApiVersionsResponseBody : ResponseBody
{
    public byte NumApiKeys;  //This is the number of currently supported Api Keys. Currently only support api_key=18 aka ApiVersions
    public List<ApiKeyEntry> ApiKeyEntries;
    public int ThrottleTimeMs;
    public byte TagBuffer;

    public ApiVersionsResponseBody()
    {
        ErrorCode = 0;
        ApiKeyEntries = new();
        ApiKeyEntries.AddRange([
            new ApiKeyEntry(ApiKey.ApiVersions.Key, ApiKey.ApiVersions.MinVersion, ApiKey.ApiVersions.MaxVersion, 0),
            new ApiKeyEntry(ApiKey.DescribeTopicPartitions.Key, ApiKey.DescribeTopicPartitions.MinVersion, ApiKey.DescribeTopicPartitions.MaxVersion, 0)
        ]);
        NumApiKeys = (byte)((byte)ApiKeyEntries.Count+0x01);    //For some reason tester treats num_api_keys=2 as 1
        ThrottleTimeMs = 0;
        TagBuffer = 0;
    }

    public override int Size()
    {
        return Marshal.SizeOf(ErrorCode) +
            Marshal.SizeOf(NumApiKeys) +
            ApiKeyEntries.Count * ApiKeyEntries[0].Size() +
            Marshal.SizeOf(ThrottleTimeMs) +
            Marshal.SizeOf(TagBuffer);
    }

    public override byte[] Serialize()
    {
        int sz = Size();
        byte[] buffer = new byte[sz];

        byte[] errorCodeBuffer = Utils.GetBigEndianBytes(ErrorCode);
        byte[] numApiKeysBuffer = Utils.GetBigEndianBytes(NumApiKeys);
        byte[] apiKeyEntriesBuffer = SerializeApiKeyEntries();
        byte[] throttleTimeBuffer = Utils.GetBigEndianBytes(ThrottleTimeMs);
        byte[] tagBufferBuffer = Utils.GetBigEndianBytes(TagBuffer);
       
        int dstOffset = 0;
        Buffer.BlockCopy(errorCodeBuffer, 0, buffer, dstOffset, errorCodeBuffer.Length);
        dstOffset += errorCodeBuffer.Length;
        
        Buffer.BlockCopy(numApiKeysBuffer, 0, buffer, dstOffset, numApiKeysBuffer.Length);
        dstOffset += numApiKeysBuffer.Length;
        
        Buffer.BlockCopy(apiKeyEntriesBuffer, 0, buffer, dstOffset, apiKeyEntriesBuffer.Length);
        dstOffset += apiKeyEntriesBuffer.Length;
        
        Buffer.BlockCopy(throttleTimeBuffer, 0, buffer, dstOffset, throttleTimeBuffer.Length);
        dstOffset += throttleTimeBuffer.Length;
        
        Buffer.BlockCopy(tagBufferBuffer, 0, buffer, dstOffset, tagBufferBuffer.Length);
        dstOffset += tagBufferBuffer.Length;   
        
        return buffer;
    }

    byte[] SerializeApiKeyEntries()
    {
        byte[] buffer = new byte[ApiKeyEntries.Count * ApiKeyEntries[0].Size()];
        int dstOffset = 0;
        foreach (var apiKeyEntry in ApiKeyEntries)
        {
            byte[] apiKeyEntryBuffer = apiKeyEntry.Serialize();
            Buffer.BlockCopy(apiKeyEntryBuffer,0,buffer, dstOffset, apiKeyEntryBuffer.Length);
            dstOffset += apiKeyEntry.Size();
        }
        return buffer;
    }
}