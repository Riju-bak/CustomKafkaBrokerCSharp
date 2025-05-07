using System.Runtime.InteropServices;

namespace Kafka;


public class Header
{
    public int CorrelationId;

    public int Size()
    {
        //returns the size of the header in bytes (B)
        return Marshal.SizeOf(CorrelationId);
    }

    public byte[] Serialize()
    {
        byte[] buffer = Utils.GetBigEndianBytes(CorrelationId);
        return buffer;
    }
}