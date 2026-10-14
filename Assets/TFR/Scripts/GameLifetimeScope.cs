using VContainer;
using VContainer.Unity;
using UnityEngine;
using TFR.Config;
using TFR.Input;
using TFR.Player;
using TFR.Camera;

namespace TFR
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] WeightControlConfig weightConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<TFRInputActions>(Lifetime.Singleton);
            builder.Register<GameplayInput>(Lifetime.Singleton).As<IGameplayInput>();
            builder.RegisterInstance(weightConfig);
            builder.Register<WeightController>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<MotionController>();
            builder.RegisterComponentInHierarchy<CameraFollowController>();

            builder.Register<GamePresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GamePresenter>();
        }
    }
}