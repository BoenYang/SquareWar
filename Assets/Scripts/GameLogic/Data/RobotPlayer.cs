using System;
using System.Collections.Generic;
using UnityEngine;

public class RobotPlayer : PlayerBase
{
    private SquareData[,] clonedMap;

    private List<AICommand> commandList = new List<AICommand>();

    private float thinkIntervalTimer = 0 ;

    private float thinkInterval = 2f;

    private float operatorInterval = 0.5f;

    private float operatorIntervalTimer = 0f;

    private int[] columnSquareCount;

    private int[] rowSquareCount;

    private int[,] rowTypeCount;

    private PlayerBase humanPlayer;

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
        columnSquareCount = new int[column];
        rowSquareCount = new int[row];
        rowTypeCount = new int[row,5];
        clonedMap = new SquareData[row, column];
    }

    public override void PlayerUpdate()
    {
        base.PlayerUpdate();
        UpdateMapStatisticsData();
        AIThink();
    }

    private void AIThink()
    {

        operatorIntervalTimer += Time.deltaTime;
        if (operatorIntervalTimer >= operatorInterval)
        {
            ExcuteAICommand();
            operatorIntervalTimer = 0;
        }

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

            if (commandList.Count == 0)
            {
                MoveTopSquare();
            }
        }
    }

    private void ExcuteAICommand()
    {
        if (commandList.Count > 0)
        {
            AICommand command = commandList[0];
            SquareSprite operateSquare = SquareMap[command.SwapSquare.Row, command.SwapSquare.Column];
            //Debug.LogFormat("[AI] Excute command [{0},{1}] {2}",command.SwapSquare.Row,command.SwapSquare.Column,command.SwapDir);
            if (operateSquare == null)
            {
                commandList.RemoveAt(0);
            }
            else
            {
                if (operateSquare.CanSwap())
                {
                    command.SwapStep--;
                    SwapSquare(operateSquare, command.SwapDir);
                    if (command.SwapStep <= 0)
                    {
                        commandList.RemoveAt(0);
                    }
                }
            }
        }
    }

    private void AddAICommand(AICommand command)
    {
        //Debug.Log(command);
        commandList.Add(command);
    }

    private void FindRemovableSquare()
    {
        CopyMap(SquareMap,clonedMap);
        for (int r = 0; r < row; r++)
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

        if (downRow > row - 1)
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

        if (down1Row > row - 1)
        {
            return false;
        }

        if (down2Row > row - 1)
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
    /// 2.行序号小于r行有两个类型为T的可到达r行的方块
    /// 纵向组成两个方块的条件:
    /// 1.r行之上有两个可以到达[r-1,c],[r-2,c]的类型为T的方块
    /// 2.r+1,r+2行有两个可以到达[r+1,c],[r+2,c]的类型为T的方块 （本算法采用该条件，由于是从上往下扫描，可以囊括1.3两种情况）
    /// 3.r+1行有两个可以到达[r+1,c]的类型为T的方块，r行之上有可以到达[r-1,c]的类型为T的方块
    /// </summary>
    /// <returns></returns>
    private void MakeThreeSquare()
    {
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                SquareSprite square = SquareMap[r, c];
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

        
                }
            }
        }

        for (int r = 0; r < row; r++)
        {

            int type = -1;

            for (int t = 0; t < 5; t++)
            {
                if (rowTypeCount[r, t] >= 3)
                {
                    type = t + 1;
                    break;
                }
            }

            if (type < 0)
            {
                continue;
            }

            //横向方法
            if (rowTypeCount[r, type - 1] >= 3)
            {
                int[] squareIndexes = GetSquareIndexesAtRowByType(r, type);

                if (rowTypeCount[r, type - 1] == 3)
                {
                    SquareSprite center = SquareMap[r, squareIndexes[1]];
                    SquareSprite left = SquareMap[r, squareIndexes[0]];
                    SquareSprite right = SquareMap[r, squareIndexes[2]];

                    if (left.Column == center.Column - 1)
                    {
                        AICommand command = new AICommand();
                        command.SwapSquare = right;
                        command.SwapDir = MoveDir.Left;
                        command.SwapStep = right.Column - center.Column - 1;
                        AddAICommand(command);
                        return;
                    }

                    if (right.Column == center.Column + 1)
                    {
                        AICommand command = new AICommand();
                        command.SwapSquare = left;
                        command.SwapDir = MoveDir.Right;
                        command.SwapStep = center.Column - left.Column - 1;
                        AddAICommand(command);
                        return;
                    }


                    if (right.Column != center.Column + 1 && left.Column != center.Column - 1)
                    {
                        AICommand command1 = new AICommand();
                        command1.SwapSquare = right;
                        command1.SwapDir = MoveDir.Left;
                        command1.SwapStep = right.Column - center.Column - 1;
                        AddAICommand(command1);

                        AICommand command2 = new AICommand();
                        command2.SwapSquare = left;
                        command2.SwapDir = MoveDir.Right;
                        command2.SwapStep = center.Column - left.Column - 1;
                        AddAICommand(command2);
                        return;
                    }

                }
                else if (rowTypeCount[r, type - 1] == 4)
                {
                    SquareSprite center = SquareMap[r, squareIndexes[2]];
                    SquareSprite left = SquareMap[r, squareIndexes[1]];
                    SquareSprite right = SquareMap[r, squareIndexes[3]];

                    if (left.Column == center.Column - 1)
                    {
                        AICommand command = new AICommand();
                        command.SwapSquare = right;
                        command.SwapDir = MoveDir.Left;
                        command.SwapStep = right.Column - center.Column - 1;
                        AddAICommand(command);
                        return;
                    }

                    if (right.Column == center.Column + 1)
                    {
                        AICommand command = new AICommand();
                        command.SwapSquare = left;
                        command.SwapDir = MoveDir.Right;
                        command.SwapStep = center.Column - left.Column - 1;
                        AddAICommand(command);
                        return;
                    }

                }
            }
        }
    }

    /// <summary>
    /// 获取r行中类型为type的所有方块列索引
    /// </summary>
    /// <param name="r">行索引</param>
    /// <param name="type">类型</param>
    /// <returns>类型为type的所有方块列索引</returns>
    private int[] GetSquareIndexesAtRowByType(int r,int type)
    {
        int[] squareIndexes = new int[rowTypeCount[r,type - 1]];
        int i = 0;
        for (int c = 0; c < column; c++)
        {
            if (SquareMap[r, c] != null && SquareMap[r,c].Type == type)
            {
                squareIndexes[i] = c;
                i++;
            }
        }
        return squareIndexes;
    }

    /// <summary>
    /// 将顶部的方块移动到最近的方块数目最少的列
    /// </summary>
    private void MoveTopSquare()
    {
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                SquareSprite square = SquareMap[r, c];
                if (square != null)
                {
                    int rowCount = rowSquareCount[r];
                    if (rowCount < 4)
                    {
                        int nearestLowSquareColumn = FindNearestLowSquareColumn(square);
                        if (nearestLowSquareColumn != -1)
                        {
                            AICommand command = new AICommand();
                            command.SwapSquare = SquareMap[r, c];
                            command.SwapStep = Math.Abs(square.Column - nearestLowSquareColumn);
                            command.SwapDir = (MoveDir) (-1 * Math.Sign(square.Column - nearestLowSquareColumn));
                            AddAICommand(command);
                            return;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 统计每一行和每一列的方块数目,每一行各个类型的方块数目
    /// </summary>
    private void UpdateMapStatisticsData()
    {
        Array.Clear(columnSquareCount,0,columnSquareCount.Length);
        Array.Clear(rowSquareCount, 0, rowSquareCount.Length);


        for (int r = 0; r < rowTypeCount.GetLength(0); r++)
        {
            for (int t = 0; t < rowTypeCount.GetLength(1); t++)
            {
                rowTypeCount[r, t] = 0;
            }
        }

        for (int c = 0; c < column; c++)
        {
            for (int r = 0; r < row; r++)
            {
                if (SquareMap[r, c] != null && SquareMap[r,c].State != SquareState.Hide)
                {
                    columnSquareCount[c]++;
                    rowSquareCount[r]++;
                    rowTypeCount[r, SquareMap[r, c].Type - 1]++;
                }
            }
        }
    }

    private int FindNearestLowSquareColumn(SquareSprite square)
    {
        int left = square.Column;
        int right = square.Column;
        int leftColumnCount = int.MaxValue;
        int rightColumnCount = int.MaxValue;

        //left
        if (square.Column - 1 >= 0)
        {
            
            for (int i = square.Column - 1; i >= 0; i--)
            {
                if (columnSquareCount[i] < leftColumnCount)
                {
                    if (columnSquareCount[i] < columnSquareCount[square.Column] - 1)
                    {
                        left = i;
                        leftColumnCount = columnSquareCount[i];
                    }
                }
                else
                {
                    break;
                }
            }
        }

        if (square.Column + 1 < column - 1)
        {
            //right
            for (int i = square.Column + 1; i < column; i++)
            {
                if (columnSquareCount[i] < rightColumnCount)
                {
                    if (columnSquareCount[i] < columnSquareCount[square.Column] - 1)
                    {
                        right = i;
                        rightColumnCount = columnSquareCount[i];
                    }
                }
                else
                {
                    break;
                }
            }
        }

        if (left != square.Column && right != square.Column)
        {
            return leftColumnCount >= rightColumnCount ? left : right;
        }
        else if (left == square.Column)
        {
            if (right == square.Column)
            {
                return -1;
            }
            else
            {
                return right;
            }
        }
        else 
        {
            if (right == square.Column)
            {
                return left;
            }
            else
            {
                return -1;
            }
        }

       
    }

    private SquareSprite FindNearestMatchSquareInRaw(SquareSprite square,int row)
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
            int up = row;
            int down = targetRaw - 1;

            for (int i = down; i <= up; i++)
            {
                if (SquareMap[i, targetColumn] != null)
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