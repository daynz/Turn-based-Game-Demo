using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Resource.Data;
using BH.Framework.Infrastructure.Resource.Enums;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;

namespace BH.Framework.Infrastructure.Resource.Interfaces
{
    /// <summary>
    /// 场景服务接口
    /// </summary>
    public interface ISceneService : IInitializable, IDisposable
    {
        #region 基础场景操作

        /// <summary>
        /// 异步加载场景（支持过渡动画、依赖加载、进度回调）
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称（业务标识）</param>
        /// <param name="sceneAddress">场景地址（AssetKeys 常量）</param>
        /// <param name="loadMode">加载模式（默认叠加）</param>
        /// <param name="autoActivate">是否自动激活（默认 true）</param>
        /// <param name="transitionParams">过渡参数（默认使用全局配置）</param>
        /// <param name="dependencies">场景依赖资源列表</param>
        /// <param name="onProgress">加载进度回调（0-1）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>场景加载结果</returns>
        Task<SceneOperationResult<SceneInstance>> LoadSceneAsync(
            string sceneLogicalName,
            string sceneAddress,
            LoadSceneMode loadMode = LoadSceneMode.Additive,
            bool autoActivate = true,
            SceneTransitionParams transitionParams = null,
            List<SceneDependency> dependencies = null,
            Action<float> onProgress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步卸载场景（支持过渡动画、依赖清理）
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <param name="forceUnload">是否强制卸载（忽略引用计数）</param>
        /// <param name="cleanDependencies">是否清理场景依赖资源</param>
        /// <param name="transitionParams">过渡参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>卸载结果</returns>
        Task<SceneOperationResult<bool>> UnloadSceneAsync(
            string sceneLogicalName,
            bool forceUnload = false,
            bool cleanDependencies = true,
            SceneTransitionParams transitionParams = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 激活已加载的场景
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <param name="transitionParams">过渡参数</param>
        /// <returns>激活结果</returns>
        Task<SceneOperationResult<bool>> ActivateSceneAsync(
            string sceneLogicalName,
            SceneTransitionParams transitionParams = null);

        /// <summary>
        /// 检查场景当前状态
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <returns>场景状态</returns>
        SceneLoadStatus GetSceneStatus(string sceneLogicalName);

        /// <summary>
        /// 获取已加载的所有场景逻辑名称
        /// </summary>
        /// <returns>场景名称列表</returns>
        List<string> GetLoadedScenes();

        /// <summary>
        /// 检查场景是否已加载（包括未激活状态）
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <returns>是否已加载</returns>
        bool IsSceneLoaded(string sceneLogicalName);

        /// <summary>
        /// 检查场景是否已激活
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <returns>是否已激活</returns>
        bool IsSceneActivated(string sceneLogicalName);

        #endregion

        #region 场景预加载

        /// <summary>
        /// 预加载场景（后台加载，不激活）
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <param name="sceneAddress">场景地址</param>
        /// <param name="onProgress">预加载进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>预加载结果</returns>
        Task<SceneOperationResult<bool>> PreloadSceneAsync(
            string sceneLogicalName,
            string sceneAddress,
            Action<float> onProgress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 取消场景预加载
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <returns>取消结果</returns>
        bool CancelPreloadScene(string sceneLogicalName);

        /// <summary>
        /// 检查场景是否已预加载
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <returns>是否已预加载</returns>
        bool IsScenePreloaded(string sceneLogicalName);

        /// <summary>
        /// 清理所有未激活的预加载场景
        /// </summary>
        /// <returns>清理的场景数量</returns>
        int CleanPreloadedScenes();

        #endregion

        #region 场景栈管理

        /// <summary>
        /// 将场景入栈（用于返回上一场景）
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <param name="overrideIfExists">如果已存在是否覆盖</param>
        void PushSceneToStack(string sceneLogicalName, bool overrideIfExists = false);

        /// <summary>
        /// 将场景出栈（移除栈顶场景）
        /// </summary>
        /// <returns>出栈的场景名称，栈空返回 null</returns>
        string PopSceneFromStack();

        /// <summary>
        /// 返回上一场景（出栈并加载）
        /// </summary>
        /// <param name="unloadCurrent">是否卸载当前激活场景</param>
        /// <param name="transitionParams">过渡参数</param>
        /// <returns>返回结果</returns>
        Task<SceneOperationResult<SceneInstance>> GoBackToPreviousSceneAsync(
            bool unloadCurrent = true,
            SceneTransitionParams transitionParams = null);

        /// <summary>
        /// 获取场景栈顶元素
        /// </summary>
        /// <returns>栈顶场景名称，栈空返回 null</returns>
        string GetTopSceneInStack();

        /// <summary>
        /// 清空场景栈
        /// </summary>
        void ClearSceneStack();

        /// <summary>
        /// 获取场景栈长度
        /// </summary>
        /// <returns>栈元素数量</returns>
        int GetSceneStackCount();

        #endregion

        #region 常驻场景管理

        /// <summary>
        /// 标记场景为常驻（不会被自动卸载）
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        void MarkSceneAsPersistent(string sceneLogicalName);

        /// <summary>
        /// 取消场景的常驻标记
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        void UnmarkSceneAsPersistent(string sceneLogicalName);

        /// <summary>
        /// 检查场景是否为常驻场景
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <returns>是否为常驻</returns>
        bool IsScenePersistent(string sceneLogicalName);

        /// <summary>
        /// 获取所有常驻场景名称
        /// </summary>
        /// <returns>常驻场景列表</returns>
        List<string> GetPersistentScenes();

        #endregion

        #region 批量场景操作

        /// <summary>
        /// 批量加载场景
        /// </summary>
        /// <param name="sceneInfos">场景信息（逻辑名称+地址）</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="onSceneLoaded">单个场景加载完成回调</param>
        /// <param name="onTotalProgress">总进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>批量加载结果</returns>
        Task<SceneOperationResult<List<SceneInstance>>> LoadScenesBatchAsync(
            Dictionary<string, string> sceneInfos,
            LoadSceneMode loadMode = LoadSceneMode.Additive,
            Action<string, SceneLoadStatus> onSceneLoaded = null,
            Action<float> onTotalProgress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量卸载场景（跳过常驻场景）
        /// </summary>
        /// <param name="sceneLogicalNames">场景逻辑名称列表</param>
        /// <param name="forceUnloadPersistent">是否强制卸载常驻场景</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>成功卸载的数量</returns>
        Task<int> UnloadScenesBatchAsync(
            List<string> sceneLogicalNames,
            bool forceUnloadPersistent = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 等待所有场景加载完成
        /// </summary>
        /// <param name="timeout">超时时间（秒），默认无限等待</param>
        /// <returns>是否全部加载完成</returns>
        Task<bool> WaitForAllScenesLoadedAsync(float timeout = -1);

        #endregion
    }
}