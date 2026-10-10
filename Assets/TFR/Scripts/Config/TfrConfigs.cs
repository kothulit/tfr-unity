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
        public float InputRiseSpeed = 2.6f;
        /// <summary>
        /// Скорость возврата к нулю, когда направление отпущено, в единицах в секунду.
        /// </summary>
        public float ReturnSpeed = 3.4f;
        /// <summary>
        /// Наибольший сдвиг веса в одну сторону. Итоговое значение остаётся в диапазоне от -MaximumOffset до MaximumOffset.
        /// </summary>
        public float MaximumOffset = 1f;
    }
}