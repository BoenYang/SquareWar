﻿using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public static GameUI Ins;

    public Text Player1Score;

    public Text Player2Score;

    public Text Player1Chain;

    public Text Player2Chain;

    public GameObject Player1View;

    public GameObject Player2View;

    public GameObject Player1ScoreContainer;

    public GameObject Player2ScoreContainer;

    public GameObject ResultView;

    public Text ResultText;

    private Vector3 player1ViewInitPos;

    void Awake()
    {
        Ins = this;
        player1ViewInitPos = Player1View.transform.localPosition;
    }

    public void Init(GameMode mode)
    {

        Player1Score.text = "0";
        Player2Score.text = "0";

        Player1Chain.text = "";
        Player2Chain.text = "";

        if (mode == GameMode.StandAlone)
        {
            Player1ScoreContainer.SetActive(true);
            Player2ScoreContainer.SetActive(false);

            Player1View.SetActive(true);
            Player2View.SetActive(false);

            Vector3 pos = player1ViewInitPos;
            pos.x = 0;
            Player1View.transform.localPosition = pos;


        } else if (mode == GameMode.PVE)
        {
            Player1View.SetActive(true);
            Player2View.SetActive(true);

            Player1View.transform.localPosition = player1ViewInitPos;

            Player1ScoreContainer.SetActive(true);
            Player2ScoreContainer.SetActive(true);
        }
    }

    public void OnBackClick()
    {
        GameScene.Instance.StopGame();
        SoundMng.Instance.StopMusic();
        MapMng.Instance.ClearAllPlayer();
        UIController.Ins.ShowMainUI();
    }

    public void OnRestartGameClick()
    {
        GameScene.Instance.Game.RestartGame();
    }

    public void ShowResultView(List<PlayerBase> players)
    {
        ResultView.SetActive(true);

        if (GameScene.Instance.Mode == GameMode.PVE)
        {
            PlayerBase winner = players.Find((p) => !p.IsGameOver);

            if (winner.IsRobot)
            {
                ResultText.text = "方块到顶了，您输了!";
            }
            else
            {
                ResultText.text = "恭喜您获得胜利^_^";
            }
        }else if (GameScene.Instance.Mode == GameMode.StandAlone)
        {
            ResultText.text = "方块到顶了，您输了!";
        }
    }

    public void CloseResultView()
    {
        ResultView.SetActive(false);
    }

    public void OnAddSpeedDown() {
        GameScene.Instance.Game.LocalPlayer.StartAddSpeed();
    }

    public void OnAddSpeedUp()
    {
        GameScene.Instance.Game.LocalPlayer.StopAddSpeed();
    }

    public void ShowChainRemoveTextAtPos(Vector3 pos, int chainCount)
    {
        Vector3 txtPos = Camera.main.WorldToScreenPoint(pos);
        txtPos = UIController.Ins.canvas.worldCamera.ScreenToWorldPoint(txtPos);
        txtPos.z = 0;

        GameObject txtGo = Instantiate(Resources.Load<GameObject>("Prefab/Effect/ChainText"));
        txtGo.transform.localScale = Vector3.zero;
        txtGo.transform.SetParent(transform);
        txtGo.transform.position = txtPos;
        txtGo.layer = gameObject.layer;

        CanvasGroup cg = txtGo.GetComponent<CanvasGroup>();
        
        Text txt = txtGo.GetComponentInChildren<Text>();
        txt.text = "连消+" + chainCount;

        Sequence seq = DOTween.Sequence();
        seq.Append(txtGo.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
        seq.AppendInterval(0.5f);
        seq.Append(DOTween.To(() =>{ return cg.alpha;}, (a) =>{ cg.alpha = a;},0,0.2f));
        seq.AppendCallback(() =>
        {
            Destroy(txtGo);
        });
    }
}
