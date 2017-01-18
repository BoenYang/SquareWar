using System;
using System.Collections.Generic;
using System.Net.Sockets;

/// <summary>
/// 优化
///     1.支持插入操作
///     2.优化性能,如读写指针比较靠后则可以通过移动数据位置来腾出空间,而不是重新扩容,可以参考java
/// 
/// </summary>

public class ByteBuffer
{
    public static bool IsLittleEndian = true;
    //字节缓存区
    private byte[] buf;
    //读取索引
    private int readIndex = 0;
    //写入索引
    private int writeIndex = 0;
    //读取索引标记
    private int markReadIndex = 0;
    //写入索引标记
    private int markWirteIndex = 0;
    //缓存区字节数组的长度
    private int capacity;

    /**
        * 构造方法
        */
    private ByteBuffer(int capacity)
    {
        buf = new byte[capacity];
        this.capacity = capacity;
    }

    /**
        * 构造方法
        */
    private ByteBuffer(byte[] bytes)
    {
        buf = bytes;
        this.capacity = bytes.Length;
    }


    /**
        * 构建一个capacity长度的字节缓存区ByteBuffer对象
        */
    public static ByteBuffer Allocate(int capacity)
    {
        return new ByteBuffer(capacity);
    }

    /**
        * 构建一个以bytes为字节缓存区的ByteBuffer对象，一般不推荐使用
        */
    public static ByteBuffer Allocate(byte[] bytes)
    {
        return new ByteBuffer(bytes);
    }

    /// <summary>
    /// 接收数据
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    public int Recive(Socket socket, int length)
    {
        if (socket.Available < length)
            return -1;

        Skip(length);
        lock (this)
        {
            int index = writeIndex - length;//Skip已经设置过writeIndex
            int result = socket.Receive(buf, index, length, SocketFlags.None);
            return result;
        }
    }

    /// <summary>
    /// 根据length长度，确定大于此leng的最近的2次方数，如length=7，则返回值为8
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    private int FixLength(int length)
    {
        int n = 2;
        int b = 2;
        while (b < length)
        {
            b = 2 << n;
            n++;
        }
        return b;
    }

