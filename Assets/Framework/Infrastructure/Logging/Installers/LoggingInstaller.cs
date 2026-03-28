using System;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Logging.Output;
using UnityEngine;
using Zenject;

namespace BH.Framework.Infrastructure.Logging.Installers
{
    [Serializable]
    public class LoggingInstaller : Installer
    {
        [SerializeField] private bool enableConsoleOutput = false;
        [SerializeField] private bool enableFileOutput = false;
        [SerializeField] private string logFileName = "BH6Log.log";

        public override void InstallBindings()
        {
            if (enableConsoleOutput)
            {
                Container.BindInterfacesAndSelfTo<LogConsoleOutput>().AsSingle();
            }

            if (enableFileOutput)
            {
                Container.BindInterfacesAndSelfTo<LogFileOutput>().AsSingle()
                    .WithArguments(logFileName);
            }

            Container.BindInterfacesAndSelfTo<LogService>().AsSingle().NonLazy();
        }
    }
}