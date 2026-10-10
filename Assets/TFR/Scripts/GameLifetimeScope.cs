using VContainer;
using VContainer.Unity;
using TFR.Input;
using TFR.Player;
using TFR.Camera;

namespace TFR
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<TFRInputActions>(Lifetime.Singleton);
            builder.Register<GameplayInput>(Lifetime.Singleton).As<IGameplayInput>();
            builder.RegisterComponentInHierarchy<MotionController>();
            builder.RegisterComponentInHierarchy<CameraFollowController>();

            builder.Register<GamePresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GamePresenter>();
        }
    }
}
