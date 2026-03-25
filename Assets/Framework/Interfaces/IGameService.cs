using System;
using Zenject;

namespace BH.Framework.Interfaces
{
    public interface IGameService : IInitializable, IDisposable
    {
        void Enable();
        void Disable();
    }
}