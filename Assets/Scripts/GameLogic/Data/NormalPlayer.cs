public class NormalPlayer : PlayerBase
{
    public NormalPlayer()
    {
        Name = "LocalPlayer";
        isRobot = false;
    }

    protected override void OnGetScore(int addScore)
    {
        DemoUI.Ins.Player1Score.text = Name + ": " + Score;
    }

    protected override void OnChain(int chainCount)
    {
        if (chainCount > 1)
        {
            DemoUI.Ins.Chain.gameObject.SetActive(true);
            DemoUI.Ins.Chain.text = "连击+" + chainCount;
        }
        else
        {
            DemoUI.Ins.Chain.gameObject.SetActive(false);
        }
    }
}