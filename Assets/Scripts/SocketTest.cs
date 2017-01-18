using UnityEngine;


public class SocketTest : MonoBehaviour
{

    private PVPClient client;

    public void OnGUI()
    {
        GUILayout.Space(50);

        if (GUILayout.Button("测试Socket连接", GUILayout.Width(200)))
        {
            client = new PVPClient();
            client.OpenLog = true;
            client.Connect("192.168.41.54", 8001);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("协议测试", GUILayout.Width(200)))
        {
            TestProtocal protocal = new TestProtocal();
            protocal.Send(client);
        }
    }

    public void OnApplicationQuit()
    {
        if (client != null)
        {
            client.Close();
        }
    }
}
