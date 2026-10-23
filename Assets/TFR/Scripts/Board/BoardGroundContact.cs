using System;
using UnityEngine;

namespace TFR.Board
{
    [Serializable]
    public sealed class BoardContactSettings
    {
        public LayerMask GroundMask = ~0;
        /// <summary>Насколько выше угла начинается луч. Должно быть больше возможного проваливания за шаг.</summary>
        public float CastHeight = 0.6f;
        /// <summary>Радиус сферы, которой ищется поверхность под углом. Сглаживает рёбра треугольников Terrain.</summary>
        public float ProbeRadius = 0.05f;
        /// <summary>Запас ниже угла, в пределах которого угол считается стоящим на земле.</summary>
        public float GroundedDepth = 0.05f;
        /// <summary>Н/м на один угол. При массе 70 кг угол проседает примерно на сантиметр.</summary>
        public float Stiffness = 17000f;
        /// <summary>Н·с/м на один угол.</summary>
        public float Damping = 550f;
        /// <summary>Скорость скольжения в м/с, при которой трение выходит на полную силу. Убирает дрожание около нуля.</summary>
        public float FrictionSlipSpeed = 0.1f;
        /// <summary>Н·с/м на один угол, пока угол вдавливается в поверхность.</summary>
        public float CompressionDamping = 450f;
        /// <summary>Н·с/м на один угол, пока угол выходит из поверхности. Больше — меньше подбрасывает.</summary>
        public float ReboundDamping = 1500f;
    }

    /// <summary>
    /// Не даёт углам BoardPhysicsModel уйти под поверхность: каждый угол работает как пружина с демпфером по лучу.
    /// Трение в каждом угле зависит от его закантовки относительно склона.
    /// Вызывается из FixedUpdate.
    /// </summary>
    public sealed class BoardGroundContact
    {
        readonly BoardPhysicsModel model;
        readonly Transform frame;
        readonly Rigidbody body;
        readonly BoardContactSettings settings;
        readonly BoardFrictionSettings frictionSettings;
        readonly bool[] grounded = new bool[4];
        readonly RaycastHit[] hits = new RaycastHit[4];
        readonly Vector3[] points = new Vector3[4];


        public BoardGroundContact(BoardPhysicsModel model,
            Transform frame,
            Rigidbody body,
            BoardContactSettings contactSettings,
            BoardFrictionSettings frictionSettings)
        {
            this.model = model;
            this.frame = frame;
            this.body = body;
            this.settings = contactSettings;
            this.frictionSettings = frictionSettings;
        }

        /// <summary>Хотя бы один угол касается поверхности.</summary>
        public bool IsGrounded { get; private set; }

        public bool IsCornerGrounded(BoardCorner corner) => grounded[(int)corner];

        public void FixedTick()
        {
            Vector3 up = frame.up;
            float reach = Mathf.Max(settings.GroundedDepth, frictionSettings.ContactFadeHeight);
            int contacts = 0;

            // Сначала все лучи: трению нужно знать число касаний до того, как приложены силы.
            for (int i = 0; i < 4; i++)
            {
                points[i] = frame.TransformPoint(model.Corners[i].LocalPosition);
                Vector3 origin = points[i] + up * settings.CastHeight;
                grounded[i] = Physics.SphereCast(origin, settings.ProbeRadius, -up, out hits[i],
                    settings.CastHeight + reach, settings.GroundMask, QueryTriggerInteraction.Ignore);
                if (grounded[i])
                    contacts++;
            }
            IsGrounded = contacts > 0;

            Vector3 normalSum = Vector3.zero;
            for (int i = 0; i < 4; i++)
                if (grounded[i])
                    normalSum += hits[i].normal;
            Vector3 groundNormal = contacts > 0 ? normalSum.normalized : up;

            float fadeHeight = Mathf.Max(frictionSettings.ContactFadeHeight, 0.0001f);
            for (int i = 0; i < 4; i++)
            {
                BoardContactPoint corner = model.Corners[i];
                corner.Contact = 0f;

                corner.NormalForce = 0f;
                corner.SideMu = 0f;
                corner.SideForce = 0f;

                if (!grounded[i])
                    continue;

                RaycastHit hit = hits[i];
                Vector3 point = points[i];

                // Больше нуля — угол ушёл под поверхность, меньше нуля — висит над ней.
                float penetration = settings.CastHeight - hit.distance - settings.ProbeRadius;
                corner.Contact = 1f - Mathf.Clamp01(-penetration / fadeHeight);
                if (corner.Contact <= 0f)
                    continue;

                Vector3 velocity = body.GetPointVelocity(point);
                float normalForce = 0f;
                if (penetration > 0f)
                {
                    float normalSpeed = Vector3.Dot(velocity, groundNormal);
                    float damping = normalSpeed < 0f ? settings.CompressionDamping : settings.ReboundDamping;
                    normalForce = Mathf.Max(0f, settings.Stiffness * penetration - damping * normalSpeed);
                    body.AddForceAtPosition(groundNormal * normalForce, point);
                }

                corner.NormalForce = normalForce;
                ApplyFriction(corner, point, groundNormal, velocity, normalForce, contacts);
            }

            ApplyAirDrag();
        }

