using TFR.Camera;
using TFR.Input;
using UnityEngine;

namespace TFR.Player
{
    /// <summary>
    /// Бег без доски и бег с доской в руке.
    /// </summary>
    public sealed class RunningController : MonoBehaviour
    {
        [SerializeField] float walkSpeed = 4f;
        [SerializeField] float runSpeed = 8f;
        [SerializeField] float turnSpeed = 12f;
        [SerializeField] CameraFollowController cameraFollow;
        [SerializeField] Animator animator;
        [SerializeField] float runClipSpeed = 8f;
        [SerializeField] float minRunAnimSpeed = 0.5f;
        [SerializeField] float maxRunAnimSpeed = 1.5f;

        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int RunAnimSpeedHash = Animator.StringToHash("RunAnimSpeed");

        CharacterController controller;
        IGameplayInput input;
        float verticalVelocity;

        public void Construct(IGameplayInput gameplayInput)
        {
            input = gameplayInput;
        }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        public void Tick(float deltaTime)
        {
            Vector2 move = input.Move;
            float speed = input.SprintHeld ? runSpeed : walkSpeed;

            Quaternion planar = Quaternion.Euler(0f, cameraFollow.Yaw, 0f);
            Vector3 forward = planar * Vector3.forward;
            Vector3 right = planar * Vector3.right;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, planar, turnSpeed * deltaTime);
            Vector3 direction = forward * move.y + right * move.x;

            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            else
                verticalVelocity += Physics.gravity.y * deltaTime;
            Vector3 velocity = direction * speed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * deltaTime);

            float horizontalSpeed = (direction * speed).magnitude;
            animator.SetFloat(SpeedHash, horizontalSpeed, 0.1f, deltaTime);

            float animSpeed = Mathf.Clamp(
                horizontalSpeed / runClipSpeed, minRunAnimSpeed, maxRunAnimSpeed);
            animator.SetFloat(RunAnimSpeedHash, animSpeed, 0.1f, deltaTime);
        }

        void OnDisable()
        {
            if (animator != null)
                animator.SetFloat(SpeedHash, 0f);
        }
    }
}