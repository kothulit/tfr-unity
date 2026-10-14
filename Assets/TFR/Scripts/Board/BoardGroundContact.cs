using System;
using UnityEngine;

namespace TFR.Board
{
    [Serializable]
    public sealed class BoardContactSettings
    {
        public LayerMask GroundMask = ~0;
        /// <summary>Насколько выше угла начинается луч. Должно быть больше возможного проваливания за шаг.</summary>
        public float CastHeight = 0.3f;
        /// <summary>Запас ниже угла, в пределах которого угол считается стоящим на земле.</summary>
        public float GroundedDepth = 0.05f;
        /// <summary>Н/м на один угол. При массе 70 кг угол проседает примерно на сантиметр.</summary>
        public float Stiffness = 17000f;
        /// <summary>Н·с/м на один угол.</summary>
        public float Damping = 550f;
        /// <summary>Скорость скольжения в м/с, при которой трение выходит на полную силу. Убирает дрожание около нуля.</summary>
        public float FrictionSlipSpeed = 0.1f;
    }

    /// <summary>
    /// Не даёт углам BoardPhysicsModel уйти под поверхность: каждый угол работает как пружина с демпфером по лучу.
    /// Вызывается из FixedUpdate.
    /// </summary>
    public sealed class BoardGroundContact
    {
        readonly BoardPhysicsModel model;
        readonly Transform frame;
        readonly Rigidbody body;
        readonly BoardContactSettings settings;
        readonly bool[] grounded = new bool[4];

        public BoardGroundContact(BoardPhysicsModel model, Transform frame, Rigidbody body, BoardContactSettings settings)
        {
            this.model = model;
            this.frame = frame;
            this.body = body;
            this.settings = settings;
        }

        /// <summary>Хотя бы один угол касается поверхности.</summary>
        public bool IsGrounded { get; private set; }

        public bool IsCornerGrounded(BoardCorner corner) => grounded[(int)corner];

        public void FixedTick()
        {
            IsGrounded = false;
            Vector3 up = frame.up;

            for (int i = 0; i < 4; i++)
            {
                BoardContactPoint corner = model.Corners[i];
                Vector3 point = frame.TransformPoint(corner.LocalPosition);
                Vector3 origin = point + up * settings.CastHeight;
                float distance = settings.CastHeight + settings.GroundedDepth;

                grounded[i] = Physics.Raycast(origin, -up, out RaycastHit hit, distance,
                    settings.GroundMask, QueryTriggerInteraction.Ignore);
                if (!grounded[i])
                    continue;
                IsGrounded = true;

                // Больше нуля — угол ушёл под поверхность.
                float penetration = settings.CastHeight - hit.distance;
                if (penetration <= 0f)
                    continue;

                Vector3 velocity = body.GetPointVelocity(point);
                float normalSpeed = Vector3.Dot(velocity, hit.normal);
                float normalForce = Mathf.Max(0f, settings.Stiffness * penetration - settings.Damping * normalSpeed);

                body.AddForceAtPosition(hit.normal * normalForce, point);
                ApplyFriction(corner, point, hit.normal, velocity, normalForce);
            }
        }

        void ApplyFriction(BoardContactPoint corner, Vector3 point, Vector3 normal, Vector3 velocity, float normalForce)
        {
            Vector3 nose = Vector3.ProjectOnPlane(frame.forward, normal).normalized;
            Vector3 toe = Vector3.ProjectOnPlane(frame.right, normal).normalized;

            float slip = Mathf.Max(settings.FrictionSlipSpeed, 0.0001f);
            float longitudinal = Mathf.Clamp(Vector3.Dot(velocity, nose) / slip, -1f, 1f);
            float lateral = Mathf.Clamp(Vector3.Dot(velocity, toe) / slip, -1f, 1f);

            Vector3 force =
                -nose * (longitudinal * corner.LongitudinalFriction * normalForce)
                - toe * (lateral * corner.LateralFriction * normalForce);

            body.AddForceAtPosition(force, point);
        }
    }
}