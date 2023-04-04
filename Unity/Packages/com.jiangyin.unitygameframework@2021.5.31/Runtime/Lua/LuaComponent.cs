using System;
using UnityEngine;
using XLua;
using XLua.LuaDLL;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// Lua组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Lua")]
    public sealed class LuaComponent : GameFrameworkComponent
    {
        private LuaEnv _luaEnv;

        protected override void Awake()
        {
            base.Awake();
            _luaEnv = new LuaEnv();
        }

        public void DoString(string lua, string chunkName = "chunk", LuaTable env = null)
        {
            _luaEnv.DoString(lua, chunkName, env);
        }

        public void DoString(byte[] luaBytes, string chunkName = "chunk", LuaTable env = null)
        {
            _luaEnv.DoString(luaBytes, chunkName, env);
        }

        public void LoadString(string lua, string chunkName = "chunk", LuaTable env = null)
        {
            _luaEnv.LoadString(lua, chunkName, env);
        }

        public void LoadString<T>(string lua, string chunkName = "chunk", LuaTable env = null)
        {
            _luaEnv.LoadString<T>(lua, chunkName, env);
        }

        public void AddBuildIn(string buildInName, lua_CSFunction init)
        {
            _luaEnv.AddBuildin(buildInName, init);
        }

        public void AddLoader(LuaEnv.CustomLoader loader)
        {
            _luaEnv.AddLoader(loader);
        }

        public LuaTable NewTable()
        {
            return _luaEnv.NewTable();
        }

        public void Dispose()
        {
            _luaEnv.Dispose();
        }

        public LuaTable GetGlobal()
        {
            return _luaEnv.Global;
        }


        public void Tick()
        {
            _luaEnv.Tick();
        }

        public void Alias(Type type, string alias)
        {
            _luaEnv.Alias(type, alias);
        }


        public void GC()
        {
            _luaEnv.GC();
        }

        public void RestartGc()
        {
            _luaEnv.RestartGc();
        }

        public void FullGc()
        {
            _luaEnv.FullGc();
        }

        public void GcStep(int data)
        {
            _luaEnv.GcStep(data);
        }

        public void StopGc()
        {
            _luaEnv.StopGc();
        }
    }
}