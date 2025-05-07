using System.Runtime.InteropServices;

namespace Kafka;

public class ResponseBody
{
    public short ErrorCode = 35; //default is error_code 35 UNSUPPORTED_VERSION

    public virtual int Size()
    {
        return Marshal.SizeOf(ErrorCode);
    }

    public virtual byte[] Serialize()
    {
        return Utils.GetBigEndianBytes(ErrorCode);
    }
}