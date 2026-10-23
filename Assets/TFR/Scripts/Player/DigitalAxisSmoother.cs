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
        /// <summary>Сколько секунд ось не удерживают.</summary>
        float releasedTime;
        public float Value { get; private set; }
        /// <summary>
        /// Сдвигает Value к target на один кадр.
        /// riseSpeed — пока направление удерживают, returnSpeed — скорость возврата к нулю от времени после отпускания.
        /// </summary>
        public float Tick(float target, float deltaTime, float riseSpeed, AnimationCurve returnSpeed)
        {
            target = Mathf.Clamp(target, -1f, 1f);
            bool isHeld = Mathf.Abs(target) > NeutralThreshold;
            float speed;
            if (isHeld)
            {
                releasedTime = 0f;
                speed = riseSpeed;
            }
            else
            {
                releasedTime += deltaTime;
                speed = Mathf.Max(0f, returnSpeed.Evaluate(releasedTime));
            }
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
            releasedTime = 0f;
        }
    }
}