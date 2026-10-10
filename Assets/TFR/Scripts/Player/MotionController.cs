using TFR.Board;
using TFR.Input;
using UnityEngine;
using VContainer;

namespace TFR.Player
{
    /// <summary>
    /// Переключает бег и езду. Сам персонажа не двигает.
    /// </summary>
    public sealed class MotionController : MonoBehaviour
    {
        readonly Character player = new();

        [SerializeField] Snowboard snowboard;
        [SerializeField] Transform playerHand;
        [SerializeField] Transform playerFoot;
        [SerializeField] float interactDistance = 2f;

        IGameplayInput input;
        RunningController running;
        RidingController riding;

        [Inject]
        public void Construct(IGameplayInput input)
        {
            this.input = input;
            running = GetComponent<RunningController>();
            riding = GetComponent<RidingController>();
            running.Construct(input);
        }

        void Start()
        {
            UseRunning();
        }

        void Update()
        {
            if (input.InteractPressed)
                Interact();

            if (input.MountPressed)
                Mount();

            if (player.State == CharacterMotionState.Riding)
                riding.Tick(Time.deltaTime);
            else
                running.Tick(Time.deltaTime);
        }

        void UseRunning()
        {
            running.enabled = true;
            riding.enabled = false;
        }

        void UseRiding()
        {
            running.enabled = false;
            riding.enabled = true;
        }

        void Interact()
        {
            if (snowboard == null || playerHand == null)
                return;
            if (snowboard.IsEquipped)
                return;
            if (snowboard.IsTaken)
            {
                if (!player.TryDropBoard())
                    return;

                Vector3 dropPosition = transform.position + transform.forward;
                Quaternion dropRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 90f);
                snowboard.Drop(dropPosition, dropRotation);
                return;
            }

            float distance = Vector3.Distance(playerHand.position, snowboard.transform.position);
            if (distance > interactDistance)
                return;
            if (!player.TryTakeBoard())
                return;

            snowboard.Take(playerHand);
        }

        void Mount()
        {
            if (snowboard == null || playerFoot == null || playerHand == null)
                return;

            if (snowboard.IsEquipped)
            {
                if (!player.TryStowBoard())
                    return;

                snowboard.Take(playerHand);
                UseRunning();
                return;
            }

            if (!snowboard.IsTaken)
                return;
            if (!player.TryMountBoard())
                return;

            snowboard.Equip(playerFoot);
            UseRiding();
        }
    }
}