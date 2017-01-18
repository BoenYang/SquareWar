using System.Collections.Generic;
using UnityEngine;

public class TestProtocal
{
    public short ActionCode = 101;
    public byte a;
    public short b;
    public int c;
    public long d;
    public bool e;
    public char f;
    public float g;
    public double h;
    public string j;
    public List<byte> k;
    public Dictionary<string,long> i;

    public TestProtocal()
    {                   //2
                        //1
        a = 20;         //1
        b = 30;         //2
        c = 40;         //4
        d = 123456789L; //8
        e = false;      //1
        f = 'a';        //2
        g = 1.23f;      //4
        h = 1.234567d;  //8
        j = "test";     //6

        k = new List<byte>();   //5
        k.Add(1);
        k.Add(2);
        k.Add(3);
        i = new Dictionary<string, long>();  //35
        i.Add("1",111L);       
        i.Add("2", 123L);
        i.Add("3", 555L);
    }

    public void Send(PVPClient client)
    {
        ByteBuffer byteBuffer = ByteBuffer.Allocate(81);
        byteBuffer.WriteShort(0);
        byteBuffer.WriteByte(1);
        byteBuffer.WriteShort(ActionCode);
        byteBuffer.WriteByte(a);
        byteBuffer.WriteShort(b);
        byteBuffer.WriteInt(c);
        byteBuffer.WriteLong(d);
        byteBuffer.WriteBool(e);
        byteBuffer.WriteChar(f);
        byteBuffer.WriteFloat(g);
        byteBuffer.WriteDouble(h);
        byteBuffer.WriteUTF8OfLen(j);
        byteBuffer.WriteShort((short)k.Count);
        for (int l = 0; l < k.Count; l++)
        {
            byteBuffer.WriteByte(k[l]);
        }
        byteBuffer.WriteShort((short)i.Count);

        foreach (var map in i)
        {
            byteBuffer.WriteUTF8OfLen(map.Key);
            byteBuffer.WriteLong(map.Value);
        }
        int writeIndex = byteBuffer.WriterIndex();
        int length = writeIndex - 2;
        byteBuffer.MarkWriterIndex();
        byteBuffer.SetWriterIndex(0);
        byteBuffer.WriteShort((short)length);
        byteBuffer.ResetWriterIndex();
        client.Send(byteBuffer);
    }

    public void Revice(ByteBuffer buffer)
    {
        int length = buffer.ReadInt();
        bool zip = buffer.ReadBool();
        byte seq = buffer.ReadByte();
        short actionId = buffer.ReadShort();
        byte state = buffer.ReadByte();
        Debug.Log("byte a = " + buffer.ReadByte());
        Debug.Log("short b = " + buffer.ReadShort());
        Debug.Log("int c = " + buffer.ReadInt());
        Debug.Log("int d = " + buffer.ReadLong());
        Debug.Log("bool e = " + buffer.ReadBool());
        Debug.Log("char f = " + buffer.ReadChar());
        Debug.Log("float g = " + buffer.ReadFloat());
        Debug.Log("double h = " + buffer.ReadDouble());
        Debug.Log("string j = " + buffer.ReadUTF8String());
        int kLength = buffer.ReadShort();
        Debug.Log("list k length = " + kLength);
        for (int l = 0; l < kLength; l++)
        {
            Debug.Log("list k [" + l + "] = " + buffer.ReadByte());
        }
        int iLength = buffer.ReadShort();
        Debug.Log("map i length = " + iLength);
        for (int l = 0; l < iLength; l++)
        {
            Debug.Log("map k [" + buffer.ReadUTF8String() + "] = " + buffer.ReadLong());
        }
    }
}
