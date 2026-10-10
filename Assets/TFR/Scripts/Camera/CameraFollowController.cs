using TFR.Input;
using UnityEngine;
using VContainer;

namespace TFR.Camera
{
    /// <summary>
    /// Орбита вокруг игрока. Направление бега читает её transform, сама она игрока не двигает.
    /// </summary>
    public class CameraFollowController : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 3f, -6f);
        [SerializeField] float followSmooth = 8f;
        [SerializeField] float lookSensitivity = 0.15f;
        [SerializeField] float minPitch = -30f;
        [SerializeField] float maxPitch = 60f;

        IGameplayInput input;
        float yaw;
        float pitch = 15f;

        public float Yaw => yaw;

        [Inject]
        public void Construct(IGameplayInput input)
        {
            this.input = input;
        }

        void Start()
        {
            if (target != null)
                yaw = target.eulerAngles.y;
        }

        void LateUpdate()
        {
            if (target == null || input == null)
                return;

            Vector2 look = input.LookDelta;
            yaw += look.x * lookSensitivity;
            // Наклон, при котором камера оказывается над головой и LookAt разворачивается на 180°.
            float overheadPitch = Mathf.Atan2(-offset.z, offset.y) * Mathf.Rad2Deg - 10f;
            pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, minPitch, Mathf.Min(maxPitch, overheadPitch));

            Quaternion orbit = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
            Vector3 desired = target.position + orbit * offset;
            transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);

            Vector3 toTarget = target.position + Vector3.up * 0.5f - transform.position;
            if (toTarget.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(toTarget, orbit * Vector3.up);
        }
    }
}