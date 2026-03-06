using Zenject;

namespace BH.Framework.Interfaces
{
    public interface IGameMode
    {
        void Initialize();
        void StartGame();
        void EndGame();
    }
    public class GameModeFactory : PlaceholderFactory<IGameMode> { }
}