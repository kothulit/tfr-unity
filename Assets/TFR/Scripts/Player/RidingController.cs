using TFR.Board;
using TFR.Input;
using UnityEngine;

namespace TFR.Player
{
    /// <summary>
    /// Включает физику езды и выключает её при возврате к бегу.
    /// Переноса веса здесь нет.
    /// </summary>
    public sealed class RidingController : MonoBehaviour
    {
        [SerializeField] float shiftSpeed = 0.8f;
        [SerializeField] float maximumOffset = 0.35f;

        IGameplayInput input;
        float weightOffset;

        CharacterController characterController;
        CapsuleCollider playerCapsule;
        Rigidbody playerBody;

        Snowboard snowboard;
        Collider boardCollider;

        /// <summary>Идёт ли сейчас езда.</summary>
        bool riding;


        // Поля для модели сноуборда
        [Header("Board physics")]
        [SerializeField] Vector3 boardPhysicsOffset = new(0f, -0.83f, 0f);
        [SerializeField] float boardPhysicsYaw = 90f;
        [SerializeField] float boardLength = 1f;
        [SerializeField] float boardWidth = 0.25f;
        [SerializeField] float boardMaxDeviation = 0.05f;
        [SerializeField] float boardLongitudinalFriction = 0.05f;
        [SerializeField] float boardLateralFriction = 1f;
        BoardPhysicsModel boardModel;
        GameObject boardPhysicsObject;

        [SerializeField] BoardContactSettings contactSettings = new();
        [SerializeField] float ridingCapsuleLift = 0.3f;
        BoardGroundContact groundContact;
        float standingCapsuleHeight;
        Vector3 standingCapsuleCenter;

        public void Construct(IGameplayInput gameplayInput)
        {
            input = gameplayInput;
        }

        void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerCapsule = GetComponent<CapsuleCollider>();
            playerBody = GetComponent<Rigidbody>();

            // В сцене капсула и динамическое тело могут быть включены вместе с CharacterController.
            // До встёгивания персонажа двигает только контроллер бега.
            SetPlayerPhysics(false);
        }

        /// <summary>
        /// Встёгивание: персонаж становится физическим телом, коробка доски входит в это тело.
        /// </summary>
        public void Begin(Snowboard board)
        {
            if (riding || board == null)
                return;

            snowboard = board;
            boardCollider = board.GetComponent<Collider>();

            // Доска уже поставлена на ноги в Equip. Оставляем её мировую позу
            // и переносим на корень персонажа, где лежит единственный Rigidbody.
            snowboard.transform.SetParent(transform, true);

            // Свой Rigidbody на доске забирает её коллайдер себе,
            // и коробка больше не принадлежит телу игрока.
            Rigidbody boardBody = snowboard.GetComponent<Rigidbody>();
            if (boardBody != null)
                Destroy(boardBody);

            boardCollider = board.GetComponent<Collider>();
            if (boardCollider != null)
                boardCollider.enabled = false;

            SetPlayerPhysics(true);
            riding = true;

            CreateBoardPhysics();

            weightOffset = 0f;
        }

        /// <summary>
        /// Снятие доски: снова бег через CharacterController.
        /// Вызывается до Take, пока ссылка на доску ещё нужна.
        /// </summary>
        public void End()
        {
            if (!riding)
                return;

            if (boardCollider != null)
                boardCollider.enabled = false;

            SetPlayerPhysics(false);

            weightOffset = 0f;
            if (playerBody != null)
                playerBody.ResetCenterOfMass();

            DestroyBoardPhysics();

            riding = false;
            snowboard = null;

            boardCollider = null;
        }

        public void Tick(float deltaTime)
        {
        }

        void FixedUpdate()
        {
            if (riding && groundContact != null)
                groundContact.FixedTick();
        }

        /// <summary>
        /// true — езда: капсула и гравитация. false — бег: CharacterController.
        /// </summary>
        void SetPlayerPhysics(bool ride)
        {
            if (playerBody != null && !ride)
            {
                playerBody.linearVelocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
            }

            if (characterController != null)
                characterController.enabled = !ride;

            if (playerCapsule != null)
                playerCapsule.enabled = ride;

            if (playerBody != null)
            {
                playerBody.isKinematic = !ride;
                playerBody.useGravity = ride;
            }
        }

        void CreateBoardPhysics()
        {
            boardModel = new BoardPhysicsModel(boardLength, boardWidth, boardMaxDeviation,
                boardLongitudinalFriction, boardLateralFriction);
            boardPhysicsObject = new GameObject("BoardPhysics");
            boardPhysicsObject.transform.SetParent(transform, false);
            float feet = playerCapsule.center.y - playerCapsule.height * 0.5f;
            boardPhysicsObject.transform.localPosition = new Vector3(0f, feet, 0f);
            boardPhysicsObject.transform.localRotation = Quaternion.Euler(0f, boardPhysicsYaw, 0f);
            boardPhysicsObject.AddComponent<BoardPhysicsView>().Bind(boardModel);

            groundContact = new BoardGroundContact(boardModel, boardPhysicsObject.transform, playerBody, contactSettings);
        }
        void DestroyBoardPhysics()
        {
            if (boardPhysicsObject != null)
                Destroy(boardPhysicsObject);
            boardPhysicsObject = null;
            boardModel = null;
        }
    }
}