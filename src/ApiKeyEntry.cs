using System.Runtime.InteropServices;

namespace Kafka;

public class ApiKeyEntry
{
    public short ApiKey;
    public short MinVersion;
    public short MaxVersion;
    public byte TagBuffer;

    public ApiKeyEntry(short apiKey, short minVersion, short maxVersion, byte tagBuffer)
    {
        ApiKey = apiKey;
        MinVersion = minVersion;
        MaxVersion = maxVersion;
        TagBuffer = tagBuffer;
    }

    public int Size()
    {
        return Marshal.SizeOf(ApiKey) + Marshal.SizeOf(MinVersion) + Marshal.SizeOf(MaxVersion) +
               Marshal.SizeOf(TagBuffer);
    }

    public byte[] Serialize()
    {
        byte[] buffer = new byte[Size()];
        byte[] apiKeyBuffer = Utils.GetBigEndianBytes(ApiKey);
        byte[] minVersionBuffer = Utils.GetBigEndianBytes(MinVersion);
        byte[] maxVersionBuffer = Utils.GetBigEndianBytes(MaxVersion);
        byte[] tagBufferBuffer = Utils.GetBigEndianBytes(TagBuffer);

        int dstOffset = 0;
        Buffer.BlockCopy(apiKeyBuffer, 0, buffer, dstOffset, apiKeyBuffer.Length);
        dstOffset += apiKeyBuffer.Length;
        
        Buffer.BlockCopy(minVersionBuffer, 0, buffer, dstOffset, minVersionBuffer.Length);
        dstOffset += minVersionBuffer.Length;
        
        Buffer.BlockCopy(maxVersionBuffer, 0, buffer, dstOffset, maxVersionBuffer.Length);
        dstOffset += maxVersionBuffer.Length;

        Buffer.BlockCopy(tagBufferBuffer, 0, buffer, dstOffset, tagBufferBuffer.Length);
        dstOffset += tagBufferBuffer.Length;

        return buffer;
    }
 }