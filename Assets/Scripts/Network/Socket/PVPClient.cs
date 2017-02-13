
public class PVPClient : SocketClient {

    protected override void ReciveData(ByteBuffer buf)
    {
        TestProtocal ptoProtocal = new TestProtocal();
        ptoProtocal.Revice(buf);
    }

    protected override void OnConnectSuccess()
    {

    }

    protected override void OnDisconnect()
    {

    }
}
