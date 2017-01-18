
public class PVPPlayer : PlayerBase
{
    public bool IsLocal
    {
        get { return isLocal; }
    }

    protected bool isLocal = false;

    public PVPPlayer(bool isLocal)
    {
        this.isLocal = isLocal;
        this.isRobot = false;
    }
}
