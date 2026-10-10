using UnityEngine;
namespace TFR.Board
{
    /// <summary>
    /// Управление ездой. Пока IsInControl ложен, кадр ему не принадлежит.
    /// </summary>
    public sealed class SnowboardController : MonoBehaviour
    {
        public bool IsInControl { get; private set; }
        public void TakeControl()
        {
            IsInControl = true;
        }
        public void ReleaseControl()
        {
            IsInControl = false;
        }
        void Update()
        {
            if (!IsInControl)
                return;
        }
    }
}