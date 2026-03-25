using System;
using BH.Framework.Infrastructure.Resource.Services;
using UnityEngine;
using Zenject;

namespace BH.Framework.Infrastructure.Resource.Manager
{
    [Serializable]
    public class SceneManager : MonoBehaviour
    {
        [Inject] private SceneService _sceneService;

        
    }
}