using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TFR
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] HelloScreen helloScreen;
        [SerializeField] DemoBoard demoBoard;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputSystem_Actions>(Lifetime.Singleton);

            builder.RegisterComponent(demoBoard);
            builder.Register<DemoBoardCreator>(Lifetime.Singleton);

            builder.Register<HelloWorldService>(Lifetime.Singleton);
            builder.Register<GamePresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GamePresenter>();
            builder.RegisterComponent(helloScreen);

        }
    }
}
