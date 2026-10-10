using UnityEngine;

namespace TFR.Player
{
    /// <summary>
    /// ѕлавно ведЄт цифровую ось к цели.
    /// —ырой ввод обычно равен -1, 0 или 1, а Value догон€ет его с ограниченной скоростью.
    /// </summary>
    public sealed class DigitalAxisSmoother
    {
        // ÷ель меньше этого порога считаетс€ нейтралью: кнопку отпустили.
        const float NeutralThreshold = 0.001f;
        public float Value { get; private set; }
        /// <summary>
        /// —двигает Value к target на один кадр.
        /// riseSpeed Ч пока направление удерживают, returnSpeed Ч пока ось возвращаетс€ к нулю.
        /// </summary>
        public float Tick(float target, float deltaTime, float riseSpeed, float returnSpeed)
        {
            target = Mathf.Clamp(target, -1f, 1f);
            var isHeld = Mathf.Abs(target) > NeutralThreshold;
            var speed = isHeld ? riseSpeed : returnSpeed;
            // «а кадр Value проходит не больше speed * deltaTime и не перескакивает цель.
            Value = Mathf.MoveTowards(Value, target, speed * deltaTime);
            return Value;
        }
        /// <summary>
        /// —разу обнул€ет ось, без плавного возврата.
        /// </summary>
        public void Reset()
        {
            Value = 0f;
        }
    }
}