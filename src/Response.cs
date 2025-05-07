using System.Runtime.InteropServices;

namespace Kafka;

public class Response
{
    public int MessageSize;
    public Header Header = new();
    public ResponseBody Body = new();

    public int Size()
    {
        return Marshal.SizeOf(MessageSize) + Header.Size() + Body.Size();
    }
}