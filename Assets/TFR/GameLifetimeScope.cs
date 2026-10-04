using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TFR
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] HelloScreen helloScreen;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<HelloWorldService>(Lifetime.Singleton);
            builder.Register<GamePresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GamePresenter>();
            builder.RegisterComponent(helloScreen);
        }
    }
}
