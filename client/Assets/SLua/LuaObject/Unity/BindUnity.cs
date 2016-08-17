using System;
using System.Collections.Generic;
namespace SLua {
	[LuaBinder(0)]
	public class BindUnity {
		public static Action<IntPtr>[] GetBindList() {
			Action<IntPtr>[] list= {
				Lua_UnityEngine_GameObject.reg,
			};
			return list;
		}
	}
}
