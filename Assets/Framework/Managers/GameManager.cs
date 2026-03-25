using System;
using BH.Framework.Services;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BH.Framework.Managers
{
    [Serializable]
    [UsedImplicitly]
    public class GameManager : MonoBehaviour
    {
        [Inject] private GameService _gameService;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void OnEnable()
        {
            _gameService.Enable();
        }

        private void OnDisable()
        {
            _gameService.Disable();
        }

        private void OnDestroy()
        {
            _gameService?.Dispose();
        }
    }
}