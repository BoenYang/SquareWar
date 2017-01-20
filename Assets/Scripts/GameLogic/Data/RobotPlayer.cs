
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class RobotPlayer : PlayerBase
{
    private SquareData[,] clonedMap;

    private List<AICommand> commandList = new List<AICommand>();

    private float thinkTimer = 0 ;

    private float thinkInterval = 2f;

    public class AICommand
    {
        public int SwapRow;
        public int SwapColumn;
        public MoveDir SwapDir;
        public int score;
        public int SwapStep;

        public override string ToString()
        {

            return string.Format("Swap [{0},{1} to {2}]",SwapRow,SwapColumn,SwapDir.ToString());
        }
    }
    public class SquareData
    {
        public int Row;

        public int Column;

        public int Type;

        public SquareState State;

        public bool Chain;

        public SquareData(SquareSprite square)
        {
            Row = square.Row;
            Column = square.Column;
            Chain = square.Chain;
            State = square.State;
            Type = square.Type;
        }

        public bool CanSwap()
        {
            return State == SquareState.Static;
        }

        public void MarkWillRemove()
        {
            State = SquareState.Clear;
        }
    }

    public RobotPlayer()
    {
        isRobot = true;
        Name = "RobotPlayer";
    }

    public override void InitPlayerMap(MapMng mapMng, int[,] map)
    {
        base.InitPlayerMap(mapMng, map);
        clonedMap = new SquareData[raw, column];
    }

    protected override void OnGetScore(int addScore)
    {
        DemoUI.Ins.Player2Score.text = Name + ": " + Score;
    }

    public override void PlayerUpdate()
    {
        base.PlayerUpdate();
        AIThink();
    }

    private void AIThink()
    {
        thinkTimer += Time.deltaTime;
        if (thinkTimer > thinkInterval)
        {
            thinkTimer = 0;
            FindSwapSquare();
            ExcuteAICommand();
        }
    }

    private void ExcuteAICommand()
    {
        if (commandList.Count > 0)
        {
            AICommand commcon = commandList[0];
            SwapSquare(SquareMap[commcon.SwapRow, commcon.SwapColumn], commcon.SwapDir);
            commandList.RemoveAt(0);
        }
    }

    private void FindSwapSquare()
    {
        CopyMap(SquareMap,clonedMap);
        for (int r = 0; r < raw; r++)
        {
            for (int c = 0; c < column; c++)
            {
                SquareData checkSquare = clonedMap[r, c];
                if (checkSquare != null && checkSquare.CanSwap())
                {
                    AICommand command = new AICommand();
                    command.SwapColumn = c;
                    command.SwapRow = r;
                    command.SwapStep = 1;

                    //检查横向
                    if (CheckVerticalUpDown(checkSquare, MoveDir.Left))
                    {
                        command.SwapDir = MoveDir.Left;
                        commandList.Add(command);
                        Debug.Log(command);
                        return;
                    }else if (CheckVerticalUpDown(checkSquare, MoveDir.Right))
                    {
                        command.SwapDir = MoveDir.Right;
                        commandList.Add(command);
                        Debug.Log(command);
                        return;
                    }
                    else if (CheckVerticalUp2(checkSquare, MoveDir.Left))
                    {
                        command.SwapDir = MoveDir.Left;
                        commandList.Add(command);
                        Debug.Log(command);
                        return;
                    }
                    else if (CheckVerticalUp2(checkSquare, MoveDir.Right))
                    {
                        command.SwapDir = MoveDir.Right;
                        commandList.Add(command);
                        Debug.Log(command);
                        return;
                    }
                    else if(CheckVerticalDown2(checkSquare, MoveDir.Left))
                    {
                        command.SwapDir = MoveDir.Left;
                        commandList.Add(command);
                        Debug.Log(command);
                        return;
                    }
                    else if (CheckVerticalDown2(checkSquare, MoveDir.Right))
                    {
                        command.SwapDir = MoveDir.Right;
                        commandList.Add(command);
                        Debug.Log(command);
                        return;
                    }else if (CheckHorizontalLeft2(checkSquare))
                    {
                        command.SwapDir = MoveDir.Left;
                        commandList.Add(command);
                        Debug.Log(command);
                        return;
                    }
                    else if (CheckHorizontalRight2(checkSquare))
                    {
                        command.SwapDir = MoveDir.Right;
                        commandList.Add(command);
                        Debug.Log(command);
                        return;
                    }
                }
            }
        }
    }

    private bool CheckVerticalUpDown(SquareData checkSprite,MoveDir dir)
    {
        int upRow = checkSprite.Row - 1;
        int downRow = checkSprite.Row + 1;
        int c = checkSprite.Column + (dir == MoveDir.Left ? -1 : 1);

        if (upRow < 0)
        {
            return false;
        }

        if (downRow > raw - 1)
        {
            return false;
        }

        if (c < 0)
        {
            return false;
        }

        if (c > column - 1)
        {
            return false;
        }

        SquareData up = clonedMap[upRow, c];
        SquareData down = clonedMap[downRow, c];

        if (up != null && down != null && up.Type == checkSprite.Type && down.Type == checkSprite.Type && up.CanSwap() &&
            down.CanSwap())
        {
            up.MarkWillRemove();
            down.MarkWillRemove();
            checkSprite.MarkWillRemove();
            return true;
        }
        return false;
    }

    private bool CheckVerticalDown2(SquareData checkSprite, MoveDir dir)
    {
        int down1Row = checkSprite.Row + 1;
        int down2Row = checkSprite.Row + 2;
        int c = checkSprite.Column + (dir == MoveDir.Left ? -1 : 1);

        if (down1Row > raw - 1)
        {
            return false;
        }

        if (down2Row > raw - 1)
        {
            return false;
        }

        if (c < 0)
        {
            return false;
        }

        if (c > column - 1)
        {
            return false;
        }

        SquareData down1 = clonedMap[down1Row, c];
        SquareData down2 = clonedMap[down2Row, c];

        if (down1 != null && down2 != null && down1.Type == checkSprite.Type && down2.Type == checkSprite.Type && down1.CanSwap() &&
            down2.CanSwap())
        {
            down1.MarkWillRemove();
            down2.MarkWillRemove();
            checkSprite.MarkWillRemove();
            return true;
        }

        return false;
    }

    private bool CheckVerticalUp2(SquareData checkSprite,MoveDir dir)
    {
        int up1Row = checkSprite.Row - 1;
        int up2Row = checkSprite.Row - 2;
        int c = checkSprite.Column + (dir == MoveDir.Left ? -1 : 1);

        if (up1Row < 0)
        {
            return false;
        }

        if (up2Row < 0)
        {
            return false;
        }

        if (c < 0)
        {
            return false;
        }

        if (c > column - 1)
        {
            return false;
        }

        SquareData up1 = clonedMap[up1Row, c];
        SquareData up2 = clonedMap[up2Row, c];

        if (up1 != null && up2 != null && up1.Type == checkSprite.Type && up2.Type == checkSprite.Type && up1.CanSwap() &&
            up2.CanSwap())
        {
            up1.MarkWillRemove();
            up2.MarkWillRemove();
            checkSprite.MarkWillRemove();
            return true;
        }
        return false;
    }


    private bool CheckHorizontalLeft2(SquareData checkSprite)
    {
        int leftColumn1 = checkSprite.Column - 2;
        int leftColumn2 = checkSprite.Column - 3;
        int r = checkSprite.Row;

        if (leftColumn1 < 0)
        {
            return false;
        }

        if (leftColumn2 < 0)
        {
            return false;
        }

        SquareData left1 = clonedMap[r, leftColumn1];
        SquareData left2 = clonedMap[r, leftColumn2];

        if (left1 != null && left2 != null && left1.Type == checkSprite.Type && left2.Type == checkSprite.Type && left1.CanSwap() &&
            left2.CanSwap())
        {
            left1.MarkWillRemove();
            left2.MarkWillRemove();
            checkSprite.MarkWillRemove();
            return true;
        }
        return false;
    }

    private bool CheckHorizontalRight2(SquareData checkSprite)
    {
        int rightColumn1 = checkSprite.Column + 2;
        int rightColumn2 = checkSprite.Column + 3;
        int r = checkSprite.Row;

        if (rightColumn1 > column - 1)
        {
            return false;
        }

        if (rightColumn2 > column - 1)
        {
            return false;
        }

        SquareData right1 = clonedMap[r, rightColumn1];
        SquareData right2 = clonedMap[r, rightColumn2];

        if (right1 != null && right2 != null && right1.Type == checkSprite.Type && right2.Type == checkSprite.Type && right1.CanSwap() &&
            right2.CanSwap())
        {
            right1.MarkWillRemove();
            right2.MarkWillRemove();
            checkSprite.MarkWillRemove();
            return true;
        }
        return false;
    }

    private int SimulateSwap(int r, int c, MoveDir dir)
    {
        return 0;
    }

    private void SimelateRemove()
    {

    }

    private void SimulateDrop()
    {

    }

    private void CopyMap(SquareSprite[,] src, SquareData[,] dst)
    {
        for (int r = 0; r < src.GetLength(0); r++)
        {
            for (int c = 0; c < src.GetLength(1); c++)
            {
                if (src[r, c] != null)
                {
                    dst[r, c] = new SquareData(src[r,c]);
                }
                else
                {
                    dst[r, c] = null;
                }
            }
        }
    }

  
} 