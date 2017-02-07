
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class RobotPlayer : PlayerBase
{
    private SquareData[,] clonedMap;

    private List<AICommand> commandList = new List<AICommand>();

    private float thinkIntervalTimer = 0 ;

    private float thinkInterval = 2f;

    private float operatorInterval = 0.5f;

    private float operatorIntervalTimer = 0f;


    public class AICommand
    {
        public SquareSprite SwapSquare;
        public MoveDir SwapDir;
        public int score;
        public int SwapStep = 1;

        public override string ToString()
        {
            return string.Format("Swap [{0},{1} to {2}] using step {3}", SwapSquare.Row, SwapSquare.Column, SwapDir.ToString(),SwapStep);
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
        thinkIntervalTimer += Time.deltaTime;
        if (thinkIntervalTimer > thinkInterval)
        {
            thinkIntervalTimer = 0;
            if (commandList.Count == 0)
            {
                FindRemovableSquare();
            }
            if (commandList.Count == 0)
            {
                MakeThreeSquare();
            }
        }

        operatorIntervalTimer += Time.deltaTime;
        if (operatorIntervalTimer >= operatorInterval)
        {
            ExcuteAICommand();
            operatorIntervalTimer = 0;
        }
    }

    private void ExcuteAICommand()
    {
        if (commandList.Count > 0)
        {
            AICommand command = commandList[0];
            command.SwapStep--;
            SwapSquare(SquareMap[command.SwapSquare.Row, command.SwapSquare.Column], command.SwapDir);
            if (command.SwapStep <= 0)
            {
                commandList.RemoveAt(0);
            }
        }
    }

    private void AddAICommand(AICommand command)
    {
        Debug.Log(command);
        commandList.Add(command);
    }

    private void FindRemovableSquare()
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
                    command.SwapSquare = SquareMap[r, c];
                    command.SwapStep = 1;

                    //检查横向
                    if (CheckVerticalUpDown(checkSquare, MoveDir.Left))
                    {
                        command.SwapDir = MoveDir.Left;
                        AddAICommand(command);
                        return;
                    }else if (CheckVerticalUpDown(checkSquare, MoveDir.Right))
                    {
                        command.SwapDir = MoveDir.Right;
                        AddAICommand(command);
                        return;
                    }
                    else if (CheckVerticalUp2(checkSquare, MoveDir.Left))
                    {
                        command.SwapDir = MoveDir.Left;
                        AddAICommand(command);
                        return;
                    }
                    else if (CheckVerticalUp2(checkSquare, MoveDir.Right))
                    {
                        command.SwapDir = MoveDir.Right;
                        AddAICommand(command);
                        return;
                    }
                    else if(CheckVerticalDown2(checkSquare, MoveDir.Left))
                    {
                        command.SwapDir = MoveDir.Left;
                        AddAICommand(command);
                        return;
                    }
                    else if (CheckVerticalDown2(checkSquare, MoveDir.Right))
                    {
                        command.SwapDir = MoveDir.Right;
                        AddAICommand(command);
                        return;
                    }else if (CheckHorizontalLeft2(checkSquare))
                    {
                        command.SwapDir = MoveDir.Left;
                        AddAICommand(command);
                        return;
                    }
                    else if (CheckHorizontalRight2(checkSquare))
                    {
                        command.SwapDir = MoveDir.Right;
                        AddAICommand(command);
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// *x  x*
    /// x*  *x
    /// *x  x*
    /// </summary>
    /// <param name="checkSprite"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    private bool CheckVerticalUpDown(SquareData checkSprite,MoveDir dir)
    {
        int upRow = checkSprite.Row - 1;
        int downRow = checkSprite.Row + 1;
        int c = checkSprite.Column + (int)dir;

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

    /// <summary>
    /// x*  x*
    /// x*  x*
    /// *x  *x
    /// </summary>
    /// <param name="checkSprite"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    private bool CheckVerticalDown2(SquareData checkSprite, MoveDir dir)
    {
        int down1Row = checkSprite.Row + 1;
        int down2Row = checkSprite.Row + 2;
        int c = checkSprite.Column + (int)dir;

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

    /// <summary>
    /// *x  x*
    /// x*  *x
    /// x*  *x
    /// </summary>
    /// <param name="checkSprite"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    private bool CheckVerticalUp2(SquareData checkSprite,MoveDir dir)
    {
        int up1Row = checkSprite.Row - 1;
        int up2Row = checkSprite.Row - 2;
        int c = checkSprite.Column + (int)dir;

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

    /// <summary>
    /// xx*x
    /// </summary>
    /// <param name="checkSprite"></param>
    /// <returns></returns>
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

    /// <summary>
    /// x*xx
    /// </summary>
    /// <param name="checkSprite"></param>
    /// <returns></returns>
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


    /// <summary>
    /// 找到两个个方块移动组成三个方块
    /// 设当前检测为r行,c列的方块类型为T，组成三个个方块需要检测横向条件和纵向条件
    /// 横向组成三个方块的条件：
    /// 1.r行，不等于c列的其他列有两个T类型的方块
    /// 2.行序号小于r行有一个类型为T的可到达r行的方块
    /// 纵向组成两个方块的条件:
    /// 1.r行之上有两个可以到达[r-1,c],[r-2,c]的类型为T的方块
    /// 2.r+1,r+2行有两个可以到达[r+1,c],[r+2,c]的类型为T的方块 （本算法采用该条件，由于是从上往下扫描，可以囊括1.3两种情况）
    /// 3.r+1行有两个可以到达[r+1,c]的类型为T的方块，r行之上有可以到达[r-1,c]的类型为T的方块
    /// </summary>
    /// <returns></returns>
    private void MakeThreeSquare()
    {
        for (int r = 0; r < clonedMap.GetLength(0); r++)
        {
            for (int c = 0; c < clonedMap.GetLength(1); c++)
            {
                SquareData square = clonedMap[r, c];
                if (square != null)
                {
                    //纵向方法
                    int down1Row = square.Row + 1;
                    int down2Row = square.Row + 2;

                    if (down1Row >= SquareMap.GetLength(0) || down2Row >= SquareMap.GetLength(0))
                    {
                        continue;
                    }
           
                    SquareSprite down1RowMatchSquare = FindNearestMatchSquareInRaw(square,down1Row);
                    SquareSprite down2RowMatchSquare = FindNearestMatchSquareInRaw(square,down2Row);
                    bool down1CanMove = false;
                    bool down2CanMove = false;
                    if (down1RowMatchSquare != null && down2RowMatchSquare != null)
                    {
                        if (down1RowMatchSquare.Column != square.Column)
                        {
                            int targetColumn = square.Column + 1 * Math.Sign(down1RowMatchSquare.Column - square.Column);
                            int targetRow = down1RowMatchSquare.Row;
                            if (CanMoveTo(down1RowMatchSquare, targetRow, targetColumn))
                            {
                                down1CanMove = true;
                            }
                        }

                        if (down2RowMatchSquare.Column != square.Column)
                        {
                            int targetColumn = square.Column + 1 * Math.Sign(down2RowMatchSquare.Column - square.Column);
                            int targetRow = down2RowMatchSquare.Row;
                            if (CanMoveTo(down2RowMatchSquare, targetRow, targetColumn))
                            {
                                down2CanMove = true;
                            }
                        }

                        if (down1CanMove && down2CanMove)
                        {
                            AICommand command1 = new AICommand();
                            command1.SwapDir = (MoveDir)(-1 * Math.Sign(down1RowMatchSquare.Column - square.Column));
                            command1.SwapSquare = down1RowMatchSquare;
                            command1.SwapStep = Mathf.Abs(down1RowMatchSquare.Column - square.Column);
                            AddAICommand(command1);

                            AICommand command2 = new AICommand();
                            command2.SwapDir = (MoveDir)(-1 * Math.Sign(down2RowMatchSquare.Column - square.Column));
                            command2.SwapSquare = down2RowMatchSquare;
                            command2.SwapStep = Mathf.Abs(down2RowMatchSquare.Column - square.Column);
                            AddAICommand(command2);
                            return;
                        }
                    }

                    //横向方法

                }
            }
        }
    }

    private SquareSprite FindNearestMatchSquareInRaw(SquareData square,int row)
    {
        int gap = int.MaxValue;
        SquareSprite matchSquare = null;
        for (int c = 0; c < SquareMap.GetLength(1); c++)
        {
            SquareSprite sq = SquareMap[row, c];
            if (sq != null && sq.Type == square.Type)
            {
                if (Math.Abs(square.Column - sq.Column) <= gap)
                {
                    gap = Math.Abs(square.Column - sq.Column);
                    matchSquare = sq;
                }
            }
        }
        return matchSquare;
    }


    /// <summary>
    /// 检查方块是否可以移动指定行和指定列
    /// 设检测的方块为r行，c列，目标行为tr,tc列，可移动过去的条件为
    /// 1.如果方块和目标行在同一行,当r+1行的tc列到c列之间方块都不为空则可以移动
    /// 2.如果方块行大于目标行，不能移动
    /// 3.如果方块行小于目标行，当r-tr行的tc列的方块都为空时可以移动
    /// </summary>
    /// <param name="square">要检查的方块</param>
    /// <param name="targetRaw">要移动到的行</param>
    /// <param name="tartetColumn">要移动到的列</param>
    /// <returns>是否可以移动过去</returns>
    private bool CanMoveTo(SquareSprite square, int targetRaw, int targetColumn)
    {
        int r = square.Row;
        int c = square.Column;

        bool canMove = true;

        //目标位置与要移动的方块在同一行
        if (targetRaw == r)
        {
            int left = c < targetColumn ? c : targetColumn;
            int right = c > targetColumn ? c : targetColumn;

            if (r < SquareMap.GetLength(0) - 1)
            {
                for (int i = left + 1; i < right - 1; i++)
                {
                    if (SquareMap[r + 1, i] == null)
                    {
                        canMove = false;
                        break;
                    }
                }
            }
        }else if (targetRaw > r)   //目标位置在移动方块的下面
        {
            int up = raw;
            int down = targetRaw;

            for (int i = down; i <= up; i++)
            {
                if (SquareMap[i, c] != null)
                {
                    canMove = false;
                    break;
                }
            }

        }
        else //目标位置在移动方块上面
        {
            canMove = false;
        }
        return canMove;
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