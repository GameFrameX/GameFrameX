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

using GameFrameX.Hotfix.Logic.Game.ConnectFour;
using Xunit;

namespace GameFrameX.Hotfix.Tests;

/// <summary>
/// 四子棋对局规则判定测试。棋盘行主序 index = row*7+column，row 0 为底行。
/// </summary>
public sealed class ConnectFourRulesTests
{
    private static int Index(int row, int column)
    {
        return row * ConnectFourBoardLogic.Width + column;
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    public void IsValidColumn_OutOfRange_ShouldReturnFalse(int column)
    {
        Assert.False(ConnectFourBoardLogic.IsValidColumn(column));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void IsValidColumn_InRange_ShouldReturnTrue(int column)
    {
        Assert.True(ConnectFourBoardLogic.IsValidColumn(column));
    }

    [Fact]
    public void FindDropRow_EmptyColumn_ShouldReturnBottomRow()
    {
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        Assert.Equal(0, ConnectFourBoardLogic.FindDropRow(board, 3));
    }

    [Fact]
    public void FindDropRow_PartiallyFilledColumn_ShouldReturnLowestEmptyRow()
    {
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        board[Index(0, 3)] = 1;
        board[Index(1, 3)] = 2;
        Assert.Equal(2, ConnectFourBoardLogic.FindDropRow(board, 3));
    }

    [Fact]
    public void FindDropRow_FullColumn_ShouldReturnNegativeOne()
    {
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        for (var row = 0; row < ConnectFourBoardLogic.Height; row++)
        {
            board[Index(row, 3)] = row % 2 + 1;
        }

        Assert.Equal(-1, ConnectFourBoardLogic.FindDropRow(board, 3));
    }

    [Fact]
    public void CheckWin_HorizontalFour_ShouldReturnTrue()
    {
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        board[Index(0, 0)] = 1;
        board[Index(0, 1)] = 1;
        board[Index(0, 2)] = 1;
        board[Index(0, 3)] = 1;

        Assert.True(ConnectFourBoardLogic.CheckWin(board, 0, 3));
    }

    [Fact]
    public void CheckWin_VerticalFour_ShouldReturnTrue()
    {
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        board[Index(0, 2)] = 2;
        board[Index(1, 2)] = 2;
        board[Index(2, 2)] = 2;
        board[Index(3, 2)] = 2;

        Assert.True(ConnectFourBoardLogic.CheckWin(board, 3, 2));
    }

    [Fact]
    public void CheckWin_MainDiagonalFour_ShouldReturnTrue()
    {
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        board[Index(0, 0)] = 1;
        board[Index(1, 1)] = 1;
        board[Index(2, 2)] = 1;
        board[Index(3, 3)] = 1;

        Assert.True(ConnectFourBoardLogic.CheckWin(board, 3, 3));
    }

    [Fact]
    public void CheckWin_AntiDiagonalFour_ShouldReturnTrue()
    {
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        board[Index(0, 3)] = 2;
        board[Index(1, 2)] = 2;
        board[Index(2, 1)] = 2;
        board[Index(3, 0)] = 2;

        Assert.True(ConnectFourBoardLogic.CheckWin(board, 3, 0));
    }

    [Fact]
    public void CheckWin_WinAcrossBoundaryColumn_ShouldNotThrowAndReturnTrue()
    {
        // 边界列（0 与 6）上的横向四连：判定不得越界。
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        board[Index(5, 3)] = 1;
        board[Index(5, 4)] = 1;
        board[Index(5, 5)] = 1;
        board[Index(5, 6)] = 1;

        Assert.True(ConnectFourBoardLogic.CheckWin(board, 5, 6));
    }

    [Fact]
    public void CheckWin_ThreeInLine_ShouldReturnFalse()
    {
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        board[Index(0, 0)] = 1;
        board[Index(0, 1)] = 1;
        board[Index(0, 2)] = 1;

        Assert.False(ConnectFourBoardLogic.CheckWin(board, 0, 2));
    }

    [Fact]
    public void CheckWin_InterruptedByOpponent_ShouldReturnFalse()
    {
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        board[Index(0, 0)] = 1;
        board[Index(0, 1)] = 1;
        board[Index(0, 2)] = 2;
        board[Index(0, 3)] = 1;
        board[Index(0, 4)] = 1;

        Assert.False(ConnectFourBoardLogic.CheckWin(board, 0, 4));
    }

    [Fact]
    public void IsBoardFull_WithEmptyCell_ShouldReturnFalse()
    {
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        Assert.False(ConnectFourBoardLogic.IsBoardFull(board));
    }

    [Fact]
    public void IsBoardFull_CompletelyFilled_ShouldReturnTrue()
    {
        var board = new int[ConnectFourBoardLogic.Width * ConnectFourBoardLogic.Height];
        for (var i = 0; i < board.Length; i++)
        {
            board[i] = i % 2 + 1;
        }

        Assert.True(ConnectFourBoardLogic.IsBoardFull(board));
    }
}
