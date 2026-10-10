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
            pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, minPitch, maxPitch);

            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desired = target.position + orbit * offset;
            transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 0.5f);
        }
    }
}