using System;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Logging.Output;
using UnityEngine;
using Zenject;

namespace BH.Framework.Infrastructure.Logging.Installers
{
    [Serializable]
    public class LoggingInstaller : Installer<LoggingInstaller>
    {
        [SerializeField] private bool enableConsoleOutput = true;
        [SerializeField] private bool enableFileOutput = true;
        [SerializeField] private string logFileName = "BH6Log.log";

        public override void InstallBindings()
        {
            if (enableConsoleOutput)
            {
                Container.BindInterfacesAndSelfTo<LogConsoleOutput>().AsSingle();
            }

            if (enableFileOutput)
            {
                // 可以传递参数给 LogFileOutput 的构造函数
                Container.BindInterfacesAndSelfTo<LogFileOutput>().AsSingle()
                    .WithArguments(logFileName);
            }

            Container.BindInterfacesAndSelfTo<LogService>().AsSingle();
            Container.Bind<ILogService>().To<LogService>().AsSingle();
        }
    }
}