using System;
using BH.Framework.Infrastructure.Resource.Enums;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace BH.Framework.Infrastructure.Resource.Data
{
    /// <summary>
    /// 场景加载结果对象
    /// </summary>
    /// <typeparam name="T">结果类型（SceneInstance/布尔值等）</typeparam>
    public class SceneOperationResult<T>
    {
        /// <summary>操作是否成功</summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>场景逻辑名称</summary>
        public string SceneLogicalName { get; set; }
        
        /// <summary>操作结果数据</summary>
        public T Data { get; set; }
        
        /// <summary>错误信息（失败时非空）</summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>异常对象（失败时非空）</summary>
        public Exception Exception { get; set; }
        
        /// <summary>操作状态</summary>
        public SceneLoadStatus Status { get; set; }

        public static SceneOperationResult<T> Failure(string errorMsg, Exception resultException = null)
        {
            throw new NotImplementedException();
        }

        public static SceneOperationResult<T> Success(T resultAsset)
        {
            throw new NotImplementedException();
        }

        public static SceneOperationResult<T> Cancelled()
        {
            throw new NotImplementedException();
        }
    }
}