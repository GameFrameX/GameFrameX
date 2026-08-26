// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//  
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//  
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//  
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//  
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

namespace GameFrameX.Apps.Game.Room.Entity;

public sealed class RoomListState : CacheState
{
    /// <summary>
    /// 下一个房间ID。
    /// </summary>
    public long NextRoomId { get; set; } = 1;

    /// <summary>
    /// 通用房间列表。
    /// </summary>
    public Dictionary<long, RoomState> Rooms { get; set; } = new Dictionary<long, RoomState>();

    /// <summary>
    /// 玩家当前所在房间。Key: 玩家ID, Value: 房间ID。
    /// </summary>
    public Dictionary<long, long> PlayerRoomMap { get; set; } = new Dictionary<long, long>();

    /// <summary>
    /// 房间内玩家断线时间。Key: 玩家ID, Value: 断线 Unix 秒。
    /// </summary>
    public Dictionary<long, long> DisconnectedPlayerTimeMap { get; set; } = new Dictionary<long, long>();
}

public sealed class RoomState
{
    /// <summary>
    /// 房间ID。
    /// </summary>
    public long RoomId { get; set; }

    /// <summary>
    /// 房间名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 游戏类型。
    /// </summary>
    public GameType GameType { get; set; }

    /// <summary>
    /// 当前房间状态。
    /// </summary>
    public RoomStatus Status { get; set; }

    /// <summary>
    /// 房主玩家ID。
    /// </summary>
    public long OwnerRoleId { get; set; }

    /// <summary>
    /// 房间玩家列表，列表顺序即座位顺序。
    /// </summary>
    public List<long> PlayerIds { get; set; } = new List<long>();

    /// <summary>
    /// 最少玩家数量。
    /// </summary>
    public int MinPlayerCount { get; set; }

    /// <summary>
    /// 最大玩家数量。
    /// </summary>
    public int MaxPlayerCount { get; set; }

    /// <summary>
    /// 当前局数。
    /// </summary>
    public int Round { get; set; } = 1;

    /// <summary>
    /// 创建时间。
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 最后更新时间。
    /// </summary>
    public long UpdatedTime { get; set; }
}
