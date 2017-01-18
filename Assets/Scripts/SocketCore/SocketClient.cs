using System.Net.Sockets;
using System;
using System.Text;
using System.Threading;

/**
 * socket通信,只处理粘包和心跳,协议解析由子类完成
 * 心跳没有长度和数据体
 * 长度为0的协议数据体为null
 * reciveData接收数据后必须立即处理,否则会被后面的协议覆盖
 * @author 邱洪波
 */

public enum SocketState
{
    Connecting,
    Connected,
    Disconnecting,
    Disconnected
}

public abstract class SocketClient
{
    #region 属性定义

    private string host;
    private int port;

      
    private bool isReconnect = false;
    private bool reciveLoop = true;
    private int currentHeartTime;
    private int currentTimeout;

      
    private Thread reciveThread;

    protected Socket client;
    protected ByteBuffer reciveBuf;

    public SocketState currentState
    {
        get;
        protected set;
    }

    public SocketConfig config
    {
        get;
        protected set;
    }


    public bool connected
    {
        get { return client.Connected; }
    }

    private int reciveThreadInterval = 100;

    public int TimeOut = 20000;

    public bool OpenLog = true;

	#endregion
 
	#region 公有方法public

    public SocketClient()
    {
        LingerOption myOpts = new LingerOption(true, 1);

        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, myOpts);
        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
        client.NoDelay = true;

        currentState = SocketState.Disconnected;
        reciveBuf = ByteBuffer.Allocate(1024);
    }
        
    public void Connect(string host, int port)
    {
        this.host = host;
        this.port = port;

        //IPAddress ipAddress = IPAddress.Parse (this.host); 
        //服务器端口 
        //IPEndPoint ipEndpoint = new IPEndPoint (ipAddress, this.port);
        isReconnect = false;
        TryConnect();
    }

    public void Reconnect()
    {
        if (host != "")
            throw new Exception("没有connect过, 不能重连");
        Disconnect();
        isReconnect = true;
        TryConnect();
    }

    public void Disconnect()
    {
        if (connected)
        {
            try
            {
                currentState = SocketState.Disconnecting;
                client.Disconnect(true);
                currentState = SocketState.Disconnected;
            }
            catch (Exception e)
            {
                Debug(e.StackTrace);
            }
        }
    }

    /// <summary>
    /// 发送数据给服务器
    /// </summary>
    /// <param name="buffer"></param>
    public void Send(ByteBuffer buffer)
    {
        try
        {
            client.Send(buffer.bytes, 0, buffer.WriterIndex(), SocketFlags.None);
        }
        catch (Exception e)
        {
            Debug(e.StackTrace);
        }
    }
        
    //关闭并释放资源,退出游戏才需要使用
    public void Close()
    {
        reciveLoop = false;
        if (client != null)
        {
            //client.Shutdown(SocketShutdown.Both);
            if (connected)
                client.Disconnect(false);
            client.Close();
            StopReciveThread();
        }
    }

    #endregion

    #region 保护方法protected

    protected abstract void ReciveData(ByteBuffer buf);

    protected virtual void OnConnectSuccess() { }

    protected virtual void OnConnectFailed() { }

    protected virtual void OnDisconnect(){ }

    protected virtual void OnTimeOut() { }

    protected void Debug(params object[] args)
    {
        if (OpenLog)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                result.Append(args[i]).Append("\t");
            }
            UnityEngine.Debug.Log(result.ToString());
        }
    }

    #endregion

    #region 私有方法private,包属性方法internal

    private void OnConnect(IAsyncResult result)
    {
        client.EndConnect(result);
        if (connected)
        {
            Debug("连接[" + host + ":" + port + "]成功.....",client.Connected,client.Blocking);
            currentState = SocketState.Connected;
            OnConnectSuccess();
            StartReciveThread();
        }
        else
        {
            Debug("连接[" + host + ":" + port + "]失败.....");
            currentState = SocketState.Disconnected;
            OnConnectFailed();
        }
    }

    private void StartReciveThread()
    {
        StopReciveThread();

        reciveLoop = true;
        reciveThread = new Thread(DoRecive);
        reciveThread.IsBackground = true;
        reciveThread.Start();
    }

    private void StopReciveThread()
    {
        if (reciveThread != null && reciveThread.IsAlive)
        {
            reciveThread.Interrupt();
            reciveThread = null;
        }
    }

    private void TryConnect()
    {
        try
        {
            Debug("开始连接[" + host + ":" + port + "]" + (isReconnect ? "isReconnect" : "") + ".....");
            currentTimeout = 0;
            currentState = SocketState.Connecting;
            client.BeginConnect(this.host, this.port, new AsyncCallback(OnConnect), client);
        }
        catch (Exception e)
        {
            OnConnectFailed();
        }
    }

    private void DoRecive()
    {
        while (reciveLoop)
        {
            if (client.Connected)
            {
                //计算心跳发送
                //CheckHeartBeating();
                int len = client.Available;
                if (len > 0)
                {
                    try
                    {
                        reciveBuf.Clear();
                        reciveBuf.Recive(client, len);
                        ReciveData(reciveBuf);
                    }
                    catch (Exception e)
                    {
                        Debug(e.StackTrace);
                    }
                }
            }
            else
            {
                if (currentState == SocketState.Connected)
                {
                    currentState = SocketState.Disconnected;
                    Debug("服务器断开连接....");
                    OnDisconnect();
                }
            }
            //暂停一个接受周期
            try 
	        {	        
		        Thread.Sleep(reciveThreadInterval);
	        }
	        catch (Exception e)
	        {
                Debug(e.StackTrace);
	        }
        }
    }

    private void trySend(byte[] data)
    {
        try
        {
            if (client.Connected)
                client.Send(data);
            else
                Reconnect();
        }
        catch (Exception e)
        {
            Debug(e.StackTrace);
        }
    }

//        private void CheckHeartBeating()
//        {
//            currentTimeout += config.reciveTime;
//            if (currentTimeout >= config.timeout)
//            {
//                SocketState lastState = currentState;
//                Disconnect();
//                if (lastState == SocketState.Connecting)//连接超时
//                {
//                    OnTimeOut();
//                }
//                else
//                {
//                    Debug("心跳超时断开连接....");
//                    OnDisconnect();
//                }
//            }
//            else
//            {
//                currentHeartTime += config.reciveTime;
//                if (currentHeartTime >= config.heartTime)
//                {
//                    currentHeartTime = 0;
//                    trySend(config.heartData);
//                }
//            }
//        }

    private void ReciveHeart()
    {
        currentTimeout = 0;
        currentHeartTime = 0;
    }



    #endregion
}