    /// <summary>
    /// 翻转字节数组，如果本地字节序列为低字节序列，则进行翻转以转换为高字节序列
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    private byte[] flip(byte[] bytes)
    {
        if (IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    /// <summary>
    /// 确定内部字节缓存数组的大小
    /// </summary>
    /// <param name="currLen"></param>
    /// <param name="futureLen"></param>
    /// <returns></returns>
    private int FixSizeAndReset(int currLen, int futureLen)
    {
        if (futureLen > currLen)
        {
            //以原大小的2次方数的两倍确定内部字节缓存区大小
            int size = FixLength(currLen) * 2;
            if (futureLen > size)
            {
                //以将来的大小的2次方的两倍确定内部字节缓存区大小
                size = FixLength(futureLen) * 2;
            }
            byte[] newbuf = new byte[size];
            Array.Copy(buf, 0, newbuf, 0, currLen);
            buf = newbuf;
            capacity = newbuf.Length;
        }
        return futureLen;
    }

    /// <summary>
    /// 跳过
    /// </summary>
    /// <param name="count"></param>
    public void Skip(int count)
    {
        lock (this)
        {
            int total = count + writeIndex;
            FixSizeAndReset(buf.Length, total);
            writeIndex = total;
        }
    }

    /// <summary>
    /// 将一个ByteBuffer的有效字节区写入此缓存区中
    /// </summary>
    /// <param name="buffer"></param>
    public void Write(ByteBuffer buffer)
    {
        if (buffer == null) return;
        if (buffer.ReadableBytes() <= 0) return;
        //WriteBytes(buffer.ToArray());
        WriteBytes(buffer.buf, buffer.readIndex, buffer.ReadableBytes());
        buffer.Clear();
    }

    /// <summary>
    /// 将bytes字节数组从startIndex开始的length字节写入到此缓存区
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="startIndex"></param>
    /// <param name="length"></param>
    public void WriteBytes(byte[] bytes, int startIndex, int length)
    {
        lock (this)
        {
            int offset = length - startIndex;
            if (offset <= 0) return;
            int total = offset + writeIndex;
            int len = buf.Length;
            FixSizeAndReset(len, total);
            for (int i = writeIndex, j = startIndex; i < total; i++, j++)
            {
                buf[i] = bytes[j];
            }
            writeIndex = total;
        }
    }

    /// <summary>
    /// 将字节数组中从0到length的元素写入缓存区
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="length"></param>
    public void WriteBytes(byte[] bytes, int length)
    {
        WriteBytes(bytes, 0, length);
    }

    /// <summary>
    /// 将字节数组全部写入缓存区
    /// </summary>
    /// <param name="bytes"></param>
    public void WriteBytes(byte[] bytes)
    {
        WriteBytes(bytes, bytes.Length);
    }

    public void WriteShort(short value)
    {
        WriteBytes(flip(BitConverter.GetBytes(value)));
    }
        
    public void WriteUshort(ushort value)
    {
        WriteBytes(flip(BitConverter.GetBytes(value)));
    }
        
    public void WriteInt(int value)
    {
        WriteBytes(flip(BitConverter.GetBytes(value)));
    }
        
    public void WriteUint(uint value)
    {
        WriteBytes(flip(BitConverter.GetBytes(value)));
    }
        
    public void WriteLong(long value)
    {
        WriteBytes(flip(BitConverter.GetBytes(value)));
    }
        
    public void WriteUlong(ulong value)
    {
        WriteBytes(flip(BitConverter.GetBytes(value)));
    }
        
    public void WriteFloat(float value)
    {
        WriteBytes(flip(BitConverter.GetBytes(value)));
    }
        
    public void WriteByte(byte value)
    {
        lock (this)
        {
            int afterLen = writeIndex + 1;
            int len = buf.Length;
            FixSizeAndReset(len, afterLen);
            buf[writeIndex] = value;
            writeIndex = afterLen;
        }
    }

    public void WriteBool(bool value)
    {
        byte b = (byte)(value ? 1 : 0);
        WriteByte(b);
    }

    public void WriteChar(char c)
    {
        WriteShort((short)c);
    }

    public void WriteDouble(double value)
    {
        WriteBytes(flip(BitConverter.GetBytes(value)));
    }

    /// <summary>
    /// 先写入字符串占用字节长度,然后写入字符串
    /// </summary>
    /// <param name="str"></param>
    public void WriteUTF8OfLen(string str)
    {
        WriteString(str, System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// 直接写入字符串
    /// </summary>
    /// <param name="str"></param>
    public void WriteUTF8(string str)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
        WriteBytes(bytes);
    }
        
    /// <summary>
    /// 写入字符串,不支持null
    /// </summary>
    /// <param name="str"></param>
    /// <param name="encoding"></param>
    public void WriteString(string str, System.Text.Encoding encoding)
    {
        byte[] bytes = encoding.GetBytes(str);
        WriteUshort((ushort)bytes.Length);
        WriteBytes(bytes);
        //WriteBytes(flip(bytes));//跟java通讯字符串会倒序,怀疑字符串处理方式跟其它数据类型不一致
    }

    /// <summary>
    /// 读取一个字节
    /// </summary>
    /// <returns></returns>
    public byte ReadByte()
    {
        byte b = buf[readIndex];
        readIndex++;
        return b;
    }

    public bool ReadBool()
    {
        byte b = ReadByte();
        return b == 1 ? true : false;
    }

    /// <summary>
    /// 从读取索引位置开始读取len长度的字节数组
    /// </summary>
    /// <param name="len"></param>
    /// <returns></returns>
    private byte[] Read(int len)
    {
        byte[] bytes = new byte[len];
        Array.Copy(buf, readIndex, bytes, 0, len);
        if (IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        readIndex += len;
        return bytes;
    }

    public ushort ReadUshort()
    {
        return BitConverter.ToUInt16(Read(2), 0);
    }

    public short ReadShort()
    {
        return BitConverter.ToInt16(Read(2), 0);
    }

    public char ReadChar()
    {
        return (char)ReadShort();
    }

    public uint ReadUint()
    {
        return BitConverter.ToUInt32(Read(4), 0);
    }

    public int ReadInt()
    {
        return BitConverter.ToInt32(Read(4), 0);
    }
        
    public ulong ReadUlong()
    {
        return BitConverter.ToUInt64(Read(8), 0);
    }
        
    public long ReadLong()
    {
        return BitConverter.ToInt64(Read(8), 0);
    }
        
    public float ReadFloat()
    {
        return BitConverter.ToSingle(Read(4), 0);
    }
        
    public double ReadDouble()
    {
        return BitConverter.ToDouble(Read(8), 0);
    }

    /// <summary>
    /// 读取字节数组
    /// </summary>
    /// <param name="len"></param>
    /// <returns></returns>
    public byte[] ReadBytes(int len)
    {
        return Read(len);
    }

    /// <summary>
    /// 自动读取字符串,先读取字符串前面的长度,再根据长度读字符串
    /// </summary>
    /// <returns></returns>
    public string ReadUTF8String()
    {
        int len = ReadUshort();
        return ReadString(len, System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// 读取指定长度的字符串
    /// </summary>
    /// <param name="len"></param>
    /// <returns></returns>
    public string ReadUTF8String(int len)
    {
        return ReadString(len, System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// 读取字符串
    /// </summary>
    /// <returns></returns>
    public string ReadString(int len, System.Text.Encoding encoding)
    {
        byte[] bytes = new byte[len];
        Array.Copy(buf, readIndex, bytes, 0, len);
        readIndex += len;
        return encoding.GetString(bytes);
    }

    /// <summary>
    /// 清除已读字节并重建缓存区
    /// </summary>
    public void DiscardReadBytes()
    {
        if (readIndex <= 0) return;
        int len = buf.Length - readIndex;
        byte[] newbuf = new byte[len];
        Array.Copy(buf, readIndex, newbuf, 0, len);
        buf = newbuf;
        writeIndex -= readIndex;
        markReadIndex -= readIndex;
        if (markReadIndex < 0)
        {
            markReadIndex = readIndex;
        }
        markWirteIndex -= readIndex;
        if (markWirteIndex < 0 || markWirteIndex < readIndex || markWirteIndex < markReadIndex)
        {
            markWirteIndex = writeIndex;
        }
        readIndex = 0;
    }

    /// <summary>
    /// 清空此对象
    /// </summary>
    public void Clear()
    {
        //buf = new byte[buf.Length];
        readIndex = 0;
        writeIndex = 0;
        markReadIndex = 0;
        markWirteIndex = 0;
    }

    /// <summary>
    /// 设置开始读取的索引
    /// </summary>
    /// <param name="index"></param>
    public void SetReaderIndex(int index)
    {
        if (index < 0) return;
        readIndex = index;
    }

    /// <summary>
    /// 设置开始写入的索引,未做越界判断
    /// </summary>
    /// <param name="index"></param>
    public void SetWriterIndex(int index)
    {
        if (index < 0) return;
        writeIndex = index;
    }

    /// <summary>
    /// 返回当前读索引
    /// </summary>
    /// <returns></returns>
    public int ReaderIndex()
    {
        return readIndex;
    }

    /// <summary>
    /// 返回当前写索引
    /// </summary>
    /// <returns></returns>
    public int WriterIndex()
    {
        return writeIndex;
    }

    /// <summary>
    /// 标记读取的索引位置
    /// </summary>
    public void MarkReaderIndex()
    {
        markReadIndex = readIndex;
    }

    /// <summary>
    /// 标记写入的索引位置
    /// </summary>
    public void MarkWriterIndex()
    {
        markWirteIndex = writeIndex;
    }

    /// <summary>
    /// 将读取的索引位置重置为标记的读取索引位置
    /// </summary>
    public void ResetReaderIndex()
    {
        readIndex = markReadIndex;
    }

    /// <summary>
    /// 将写入的索引位置重置为标记的写入索引位置
    /// </summary>
    public void ResetWriterIndex()
    {
        writeIndex = markWirteIndex;
    }

    /// <summary>
    /// 可读的有效字节数
    /// </summary>
    /// <returns></returns>
    public int ReadableBytes()
    {
        return writeIndex - readIndex;
    }

    /// <summary>
    /// 获取可读的字节数组
    /// </summary>
    /// <returns></returns>
    public byte[] ToArray()
    {
        byte[] bytes = new byte[writeIndex];
        Array.Copy(buf, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>
    /// 获取缓存区大小
    /// </summary>
    /// <returns></returns>
    public int GetCapacity()
    {
        return this.capacity;
    }

    /// <summary>
    /// 真实的字节数组
    /// </summary>
    public byte[] bytes { get { return buf; } }
}
