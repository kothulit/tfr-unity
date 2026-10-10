using UnityEngine;
using VContainer.Unity;
using TFR.Input;

namespace TFR
{
    public class GamePresenter : IStartable
    {
        private readonly IGameplayInput gameplayInput;
        public GamePresenter(
            IGameplayInput gameplayInput
            )
        {
            this.gameplayInput = gameplayInput;
        }

        public void Start()
        {
            gameplayInput.GameplayEnabled = true;
        }
    }
}