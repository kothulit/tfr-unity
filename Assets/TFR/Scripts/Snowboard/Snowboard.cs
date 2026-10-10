using UnityEngine;
namespace TFR
{
    public class Snowboard : MonoBehaviour
    {
        [SerializeField] Collider solidCollider;

        public bool IsTaken { get; private set; }
        public bool IsEquipped { get; private set; }

        public void Take(Transform mount)
        {
            if (IsTaken)
                return;
            IsEquipped = false;
            IsTaken = true;
            transform.SetParent(mount, false);
            transform.localScale = Vector3.one;
            transform.localPosition = new Vector3(0f, 0f, 0f);
            transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
        }

        public void Equip(Transform mount)
        {
            if (IsEquipped)
                return;
            if (!IsTaken)
                return;
            IsEquipped = true;
            IsTaken = false;
            if (solidCollider != null)
                solidCollider.enabled = false;
            transform.SetParent(mount, false);
            transform.localScale = Vector3.one;
            transform.localPosition = new Vector3(0.14f, -0.1f, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        
    }
}