using UnityEngine;
#if XLUA
using System;
using GameFrameX.Lua;
using XLua;
using XLua.LuaDLL;

#endif
namespace GameFrameX.Runtime
{
    /// <summary>
    /// Lua组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Lua")]
    public sealed class LuaComponent : GameFrameworkComponent
    {
        
#if XLUA
        private ILuaManager _luaManager;

        protected override void Awake()
        {
            base.Awake();
            new LuaManager();
            _luaManager = GameFrameworkEntry.GetModule<ILuaManager>();
            if (_luaManager == null)
            {
                Log.Fatal("Lua manager is invalid.");
                return;
            }

            _luaManager.InitLuaEnv(new LuaEnv());
        }

        /// <summary>
        /// 执行Lua代码字符串
        /// </summary>
        /// <param name="lua">Lua代码字符串</param>
        /// <param name="chunkName">代码块名称</param>
        /// <param name="env">Lua环境表</param>
        /// <returns>执行结果</returns>
        public object[] DoString(string lua, string chunkName = "chunk", LuaTable env = null)
        {
            return _luaManager.DoString(lua, chunkName, env);
        }

        /// <summary>
        /// 执行Lua字节数组
        /// </summary>
        /// <param name="luaBytes">Lua字节数组</param>
        /// <param name="chunkName">代码块名称</param>
        /// <param name="env">Lua环境表</param>
        /// <returns>执行结果</returns>
        public object[] DoString(byte[] luaBytes, string chunkName = "chunk", LuaTable env = null)
        {
            return _luaManager.DoString(luaBytes, chunkName, env);
        }

        /// <summary>
        /// 加载Lua代码字符串
        /// </summary>
        /// <param name="lua">Lua代码字符串</param>
        /// <param name="chunkName">代码块名称</param>
        /// <param name="env">Lua环境表</param>
        /// <returns>加载的Lua函数</returns>
        public LuaFunction LoadString(string lua, string chunkName = "chunk", LuaTable env = null)
        {
            return _luaManager.LoadString(lua, chunkName, env);
        }

        /// <summary>
        /// 加载Lua代码字符串并返回指定类型
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="lua">Lua代码字符串</param>
        /// <param name="chunkName">代码块名称</param>
        /// <param name="env">Lua环境表</param>
        /// <returns>加载的Lua函数</returns>
        public T LoadString<T>(string lua, string chunkName = "chunk", LuaTable env = null)
        {
            return _luaManager.LoadString<T>(lua, chunkName, env);
        }

        /// <summary>
        /// 添加内置函数
        /// </summary>
        /// <param name="buildInName">内置函数名称</param>
        /// <param name="init">初始化函数</param>
        public void AddBuildIn(string buildInName, lua_CSFunction init)
        {
            _luaManager.AddBuildIn(buildInName, init);
        }

        /// <summary>
        /// 添加自定义加载器
        /// </summary>
        /// <param name="loader">自定义加载器</param>
        public void AddLoader(LuaEnv.CustomLoader loader)
        {
            _luaManager.AddLoader(loader);
        }

        /// <summary>
        /// 创建新的Lua表
        /// </summary>
        /// <returns>新的Lua表</returns>
        public LuaTable NewTable()
        {
            return _luaManager.NewTable();
        }

        /// <summary>
        /// 释放Lua环境
        /// </summary>
        public void Dispose()
        {
            _luaManager.Dispose();
        }

        /// <summary>
        /// 获取全局Lua表
        /// </summary>
        /// <returns>全局Lua表</returns>
        public LuaTable GetGlobal()
        {
            return _luaManager.GetGlobal();
        }

        /// <summary>
        /// 进行一次Lua垃圾回收
        /// </summary>
        public void Tick()
        {
            _luaManager.Tick();
        }

        /// <summary>
        /// 为指定类型添加别名
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <param name="alias">类型别名</param>
        public void Alias(Type type, string alias)
        {
            _luaManager.Alias(type, alias);
        }

        /// <summary>
        /// 执行一次Lua垃圾回收
        /// </summary>
        public void GC()
        {
            _luaManager.GC();
        }

        /// <summary>
        /// 重新启动Lua垃圾回收
        /// </summary>
        public void RestartGc()
        {
            _luaManager.RestartGc();
        }

        /// <summary>
        /// 进行一次完整Lua垃圾回收
        /// </summary>
        public void FullGc()
        {
            _luaManager.FullGc();
        }

        /// <summary>
        /// 执行Lua垃圾回收一步
        /// </summary>
        /// <param name="data">回收数据</param>
        public void GcStep(int data)
        {
            _luaManager.GcStep(data);
        }

        /// <summary>
        /// 停止Lua垃圾回收
        /// </summary>
        public void StopGc()
        {
            _luaManager.StopGc();
        }
#endif
    }
}