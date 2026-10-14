using System;
using UnityEngine;

namespace TFR.Player
{
    [Serializable]
    public sealed class UprightAssistSettings
    {
        /// <summary>Общая сила помощи: 0 — нет, 1 — полная.</summary>
        [Range(0f, 1f)] public float Strength = 0.5f;
        /// <summary>Собственная частота возврата к вертикали, Гц. Чем выше, тем жёстче держит.</summary>
        public float Frequency = 1.2f;
        /// <summary>1 — возврат без раскачки, меньше 1 — с перелётом.</summary>
        public float DampingRatio = 0.8f;
        /// <summary>Предел углового ускорения от помощи, рад/с².</summary>
        public float MaxAcceleration = 15f;
        /// <summary>С этого наклона помощь начинает ослабевать, градусы.</summary>
        public float FadeStartAngle = 25f;
        /// <summary>При этом наклоне помощь пропадает полностью, градусы.</summary>
        public float GiveUpAngle = 60f;
    }

    /// <summary>
    /// Слегка подталкивает Rigidbody к вертикали. Поворот вокруг вертикали не трогает.
    /// Вызывается из FixedUpdate.
    /// </summary>
    public sealed class UprightAssist
    {
        readonly Rigidbody body;
        readonly UprightAssistSettings settings;

        public UprightAssist(Rigidbody body, UprightAssistSettings settings)
        {
            this.body = body;
            this.settings = settings;
        }

        public bool Enabled { get; set; }
        /// <summary>К чему тянуть ось up персонажа. Позже сюда можно подставлять нормаль склона.</summary>
        public Vector3 TargetUp { get; set; } = Vector3.up;
        /// <summary>Текущий наклон от TargetUp, градусы.</summary>
        public float TiltAngle { get; private set; }

        public void FixedTick()
        {
            if (!Enabled || body == null || body.isKinematic)
                return;

            Vector3 up = body.rotation * Vector3.up;
            Vector3 target = TargetUp.normalized;
            TiltAngle = Vector3.Angle(up, target);

            float fade = 1f - Mathf.InverseLerp(settings.FadeStartAngle, settings.GiveUpAngle, TiltAngle);
            float strength = settings.Strength * fade;
            if (strength <= 0f)
                return;

            Vector3 axis = Vector3.Cross(up, target);
            if (axis.sqrMagnitude < 1e-8f)
                return;
            Vector3 error = axis.normalized * (TiltAngle * Mathf.Deg2Rad);

            Vector3 angularVelocity = body.angularVelocity;
            Vector3 tiltVelocity = angularVelocity - Vector3.Project(angularVelocity, up);

            float omega = 2f * Mathf.PI * settings.Frequency;
            float stiffness = omega * omega;
            float damping = 2f * settings.DampingRatio * omega;

            Vector3 acceleration = (stiffness * error - damping * tiltVelocity) * strength;
            acceleration = Vector3.ClampMagnitude(acceleration, settings.MaxAcceleration * strength);

            body.AddTorque(acceleration, ForceMode.Acceleration);
        }
    }
}