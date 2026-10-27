// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and relevant international regulations.
//   使用本项目须严格遵守相应法律法规与开源许可证之规定。
//   Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//   本项目采用 Apache License 2.0 单协议分发，
//   This project is licensed solely under the Apache License 2.0,
//   完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//   please refer to the LICENSE file in the root directory of the source code for the full license text.
//   禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//   It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//   侵犯他人合法权益等法律法规所禁止的行为！
//   or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//   因基于本项目二次开发所产生的一切法律纠纷与责任，
//   Any disputes and liabilities arising from secondary development based on this project
//   本项目组织与贡献者概不承担。
//   shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//   GitHub 仓库：https://github.com/GameFrameX
//   GitHub Repository: https://github.com/GameFrameX
//   Gitee  仓库：https://gitee.com/GameFrameX
//   Gitee Repository:  https://gitee.com/GameFrameX
//   CNB  仓库：https://cnb.cool/GameFrameX
//   CNB Repository:   https://cnb.cool/GameFrameX
//   官方文档：https://gameframex.doc.alianblank.com/
//   Official Documentation: https://gameframex.doc.alianblank.com/
//  ==========================================================================================


using GameFrameX.Apps.Game.ConnectFour.Entity;

namespace GameFrameX.Hotfix.Logic.Game.ConnectFour;

/// <summary>
/// 四子棋对局规则判定。棋盘坐标约定：行主序 index = row*7+column，row 0 为底行（重力落点方向），格子取值 0 空、1/2 为座位号。
/// </summary>
internal static class ConnectFourBoardLogic
{
    /// <summary>
    /// 棋盘宽度（列数）。
    /// </summary>
    public const int Width = ConnectFourGameState.BoardWidth;

    /// <summary>
    /// 棋盘高度（行数）。
    /// </summary>
    public const int Height = ConnectFourGameState.BoardHeight;

    /// <summary>
    /// 连子取胜所需的连续棋子数。
    /// </summary>
    public const int WinLength = 4;

    /// <summary>
    /// 判断落子列是否在棋盘范围内（0-6）。
    /// </summary>
    /// <param name="column">落子列。</param>
    /// <returns>列合法返回 true。</returns>
    public static bool IsValidColumn(int column)
    {
        return column >= 0 && column < Width;
    }

    /// <summary>
    /// 计算重力落点：指定列中最底的空格行号。四子棋规则规定棋子必须落在该列最下方的空位。
    /// </summary>
    /// <param name="board">棋盘数组。</param>
    /// <param name="column">落子列。</param>
    /// <returns>落点行号（0-5）；该列已满返回 -1。</returns>
    public static int FindDropRow(int[] board, int column)
    {
        for (var row = 0; row < Height; row++)
        {
            if (board[row * Width + column] == 0)
            {
                return row;
            }
        }

        return -1;
    }

    /// <summary>
    /// 判断刚落下的棋子是否形成四连（横、竖、两条斜线四个方向）。以落点为中心向两侧延伸计数，
    /// 覆盖所有可能的连线组合，避免全盘扫描。
    /// </summary>
    /// <param name="board">棋盘数组。</param>
    /// <param name="row">落点行。</param>
    /// <param name="column">落点列。</param>
    /// <returns>形成四连及以上返回 true。</returns>
    public static bool CheckWin(int[] board, int row, int column)
    {
        var cell = board[row * Width + column];
        if (cell == 0)
        {
            return false;
        }

        // 横向 (1,0)、竖向 (0,1)、主对角线 (1,1)、副对角线 (1,-1)
        int[,] directions = { { 1, 0 }, { 0, 1 }, { 1, 1 }, { 1, -1 } };
        for (var i = 0; i < directions.GetLength(0); i++)
        {
            var count = 1 + CountSameCell(board, row, column, directions[i, 0], directions[i, 1], cell) +
                        CountSameCell(board, row, column, -directions[i, 0], -directions[i, 1], cell);
            if (count >= WinLength)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断棋盘是否已落满（42 格全非空）。落满且无四连即为平局。
    /// </summary>
    /// <param name="board">棋盘数组。</param>
    /// <returns>棋盘已满返回 true。</returns>
    public static bool IsBoardFull(int[] board)
    {
        for (var i = 0; i < board.Length; i++)
        {
            if (board[i] == 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 从指定格出发沿单方向统计与落点棋子同色的连续格数（不含起点自身）。
    /// </summary>
    private static int CountSameCell(int[] board, int row, int column, int rowDelta, int columnDelta, int cell)
    {
        var count = 0;
        var r = row + rowDelta;
        var c = column + columnDelta;
        while (r >= 0 && r < Height && c >= 0 && c < Width && board[r * Width + c] == cell)
        {
            count++;
            r += rowDelta;
            c += columnDelta;
        }

        return count;
    }
}
