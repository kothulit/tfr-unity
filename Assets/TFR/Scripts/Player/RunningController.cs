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
        [SerializeField] Transform cameraTransform;

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

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            if (forward.sqrMagnitude > 0.001f)
            {
                forward.Normalize();
                right.Normalize();
                Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRotation, turnSpeed * deltaTime);
            }
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
        }
    }
}