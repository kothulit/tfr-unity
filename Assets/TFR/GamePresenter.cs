using UnityEngine;
using VContainer.Unity;

namespace TFR
{
    public class GamePresenter : IStartable
    {
        readonly HelloWorldService helloWorldService;
        readonly HelloScreen helloScreen;
        public GamePresenter(
            HelloWorldService helloWorldService,
            HelloScreen helloScreen
            )
        {
            this.helloWorldService = helloWorldService;
            this.helloScreen = helloScreen;
        }

        public void Start()
        {
            helloScreen.HelloButton.onClick.AddListener( ()=> helloWorldService.Hello());
        }
    }
}