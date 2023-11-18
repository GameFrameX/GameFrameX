using System;
using XLua;
using XLua.LuaDLL;

namespace GameFrameX.Lua
{
    /// <summary>
    /// Lua管理器接口
    /// </summary>
    public interface ILuaManager
    {
        /// <summary>
        /// 设置LUA环境
        /// </summary>
        /// <param name="luaEnv">Lua环境</param>
        void InitLuaEnv(LuaEnv luaEnv);
        
        /// <summary>
        /// 执行Lua代码字符串
        /// </summary>
        /// <param name="lua">Lua代码字符串</param>
        /// <param name="chunkName">代码块名称</param>
        /// <param name="env">Lua环境表</param>
        /// <returns>执行结果</returns>
        object[] DoString(string lua, string chunkName = "chunk", LuaTable env = null);

        /// <summary>
        /// 执行Lua字节数组
        /// </summary>
        /// <param name="luaBytes">Lua字节数组</param>
        /// <param name="chunkName">代码块名称</param>
        /// <param name="env">Lua环境表</param>
        /// <returns>执行结果</returns>
        object[] DoString(byte[] luaBytes, string chunkName = "chunk", LuaTable env = null);

        /// <summary>
        /// 加载Lua代码字符串
        /// </summary>
        /// <param name="lua">Lua代码字符串</param>
        /// <param name="chunkName">代码块名称</param>
        /// <param name="env">Lua环境表</param>
        /// <returns>加载的Lua函数</returns>
        LuaFunction LoadString(string lua, string chunkName = "chunk", LuaTable env = null);

        /// <summary>
        /// 加载Lua代码字符串并返回指定类型
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="lua">Lua代码字符串</param>
        /// <param name="chunkName">代码块名称</param>
        /// <param name="env">Lua环境表</param>
        /// <returns>加载的Lua函数</returns>
        T LoadString<T>(string lua, string chunkName = "chunk", LuaTable env = null);

        /// <summary>
        /// 添加内置函数
        /// </summary>
        /// <param name="buildInName">内置函数名称</param>
        /// <param name="init">初始化函数</param>
        void AddBuildIn(string buildInName, lua_CSFunction init);

        /// <summary>
        /// 添加自定义加载器
        /// </summary>
        /// <param name="loader">自定义加载器</param>
        void AddLoader(LuaEnv.CustomLoader loader);

        /// <summary>
        /// 创建新的Lua表
        /// </summary>
        /// <returns>新的Lua表</returns>
        LuaTable NewTable();

        /// <summary>
        /// 释放Lua环境
        /// </summary>
        void Dispose();

        /// <summary>
        /// 获取全局Lua表
        /// </summary>
        /// <returns>全局Lua表</returns>
        LuaTable GetGlobal();

        /// <summary>
        /// 进行一次Lua垃圾回收
        /// </summary>
        void Tick();

        /// <summary>
        /// 为指定类型添加别名
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <param name="alias">类型别名</param>
        void Alias(Type type, string alias);

        /// <summary>
        /// 执行一次Lua垃圾回收
        /// </summary>
        void GC();

        /// <summary>
        /// 重新启动Lua垃圾回收
        /// </summary>
        void RestartGc();

        /// <summary>
        /// 进行一次完整Lua垃圾回收
        /// </summary>
        void FullGc();

        /// <summary>
        /// 执行Lua垃圾回收一步
        /// </summary>
        /// <param name="data">回收数据</param>
        void GcStep(int data);

        /// <summary>
        /// 停止Lua垃圾回收
        /// </summary>
        void StopGc();
    }
}