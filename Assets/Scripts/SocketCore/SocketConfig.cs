using System.Text;
using System;

/**
 * 
 * @author 邱洪波
 */

public class SocketConfig
{
    public SocketConfig()
    {
        heartData = new byte[LENGTH_SIZE];
        for (int i = 0; i < LENGTH_SIZE; i++)
        {
            heartData[i] = 0;
        }
        timeout = 5 * heartTime;
    }

    public bool IsLittleEndian = false;

    /// <summary>
    /// 字符串编码
    /// </summary>
    public Encoding encode = Encoding.UTF8;

    /// <summary>
    /// 是否开启日志打印
    /// </summary>
    public bool openLog = true;

    /// <summary>
    /// 字符串长度所占字节数
    /// </summary>
    public int stringSize = 2;

    ///// <summary>
    ///// 默认心跳指令
    ///// </summary>
    //public ushort HEART_CMD = 0;
    public byte[] heartData;//心跳数据

    /// <summary>
    /// 心跳命令
    /// </summary>
    public int HEART_CMD = 0;

    /// <summary>
    /// 协议头长度,支持1(byte),2(short),4(int)
    /// </summary>
    public int HEAD_SIZE = 1;

    /// <summary>
    /// 数据内容长度
    /// </summary>
    public int LENGTH_SIZE = 2;

    /// <summary>
    /// 收数据间隔
    /// </summary>
    public int reciveTime = 500;

    /// <summary>
    /// 心跳间隔一秒
    /// </summary>
    public int heartTime = 5000;

    /// <summary>
    /// 超时最大间隔
    /// </summary>
    public int timeout;

    /// <summary>
    /// 断线重连次数,0表示不重连
    /// </summary>
    public int reconnectCount = 0;

    /// <summary>
    /// 字节数组转换成数值,用来处理协议头和长度
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public virtual int ConverNum(byte[] bytes)
    {
        int len = bytes.Length;
        if (len == 1)
            return bytes[0];
            
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return len == 2 ? BitConverter.ToUInt16(bytes, 0) : BitConverter.ToInt32(bytes, 0);
    }


    /// <summary>
    /// 默认配置信息
    /// </summary>
    public static readonly SocketConfig DefaultConfig = new SocketConfig();
}
