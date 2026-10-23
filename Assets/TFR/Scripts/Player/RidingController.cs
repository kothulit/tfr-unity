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
        WeightController weight;

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
        [SerializeField] float boardMaxDeviation = 0.03f;

        /// <summary>Сдвиг центра масс к носу или хвосту при полном переносе веса, метры.</summary>
        [SerializeField] float maxCenterOfMassShift = 0.1f;
        Vector3 baseCenterOfMass;

        BoardPhysicsModel boardModel;
        GameObject boardPhysicsObject;

        [SerializeField] BoardContactSettings contactSettings = new();
        [SerializeField] BoardFrictionSettings frictionSettings = new();

        [SerializeField] float ridingCapsuleLift = 0.3f;
        BoardGroundContact groundContact;
        float standingCapsuleHeight;
        Vector3 standingCapsuleCenter;

        [SerializeField] UprightAssistSettings uprightSettings = new();
        /// <summary>Наклон тела к канту при полном переносе веса, градусы.</summary>
        [SerializeField, Range(0f, 45f)] float maxLeanAngle = 20f;
        /// <summary>
        /// Насколько вес на конце доски уменьшает наклон тела: 0 — не уменьшает, 1 — при весе целиком на конце тело не наклоняется.
        /// </summary>
        [SerializeField, Range(0f, 1f)] float endWeightLeanReduction = 1f;

        UprightAssist uprightAssist;

        public void Construct(IGameplayInput gameplayInput, WeightController weightController)
        {
            input = gameplayInput;
            weight = weightController;
        }

        void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerCapsule = GetComponent<CapsuleCollider>();
            playerBody = GetComponent<Rigidbody>();
            uprightAssist = new UprightAssist(playerBody, uprightSettings);

            // Запоминаем исходные размеры капсулы, чтобы поднять, а потом вернуть на место
            playerCapsule = GetComponent<CapsuleCollider>();
            if (playerCapsule != null)
            {
                standingCapsuleHeight = playerCapsule.height;
                standingCapsuleCenter = playerCapsule.center;
            }

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
            playerBody.ResetCenterOfMass();
            baseCenterOfMass = playerBody.centerOfMass;

            uprightAssist.Enabled = true;
            riding = true;

            CreateBoardPhysics();

            weight.Reset();
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

            uprightAssist.Enabled = false;
            StandUpright();

            weight.Reset();

            if (playerBody != null)
                playerBody.ResetCenterOfMass();

            DestroyBoardPhysics();

            riding = false;
            snowboard = null;

            boardCollider = null;
        }

        public void Tick(float deltaTime)
        {
            if (!riding)
                return;
            // Нос слева, хвост справа: стик вправо — вес к хвосту. Стик вперёд — на носки.
            Vector2 move = input.Move;
            weight.Tick(new Vector2(-move.x, move.y), deltaTime);
            // Нагруженный конец поворачивает кант к носку или пятке, другой конец не трогаем.
            boardModel?.ApplyEndEdge(weight.NoseTail, weight.ToeHeel);
        }

        void FixedUpdate()
        {
            if (!riding)
                return;
            ShiftCenterOfMass();
            if (groundContact != null)
                groundContact.FixedTick();
            uprightAssist.TargetUp = LeanTarget();
            uprightAssist.FixedTick();
        }

        void OnGUI()
        {
            if (!riding || boardModel == null)
                return;
            var text = new System.Text.StringBuilder();
            text.AppendLine($"NoseTail {weight.NoseTail:F2}   ToeHeel {weight.ToeHeel:F2}   lean {uprightAssist.TiltAngle:F1}");
            for (int i = 0; i < 4; i++)
            {
                BoardContactPoint c = boardModel.Corners[i];
                text.AppendLine($"{(BoardCorner)i,-9} contact {c.Contact:F2}  N {c.NormalForce,5:F0}  edge {c.EdgeAngle,6:F1}  mu {c.SideMu:F2}  F {c.SideForce,5:F0}");
            }
            GUI.Label(new Rect(10, 10, 700, 110), text.ToString());
        }

        /// <summary>
        /// Перенос веса на нос или хвост: центр масс сдвигается вдоль доски, сама доска не наклоняется.
        /// </summary>
        void ShiftCenterOfMass()
        {
            if (boardPhysicsObject == null)
                return;
            Vector3 nose = boardPhysicsObject.transform.localRotation * Vector3.forward;
            playerBody.centerOfMass = baseCenterOfMass + nose * (weight.NoseTail * maxCenterOfMassShift);
        }

        /// <summary>
        /// Вертикаль, наклонённая к носочному или пяточному канту по переносу веса.
        /// </summary>
        Vector3 LeanTarget()
        {
            if (boardPhysicsObject == null)
                return Vector3.up;
            Vector3 toe = Vector3.ProjectOnPlane(boardPhysicsObject.transform.right, Vector3.up);
            if (toe.sqrMagnitude < 1e-4f)
                return Vector3.up;
            float leanShare = 1f - endWeightLeanReduction * Mathf.Abs(weight.NoseTail);
            float lean = weight.ToeHeel * leanShare * maxLeanAngle * Mathf.Deg2Rad;
            return Vector3.up * Mathf.Cos(lean) + toe.normalized * Mathf.Sin(lean);
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
                playerBody.interpolation = ride ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
            }

            // Поднимаем капсулу для того, чтобы она не мешала доске скользить
            if (playerCapsule != null)
            {
                playerCapsule.enabled = ride;
                float lift = ride ? ridingCapsuleLift : 0f;
                playerCapsule.height = standingCapsuleHeight - lift;
                playerCapsule.center = standingCapsuleCenter + Vector3.up * (lift * 0.5f);
            }
        }

        void CreateBoardPhysics()
        {
            boardModel = new BoardPhysicsModel(boardLength, boardWidth, boardMaxDeviation);
            boardPhysicsObject = new GameObject("BoardPhysics");
            boardPhysicsObject.transform.SetParent(transform, false);
            float feet = standingCapsuleCenter.y - standingCapsuleHeight * 0.5f;
            boardPhysicsObject.transform.localPosition = new Vector3(0f, feet, 0f);
            boardPhysicsObject.transform.localRotation = Quaternion.Euler(0f, boardPhysicsYaw, 0f);
            boardPhysicsObject.AddComponent<BoardPhysicsView>().Bind(boardModel);

            groundContact = new BoardGroundContact(boardModel, boardPhysicsObject.transform, playerBody, contactSettings, frictionSettings);
        }
        void DestroyBoardPhysics()
        {
            if (boardPhysicsObject != null)
                Destroy(boardPhysicsObject);
            boardPhysicsObject = null;
            boardModel = null;
        }

        /// <summary>
        /// CharacterController не выпрямляет наклон, поэтому перед бегом оставляем только поворот вокруг вертикали.
        /// </summary>
        void StandUpright()
        {
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (forward.sqrMagnitude < 1e-4f)
                forward = Vector3.ProjectOnPlane(transform.up, Vector3.up);
            if (forward.sqrMagnitude > 1e-4f)
                transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }
}