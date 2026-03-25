using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Resource.Data;
using BH.Framework.Infrastructure.Resource.Interfaces;
using BH.UI.Base;
using BH.UI.Enums;
using BH.UI.Module.MainMenu.View;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BH.UI.Managers
{
    /// <summary>
    /// 全局UI管理器
    /// </summary>
    [UsedImplicitly]
    public class UIManager : MonoBehaviour, IInitializable, IDisposable
    {
        [Inject] private readonly ILogService _logService;
        [Inject] private readonly UIFactory _uiFactory;
        [Inject] private readonly IResourceService _resourceService;
        private readonly Dictionary<string, ViewBase> _uiCache = new(); // UI实例缓存（业务层）
        private readonly Stack<ViewBase> _uiStack = new(); // 页面栈

        public void Initialize()
        {
            _logService.Info("UIManager 初始化完成");
        }

        /// <summary>
        /// 打开指定UI（支持缓存）
        /// </summary>
        /// <typeparam name="T">View类型</typeparam>
        /// <param name="uiPath">UI路径</param>
        /// <param name="layer">层级</param>
        /// <param name="useCache">是否使用缓存</param>
        /// <returns>打开的View</returns>
        public T OpenUI<T>(string uiPath, UILayer layer = UILayer.Middle, bool useCache = true) where T : ViewBase
        {
            // 检查实例缓存（业务层）
            if (useCache && _uiCache.TryGetValue(uiPath, out var cachedView))
            {
                cachedView.Show();
                _uiStack.Push(cachedView);
                _logService.Info($"打开UI（缓存命中）：{uiPath}");
                return cachedView as T;
            }

            // 创建新UI（底层预制体由ResourceService缓存）
            var newView = _uiFactory.CreateUI<T>(uiPath, layer);
            if (!newView) return null;

            newView.Show();
            _uiCache[uiPath] = newView;
            _uiStack.Push(newView);
            _logService.Info($"打开UI（新建实例）：{uiPath}");

            return newView;
        }

        /// <summary>
        /// 异步打开UI（推荐使用，避免主线程阻塞）
        /// </summary>
        public async Task<T> OpenUIAsync<T>(string uiPath, UILayer layer = UILayer.Middle,
            bool useCache = true) where T : ViewBase
        {
            if (useCache && _uiCache.TryGetValue(uiPath, out var cachedView))
            {
                cachedView.Show();
                _uiStack.Push(cachedView);
                _logService.Info($"打开UI（缓存命中）：{uiPath}");
                return cachedView as T;
            }

            var newView = await _uiFactory.CreateUIAsync<T>(uiPath, layer);
            if (!newView) return null;

            newView.Show();
            _uiCache[uiPath] = newView;
            _uiStack.Push(newView);
            _logService.Info($"打开UI（新建实例）：{uiPath}");

            return newView;
        }

        /// <summary>
        /// 关闭当前最上层UI
        /// </summary>
        public void CloseCurrentUI()
        {
            if (_uiStack.Count == 0) return;

            var currentView = _uiStack.Pop();
            currentView.Hide();
            _logService.Info($"关闭UI：{currentView.name}");
        }

        /// <summary>
        /// 关闭指定UI
        /// </summary>
        public void CloseUI(string uiPath)
        {
            if (_uiCache.TryGetValue(uiPath, out var view))
            {
                view.Hide();
                // 从栈中移除（需遍历，Stack不支持直接Remove，可优化为LinkedList）
                var tempStack = new Stack<ViewBase>();
                while (_uiStack.Count > 0)
                {
                    var item = _uiStack.Pop();
                    if (item != view)
                    {
                        tempStack.Push(item);
                    }
                }

                while (tempStack.Count > 0)
                {
                    _uiStack.Push(tempStack.Pop());
                }

                _logService.Info($"关闭指定UI：{uiPath}");
            }
            else
            {
                _logService.Warning($"关闭UI失败：{uiPath} 未找到缓存实例");
            }
        }

        /// <summary>
        /// 清空所有UI
        /// </summary>
        /// <param name="releasePrefab">是否释放底层预制体缓存</param>
        public void ClearAllUI(bool releasePrefab = false)
        {
            foreach (var view in _uiCache.Values)
            {
                view.Hide();
            }

            _uiStack.Clear();
            _logService.Info("清空所有UI实例");

            // 可选：释放底层预制体缓存（根据业务需求）
            if (releasePrefab)
            {
                foreach (var uiPath in _uiCache.Keys)
                {
                    var addressablePath = $"UI/Modules/{uiPath}";
                    _resourceService.UnloadPersistentAsset(addressablePath);
                }

                _logService.Info("释放所有UI预制体缓存");
            }
        }

        /// <summary>
        /// 释放指定UI的预制体缓存（实例仍保留，仅释放预制体）
        /// </summary>
        public void ReleaseUIPrefab(string uiPath)
        {
            var addressablePath = $"UI/Modules/{uiPath}";
            if (_resourceService.IsPersistentAssetCached(addressablePath))
            {
                _resourceService.UnloadPersistentAsset(addressablePath);
                _logService.Info($"释放UI预制体缓存：{uiPath}");
            }
            else
            {
                _logService.Warning($"释放UI预制体失败：{uiPath} 未在缓存中");
            }
        }

        public void Dispose()
        {
            // 清空实例缓存
            ClearAllUI(true); // 释放实例+预制体
            _uiCache.Clear();

            // 释放工厂资源
            (_uiFactory as IDisposable)?.Dispose();

            _logService.Info("UIManager 已释放所有资源");
        }
    }
}