        void ApplyFriction(BoardContactPoint corner, Vector3 point, Vector3 normal, Vector3 velocity,
    float normalForce, int contacts)
        {
            Vector3 nose = Vector3.ProjectOnPlane(frame.forward, normal).normalized;
            Vector3 toe = Vector3.Cross(normal, nose);
            float edge = EdgeAngle(corner, normal);
            corner.EdgeAngle = edge;
            float absEdge = Mathf.Abs(edge);
            // Боковая сила на высоте центра масс: уводит и разворачивает доску, но не опрокидывает тело.
            Vector3 sidePoint = point + normal * Vector3.Dot(body.worldCenterOfMass - point, normal);
            float vAlong = Vector3.Dot(velocity, nose);
            float vSide = Vector3.Dot(body.GetPointVelocity(sidePoint), toe);
            // +1 — впереди по скольжению опущенный кант, -1 — поднятый, 0 — доска плашмя.
            float blend = Mathf.Max(frictionSettings.FlatBlendAngle, 0.01f);
            float leadingDown = Mathf.Clamp(edge / blend, -1f, 1f) * Mathf.Sign(vSide);
            float muAlong = frictionSettings.AlongEdge.Evaluate(absEdge);
            float muSide = Mathf.Lerp(frictionSettings.TowardBase.Evaluate(absEdge),
                                      frictionSettings.AwayFromBase.Evaluate(absEdge),
                                      0.5f + 0.5f * leadingDown);
            float slip = Mathf.Max(settings.FrictionSlipSpeed, 0.0001f);
            float fAlong = Mathf.Clamp(vAlong / slip, -1f, 1f) * muAlong * normalForce
                         + frictionSettings.AlongDrag * corner.Contact * vAlong * Mathf.Abs(vAlong);
            float fSide = Mathf.Clamp(vSide / slip, -1f, 1f) * muSide * normalForce
                        + frictionSettings.SideDrag * corner.Contact * vSide * Mathf.Abs(vSide);
            // Не больше половины силы, гасящей скорость точки за шаг, иначе трение раскачивает тело.
            float perStep = 0.5f / contacts / Time.fixedDeltaTime;
            float alongLimit = perStep * EffectiveMass(point, nose);
            float sideLimit = perStep * EffectiveMass(sidePoint, toe);
            fAlong = Mathf.Sign(vAlong) * Mathf.Min(Mathf.Abs(fAlong), alongLimit * Mathf.Abs(vAlong));
            fSide = Mathf.Sign(vSide) * Mathf.Min(Mathf.Abs(fSide), sideLimit * Mathf.Abs(vSide));
            body.AddForceAtPosition(-nose * fAlong, point);
            body.AddForceAtPosition(-toe * fSide, sidePoint);

            corner.SideMu = muSide;
            corner.SideForce = fSide;
            Debug.DrawRay(point, normal * (normalForce * 0.002f), Color.green);
            Debug.DrawRay(sidePoint, -toe * (fSide * 0.005f), Color.yellow);
        }

        /// <summary>
        /// Масса, которую чувствует сила в точке вдоль направления: тело не только сдвигается, но и поворачивается.
        /// </summary>
        float EffectiveMass(Vector3 point, Vector3 direction)
        {
            Vector3 lever = Vector3.Cross(point - body.worldCenterOfMass, direction);
            Vector3 local = Quaternion.Inverse(body.rotation * body.inertiaTensorRotation) * lever;
            Vector3 inertia = body.inertiaTensor;
            float angular = local.x * local.x / inertia.x
                          + local.y * local.y / inertia.y
                          + local.z * local.z / inertia.z;
            return 1f / (1f / body.mass + angular);
        }

        /// <summary>
        /// Закантовка точки относительно склона: крен всей доски плюс перекос её конца от переноса веса.
        /// Наклон нос/хвост не учитывается. «+» — носочный кант ниже.
        /// </summary>
        float EdgeAngle(BoardContactPoint corner, Vector3 normal)
        {
            float frameRoll = Mathf.Atan2(-Vector3.Dot(frame.right, normal), Vector3.Dot(frame.up, normal));

            bool nose = corner.NoseSide > 0;
            BoardContactPoint toe = model[nose ? BoardCorner.NoseToe : BoardCorner.TailToe];
            BoardContactPoint heel = model[nose ? BoardCorner.NoseHeel : BoardCorner.TailHeel];
            float twist = Mathf.Atan2(heel.Height - toe.Height, model.Width);

            return (frameRoll + twist) * Mathf.Rad2Deg;
        }

        void ApplyAirDrag()
        {
            Vector3 velocity = body.linearVelocity;
            body.AddForce(-frictionSettings.AirDrag * velocity.magnitude * velocity);
        }
    }
}