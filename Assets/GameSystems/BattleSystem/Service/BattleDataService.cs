using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Resource.Services;
using BH.Framework.Interfaces;
using BH.Framework.Services;
using BH.Framework.Singleton;
using BH.GameSystems.BattleSystem.Config;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Service
{
    public class BattleDataService : CSharpSingleton<BattleDataService>, IGameService
    {
        public string Name => GetType().Name;
        private Dictionary<string, IConfig> _configsCache;
        private ResourceService _resourceService;
        [Inject] private LogService _logService;

        public Dictionary<string, IConfig> ConfigsCache => _configsCache;

        public async void Init()
        {
            try
            {
                //_resourceService = ResourceService.Instance;
                var isSuccess = await LoadAllConfigsAsync();
                _logService.Info($"战斗配置加载完成", Name);
            }
            catch (Exception e)
            {
                _logService.Error($"战斗配置加载失败", Name);
            }
        }

        public async Task<bool> LoadAllConfigsAsync()
        {
            Debug.Log($"开始加载所有配置文件");

            var allLoaded = true;

            // Load each type of config
            allLoaded &= await LoadAllConfigureAsync();

            Debug.Log(allLoaded ? $"所有配置文件加载成功。" : $"部分配置文件加载失败。");

            return allLoaded;
        }

        private async Task<bool> LoadAllConfigureAsync()
        {
            const string battleConfigPath = "BattleConfig";
            var battleConfig = await _resourceService.LoadJsonConfigAsync<BattleConfig>(battleConfigPath);
            if (battleConfig)
            {
                _configsCache.Clear();
                _configsCache[battleConfigPath] = battleConfig;
                return true;
            }
            else
            {
                Debug.LogError($"加载战斗配置失败 ({battleConfigPath})。");
                return false;
            }
        }
    }
}