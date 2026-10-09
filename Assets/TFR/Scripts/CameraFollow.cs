using UnityEngine;
namespace TFR
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 3f, -6f);
        [SerializeField] float smooth = 8f;
        void LateUpdate()
        {
            if (target == null)
                return;
            Vector3 desired = target.position + target.rotation * offset;
            transform.position = Vector3.Lerp(
                transform.position, desired, smooth * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
    }
}