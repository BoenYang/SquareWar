
public class PVPClient : SocketClient {

    protected override void ReciveData(ByteBuffer buf)
    {
        TestProtocal ptoProtocal = new TestProtocal();
        ptoProtocal.Revice(buf);
    }

    protected override void OnConnectSuccess()
    {
        UnityEngine.Debug.Log("connect success");
    }

    protected override void OnDisconnect()
    {
        UnityEngine.Debug.Log("OnDisconnect");
    }
}
