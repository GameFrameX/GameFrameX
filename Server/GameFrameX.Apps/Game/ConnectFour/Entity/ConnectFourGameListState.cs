// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and related international regulations.
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


namespace GameFrameX.Apps.Game.ConnectFour.Entity;

public sealed class ConnectFourGameListState : CacheState
{
    /// <summary>
    /// Key: 房间ID，Value: 四子棋玩法数据。
    /// </summary>
    public Dictionary<long, ConnectFourGameState> Games { get; set; } = new Dictionary<long, ConnectFourGameState>();
}

public sealed class ConnectFourGameState
{
    /// <summary>
    /// 棋盘宽度（列数）。
    /// </summary>
    public const int BoardWidth = 7;

    /// <summary>
    /// 棋盘高度（行数）。
    /// </summary>
    public const int BoardHeight = 6;

    /// <summary>
    /// 棋盘格子总数（7 列 × 6 行）。
    /// </summary>
    public const int BoardSize = BoardWidth * BoardHeight;

    /// <summary>
    /// 房间ID。
    /// </summary>
    public long RoomId { get; set; }

    /// <summary>
    /// 当前局数。
    /// </summary>
    public int Round { get; set; } = 1;

    /// <summary>
    /// 棋盘 42 格，行主序 index = row*7+column，row 0 为底行；0 空、1/2 为座位号。
    /// </summary>
    public int[] Board { get; set; } = new int[BoardSize];

    /// <summary>
    /// 当前回合玩家ID。开局为首个玩家；对局结束（分胜负或平局）后为0。
    /// </summary>
    public long CurrentPlayerId { get; set; }

    /// <summary>
    /// 胜利玩家ID。未分出胜负为0。
    /// </summary>
    public long WinnerRoleId { get; set; }

    /// <summary>
    /// 是否平局（棋盘落满且无四连）。
    /// </summary>
    public bool IsDraw { get; set; }

    /// <summary>
    /// 已落子总数。
    /// </summary>
    public int MoveCount { get; set; }

    /// <summary>
    /// 最近一次落子列（0-6）。无落子为-1。
    /// </summary>
    public int LastMoveColumn { get; set; } = -1;

    /// <summary>
    /// 最近一次落子行（0-5，0为底行）。无落子为-1。
    /// </summary>
    public int LastMoveRow { get; set; } = -1;
}
