using UnityEngine;
using VContainer.Unity;

namespace TFR
{
    public class GamePresenter : IStartable
    {
        readonly HelloWorldService helloWorldService;
        readonly HelloScreen helloScreen;

        readonly DemoBoardCreator demoBoardCreator;
        readonly DemoBoard demoBoard;
        public GamePresenter(
            HelloWorldService helloWorldService,
            HelloScreen helloScreen,
            DemoBoardCreator demoBoardCreator
            )
        {
            this.helloWorldService = helloWorldService;
            this.helloScreen = helloScreen;
            this.demoBoardCreator = demoBoardCreator;
        }

        public void Start()
        {
            helloScreen.HelloButton.onClick.AddListener( ()=> helloWorldService.Hello());
            demoBoardCreator.CreateGameObject();
        }
    }
}