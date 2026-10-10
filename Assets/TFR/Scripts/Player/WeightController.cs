using TFR.Config;
using UnityEngine;

namespace TFR.Player
{
    /// <summary>
    /// Сглаживает перенос веса по двум осям и ограничивает общий сдвиг.
    /// </summary>
    public sealed class WeightController
    {
        readonly WeightControlConfig config;
        // Вдоль доски: нос и хвост.
        readonly DigitalAxisSmoother ends = new();
        // Поперёк доски: канты.
        readonly DigitalAxisSmoother edge = new();
        public WeightController(WeightControlConfig config)
        {
            this.config = config;
        }
        /// <summary>
        /// Текущий сглаженный перенос веса. x — нос/хвост, y — канты.
        /// </summary>
        public Vector2 Value { get; private set; }
        /// <summary>
        /// Догоняет rawInput за один кадр и не даёт длине вектора превысить MaximumOffset.
        /// </summary>
        public void Tick(Vector2 rawInput, float deltaTime)
        {
            Value = new Vector2(
                ends.Tick(rawInput.x, deltaTime, config.InputRiseSpeed, config.ReturnSpeed),
                edge.Tick(rawInput.y, deltaTime, config.InputRiseSpeed, config.ReturnSpeed));
            // Одновременно нажатые оси складываются по диагонали.
            // ClampMagnitude сохраняет направление и укорачивает только слишком длинный вектор.
            Value = Vector2.ClampMagnitude(Value, config.MaximumOffset);
        }
        /// <summary>
        /// Сразу обнуляет обе оси и опубликованное значение.
        /// </summary>
        public void Reset()
        {
            ends.Reset();
            edge.Reset();
            Value = Vector2.zero;
        }
    }
}
