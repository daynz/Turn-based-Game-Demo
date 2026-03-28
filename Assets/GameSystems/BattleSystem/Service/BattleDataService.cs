using System;
using System.Collections.Generic;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Resource.Services;
using BH.Framework.Interfaces;
using BH.Framework.Singleton;

namespace BH.GameSystems.BattleSystem.Service
{
    public class BattleDataService : CSharpSingleton<BattleDataService>//, IGameService
    {
        private Dictionary<string, IConfig> _configsCache;
        private ResourceService _resourceService;
        private LogService _logService;

        public Dictionary<string, IConfig> ConfigsCache => _configsCache;

        public void Init()
        {
            try
            {
                _logService.Info($"战斗配置加载完成");
            }
            catch (Exception e)
            {
                _logService.Error($"战斗配置加载失败");
            }
        }

        // public async Task<bool> LoadAllConfigsAsync()
        // {
        //     
        // }

        // private async Task<bool> LoadAllConfigureAsync()
        // {
        //     
        // }
    }
}