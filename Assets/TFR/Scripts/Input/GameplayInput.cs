using System;
using UnityEngine;
namespace TFR.Input
{
    /// <summary>
    /// Чтение карты Gameplay.
    /// Пока GameplayEnabled выключен, оси движения и взгляда возвращают ноль.
    /// </summary>
    public interface IGameplayInput
    {
        /// <summary>
        /// Стик движения: x — вбок, y — вперёд.
        /// Тот же вектор используется как сырой перенос веса.
        /// </summary>
        Vector2 Move { get; }
        Vector2 LookDelta { get; }
        bool PausePressed { get; }
        bool DebugPressed { get; }
        bool GameplayEnabled { get; set; }
        bool SprintHeld { get; }
        bool InteractPressed { get; }
        bool MountPressed { get; }
    }

    /// <summary>
    /// Читает действия карты Gameplay из сгенерированного TFRInputActions.
    /// </summary>
    public sealed class GameplayInput : IGameplayInput, IDisposable
    {
        readonly TFRInputActions actions;
        public GameplayInput(TFRInputActions actions)
        {
            this.actions = actions;
            actions.Gameplay.Enable();
        }

        public Vector2 Move => GameplayEnabled ? actions.Gameplay.Move.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 LookDelta => GameplayEnabled ? actions.Gameplay.Look.ReadValue<Vector2>() : Vector2.zero;
        public bool PausePressed => actions.Gameplay.Pause.WasPressedThisFrame();
        public bool DebugPressed => actions.Gameplay.DebugHud.WasPressedThisFrame();
        public bool GameplayEnabled { get; set; }
        public bool SprintHeld => GameplayEnabled && actions.Gameplay.Sprint.IsPressed();
        public bool InteractPressed => GameplayEnabled && actions.Gameplay.Interact.WasPressedThisFrame();
        public bool MountPressed => GameplayEnabled && actions.Gameplay.Mount.WasPressedThisFrame();

        public void Dispose()
        {
            actions.Gameplay.Disable();
        }
    }
}