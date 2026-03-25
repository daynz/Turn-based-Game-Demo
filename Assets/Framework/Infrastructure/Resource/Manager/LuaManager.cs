using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Singleton;
using XLua;

namespace BH.Framework.Infrastructure.Resource.Manager
{
    public class LuaManager : MonoSingleton<LuaManager>
    {
        public string Name => GetType().Name;
        private LuaEnv _luaEnv;
        public LuaTable Global => _luaEnv.Global;
         private LogService _logService;

        public void Init()
        {
            if (_luaEnv != null)
                return;
            _luaEnv = new LuaEnv();
            _luaEnv.AddLoader(CustomLoaderFromAssetBundle);
            // _luaEnv.AddLoader(GachaSystemCustomLoader);
            // _luaEnv.AddLoader(BattleSystemCustomLoader);
        }

        // private byte[] GachaSystemCustomLoader(ref string filePath)
        // {
        //     var path = UnityEngine.Application.dataPath + "/UISystem/Scripts/Lua/GachaSystem/" + filePath + ".lua";
        //     if (File.Exists(path))
        //     {
        //         return File.ReadAllBytes(path);
        //     }
        //
        //     LogManager.Instance.Error("GachaSystemCustomLoader重定向失败:" + path);
        //     return null;
        // }

        // private byte[] BattleSystemCustomLoader(ref string filePath)
        // {
        //     var path = UnityEngine.Application.dataPath + "/UISystem/Scripts/Lua/BattleSystem/" + filePath + ".lua";
        //     if (File.Exists(path))
        //     {
        //         return File.ReadAllBytes(path);
        //     }
        //
        //     LogManager.Instance.Error("BattleSystemCustomLoader重定向失败:" + path);
        //     return null;
        // }

        private byte[] CustomLoaderFromAssetBundle(ref string filePath)
        {
            // var lua = AssetBundleManager.Instance.LoadAsset<TextAsset>("lua", filePath + ".lua");
            // if (lua)
            //     return lua.bytes;
            // Debug.Log("CustomLoaderFromAssetBundle重定向失败" + filePath);
            return null;
        }

        public void DoString(string str)
        {
            if (_luaEnv == null)
            {
                _logService.Error("xLua未初始化", Name);
                return;
            }

            _luaEnv.DoString(str);
        }

        public void DoLuaFile(string fileName)
        {
            var str = $"require('{fileName}')";
            DoString(str);
        }

        public void Tick()
        {
            if (_luaEnv == null)
            {
                _logService.Error("xLua未初始化", Name);
                return;
            }

            _luaEnv.Tick();
        }

        public void Dispose()
        {
            if (_luaEnv == null)
            {
                _logService.Error("xLua未初始化", Name);
                return;
            }

            _luaEnv.Dispose();
            _luaEnv = null;
        }
    }
}