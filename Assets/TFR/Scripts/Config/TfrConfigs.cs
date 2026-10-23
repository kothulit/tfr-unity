using UnityEngine;

namespace TFR.Config
{
    /// <summary>
    /// Настройки переноса веса. Создаётся через Assets → Create → TFR/Weight Config
    /// и правится в инспекторе, не в коде.
    /// </summary>
    [CreateAssetMenu(menuName = "TFR/Weight Config")]
    public sealed class WeightControlConfig : ScriptableObject
    {
        /// <summary>
        /// Скорость, с которой вес догоняет удержанное направление, в единицах в секунду.
        /// </summary>
        public float InputRiseSpeed = 1.0f;
        /// <summary>
        /// Скорость возврата к нулю, когда направление отпущено, в единицах в секунду.
        /// </summary>
        public float ReturnSpeed = 3.4f;
        /// <summary>
        /// Наибольший сдвиг веса в одну сторону. Итоговое значение остаётся в диапазоне от -MaximumOffset до MaximumOffset.
        /// </summary>
        public float MaximumOffset = 1f;

        /// <summary>
        /// Скорость возврата к нулю после отпускания, в единицах в секунду.
        /// По горизонтали — секунды с момента отпускания, по вертикали — скорость.
        /// Медленное начало позволяет удерживать вес короткими нажатиями.
        /// </summary>
        public AnimationCurve ReturnSpeedOverTime = new(
            new Keyframe(0f, 0.5f),
            new Keyframe(0.3f, 1f),
            new Keyframe(0.6f, 4f));
    }
}