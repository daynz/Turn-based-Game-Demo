using System;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.UI.Base;
using BH.UI.Enums;
using JetBrains.Annotations;
using System.Threading;
using BH.Framework.Infrastructure.Resource.Data;
using BH.Framework.Infrastructure.Resource.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace BH.UI.Managers
{
    /// <summary>
    /// UI预制体工厂
    /// </summary>
    [UsedImplicitly]
    public class UIFactory : IInitializable, IDisposable
    {
        [Inject] private readonly ILogService _logService;
        [Inject] private readonly DiContainer _container;
        [Inject] private readonly IResourceService _resourceService;
        private Transform _uiRoot;
        private CancellationTokenSource _cts;

        public void Initialize()
        {
            // 初始化取消令牌
            _cts = new CancellationTokenSource();

            // 获取或创建UI根节点
            _uiRoot = GameObject.Find("Canvas")?.transform;
            if (_uiRoot) return;
            var canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _uiRoot = canvasObj.transform;
            var canvas = canvasObj.GetComponent<Canvas>();
            //canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        /// <summary>
        /// 创建指定路径的UI（同步封装异步加载）
        /// </summary>
        /// <param name="uiPath">UI预制体Addressables路径</param>
        /// <param name="layer">UI层级</param>
        /// <returns>创建的View</returns>
        public T CreateUI<T>(string uiPath, UILayer layer = UILayer.Middle) where T : ViewBase
        {
            // 异步加载转为同步（核心UI需提前预加载，避免阻塞）
            var loadTask = _resourceService.LoadPersistentAssetAsync<GameObject>(uiPath, _cts.Token);
            loadTask.Wait(); // 同步等待，生产环境建议加超时控制

            var loadResult = loadTask.Result;
            if (loadResult.Status != ResourceLoadStatus.Success || !loadResult.Asset)
            {
                _logService.Error($"UI预制体加载失败：{uiPath}，错误信息：{loadResult.ErrorMessage}");
                return null;
            }

            // 获取缓存的预制体并实例化（注入依赖）
            var prefab = loadResult.Asset;
            var uiObj = _container.InstantiatePrefab(prefab, _uiRoot);
            var view = uiObj.GetComponent<T>();

            if (!view)
            {
                _logService.Error($"UI预制体{uiPath}缺少{typeof(T).Name}组件");
                Object.Destroy(uiObj);
                return null;
            }

            // 设置层级
            SetUILayer(uiObj.transform, layer);

            return view;
        }

        /// <summary>
        /// 异步创建UI（推荐使用，避免主线程阻塞）
        /// </summary>
        public async System.Threading.Tasks.Task<T> CreateUIAsync<T>(string uiPath, UILayer layer = UILayer.Middle)
            where T : ViewBase
        {
            var loadResult = await _resourceService.LoadPersistentAssetAsync<GameObject>(uiPath, _cts.Token);

            if (loadResult.Status != ResourceLoadStatus.Success || !loadResult.Asset)
            {
                _logService.Error($"UI预制体加载失败：{uiPath}，错误信息：{loadResult.ErrorMessage}");
                return null;
            }

            var prefab = loadResult.Asset;
            var uiObj = _container.InstantiatePrefab(prefab, _uiRoot);
            var view = uiObj.GetComponent<T>();

            if (!view)
            {
                _logService.Error($"UI预制体{uiPath}缺少{typeof(T).Name}组件");
                Object.Destroy(uiObj);
                return null;
            }

            SetUILayer(uiObj.transform, layer);
            return view;
        }

        /// <summary>
        /// 设置UI层级（通过调整父节点实现）
        /// </summary>
        private void SetUILayer(Transform uiTransform, UILayer layer)
        {
            // 创建层级父节点（如不存在）
            var layerName = $"Layer_{layer}";
            var layerParent = _uiRoot.Find(layerName);
            if (!layerParent)
            {
                layerParent = new GameObject(layerName).transform;
                layerParent.SetParent(_uiRoot);
                layerParent.localPosition = Vector3.zero;
                layerParent.localScale = Vector3.one;
            }

            uiTransform.SetParent(layerParent);
            uiTransform.localPosition = Vector3.zero;
            uiTransform.localScale = Vector3.one;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}