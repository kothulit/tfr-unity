using UnityEngine;

namespace TFR.Player
{
    /// <summary>
    /// Плавно ведёт цифровую ось к цели.
    /// Сырой ввод обычно равен -1, 0 или 1, а Value догоняет его с ограниченной скоростью.
    /// </summary>
    public sealed class DigitalAxisSmoother
    {
        // Цель меньше этого порога считается нейтралью: кнопку отпустили.
        const float NeutralThreshold = 0.001f;
        public float Value { get; private set; }
        /// <summary>
        /// Сдвигает Value к target на один кадр.
        /// riseSpeed — пока направление удерживают, returnSpeed — пока ось возвращается к нулю.
        /// </summary>
        public float Tick(float target, float deltaTime, float riseSpeed, float returnSpeed)
        {
            target = Mathf.Clamp(target, -1f, 1f);
            var isHeld = Mathf.Abs(target) > NeutralThreshold;
            var speed = isHeld ? riseSpeed : returnSpeed;
            // За кадр Value проходит не больше speed * deltaTime и не перескакивает цель.
            Value = Mathf.MoveTowards(Value, target, speed * deltaTime);
            return Value;
        }
        /// <summary>
        /// Сразу обнуляет ось, без плавного возврата.
        /// </summary>
        public void Reset()
        {
            Value = 0f;
        }
    }
}