using UnityEngine;
using UnityEngine.InputSystem;

namespace TFR
{
    public class PlayerLocomotion : MonoBehaviour
    {
        [SerializeField] float walkSpeed = 4f;
        [SerializeField] float runSpeed = 8f;
        [SerializeField] float turnSpeed = 12f;
        [SerializeField] Transform cameraTransform;

        [SerializeField] Snowboard snowboard;
        [SerializeField] Transform playerHand;
        [SerializeField] Transform playerFoot;
        [SerializeField] float interactDistance = 2f;
        [SerializeField] Transform snowboardTransform;

        private CharacterController controller;
        private Animator animator;
        private InputSystem_Actions input;
        private float verticalVelocity;


        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();
            input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            input.Player.Enable();
            input.Player.Interact.performed += OnInteract;
            input.Player.Crouch.performed += OnCrouch;
        }

        private void OnDisable()
        {
            input.Player.Interact.performed -= OnInteract;
            input.Player.Crouch.performed -= OnCrouch;
            input.Player.Disable();
        }

        private void Update()
        {
            Vector2 move = input.Player.Move.ReadValue<Vector2>();
            float speed = input.Player.Sprint.IsPressed() ? walkSpeed : runSpeed;

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 direction = forward * move.y + right * move.x;
            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, target, turnSpeed * Time.deltaTime);
            }

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            else
                verticalVelocity += Physics.gravity.y * Time.deltaTime;

            Vector3 velocity = direction * speed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
            //if (animator != null)
            //    animator.SetFloat("Speed", direction.magnitude * (speed / runSpeed));
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (snowboard == null || snowboard.IsTaken || playerHand == null)
                return;
            float distance = Vector3.Distance(playerHand.position, snowboard.transform.position);
            if (distance > interactDistance)
                return;
            snowboard.Take(playerHand);
        }

        private void OnCrouch(InputAction.CallbackContext context)
        {
            if (snowboard == null || !snowboard.IsTaken || playerFoot == null)
            {
                return;
            }
            snowboard.Equip(playerFoot);
        }
    }
}
