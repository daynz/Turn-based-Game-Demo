using System;
using BH.Framework.Infrastructure.Resource.Interfaces;
using UnityEngine;
using Zenject;

namespace BH.Framework.Infrastructure.Resource.Manager
{
    [Serializable]
    public class ResourceManager : MonoBehaviour
    {
        [Inject] private IResourceService _resourceService;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}