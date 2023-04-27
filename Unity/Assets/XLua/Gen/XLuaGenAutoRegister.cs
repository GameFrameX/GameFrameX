#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;


namespace XLua.CSObjectWrap
{
    public class XLua_Gen_Initer_Register__
	{
        
        
        
        
        
        
        public static void Init(LuaEnv luaenv, ObjectTranslator translator)
        {
            
            
        }
	}
}

namespace XLua
{
	internal partial class InternalGlobals_Gen
    {
	    
        private delegate bool TryArrayGet(Type type, RealStatePtr L, ObjectTranslator translator, object obj, int index);
        private delegate bool TryArraySet(Type type, RealStatePtr L, ObjectTranslator translator, object obj, int array_idx, int obj_idx);
	    private static void Init(
            out Dictionary<Type, IEnumerable<MethodInfo>> extensionMethodMap,
            out TryArrayGet genTryArrayGetPtr,
            out TryArraySet genTryArraySetPtr)
		{
            XLua.LuaEnv.AddIniter(XLua.CSObjectWrap.XLua_Gen_Initer_Register__.Init);
            XLua.LuaEnv.AddIniter(XLua.ObjectTranslator_Gen.Init);
		    extensionMethodMap = new Dictionary<Type, IEnumerable<MethodInfo>>()
			{
			    
			};
			
            genTryArrayGetPtr = StaticLuaCallbacks_Wrap.__tryArrayGet;
            genTryArraySetPtr = StaticLuaCallbacks_Wrap.__tryArraySet;
		}
	}
}
