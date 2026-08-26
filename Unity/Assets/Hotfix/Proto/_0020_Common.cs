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

using System;
using ProtoBuf;
using System.Collections.Generic;
using GameFrameX.Network.Runtime;

namespace Hotfix.Proto
{
	/// <summary>
	/// 返回码
	/// </summary>
	public enum ResultCode
	{
		/// <summary>
		/// 成功
		/// </summary>
		Success = 0,

		/// <summary>
		/// 失败
		/// </summary>
		Failed = 1,
	}

	/// <summary>
	/// 操作错误代码（客户端与服务器之间的业务错误码）
	/// </summary>
	public enum OperationStatusCode
	{
		/// <summary>
		/// 成功
		/// </summary>
		Ok = 0,

		/// <summary>
		/// 配置表错误
		/// </summary>
		ConfigErr = 1,

		/// <summary>
		/// 客户端传递参数错误
		/// </summary>
		ParamErr = 2,

		/// <summary>
		/// 消耗不足
		/// </summary>
		CostNotEnough = 3,

		/// <summary>
		/// 未开通服务
		/// </summary>
		Forbidden = 4,

		/// <summary>
		/// 不存在
		/// </summary>
		NotFound = 5,

		/// <summary>
		/// 已经存在
		/// </summary>
		HasExist = 6,

		/// <summary>
		/// 账号不存在或为空
		/// </summary>
		AccountCannotBeNull = 7,

		/// <summary>
		/// 无法执行数据库修改
		/// </summary>
		Unprocessable = 8,

		/// <summary>
		/// 未知平台
		/// </summary>
		UnknownPlatform = 9,

		/// <summary>
		/// 正常通知
		/// </summary>
		Notice = 10,

		/// <summary>
		/// 功能未开启，主消息屏蔽
		/// </summary>
		FuncNotOpen = 11,

		/// <summary>
		/// 其他
		/// </summary>
		Other = 12,

		/// <summary>
		/// 内部服务错误
		/// </summary>
		InternalServerError = 13,

		/// <summary>
		/// 通知客户端服务器人数已达上限
		/// </summary>
		ServerFullyLoaded = 14,

		/// <summary>
		/// 不支持的操作（如未实现的奖励类型路由）
		/// </summary>
		Unsupported = 15,

		/// <summary>
		/// 奖励参数非法
		/// </summary>
		InvalidReward = 16,

		/// <summary>
		/// 部分成功（多项发放中仅部分成功）
		/// </summary>
		PartialSuccess = 17,
	}

}
