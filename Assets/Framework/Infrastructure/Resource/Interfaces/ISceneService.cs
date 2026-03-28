using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Resource.Data;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;

namespace BH.Framework.Infrastructure.Resource.Interfaces
{
    /// <summary>
    /// 场景管理服务接口
    /// </summary>
    public interface ISceneService : IInitializable ,IDisposable
    {
        /// <summary>
        /// 异步加载场景（默认不激活）
        /// </summary>
        /// <param name="sceneName">场景地址</param>
        /// <param name="loadMode">加载模式</param>
        /// <returns>场景加载结果</returns>
        Task<ResourceLoadResult<SceneInstance>> LoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Additive);

        /// <summary>
        /// 异步加载并激活场景
        /// </summary>
        /// <param name="sceneName">场景地址</param>
        /// <param name="loadMode">加载模式</param>
        /// <returns>场景加载结果</returns>
        Task<ResourceLoadResult<SceneInstance>> LoadAndActivateSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Additive);

        /// <summary>
        /// 异步卸载场景
        /// </summary>
        /// <param name="sceneName">场景地址</param>
        Task UnloadSceneAsync(string sceneName);

        /// <summary>
        /// 判断场景是否已加载
        /// </summary>
        /// <param name="sceneName">场景地址</param>
        /// <returns>是否加载且有效</returns>
        bool IsSceneLoaded(string sceneName);

        /// <summary>
        /// 获取当前缓存中的场景列表
        /// </summary>
        /// <returns>场景名称列表</returns>
        List<string> GetScenesCache();

        /// <summary>
        /// 清理无效的场景句柄
        /// </summary>
        void CleanInvalidHandles();
    }
}