using TFR.Config;
using UnityEngine;

namespace TFR.Player
{
    /// <summary>
    /// —глаживает перенос веса в ос€х доски и ограничивает его по каждой оси.
    /// </summary>
    public sealed class WeightController
    {
        const float NeutralThreshold = 0.0001f;

        readonly WeightControlConfig config;
        readonly DigitalAxisSmoother noseTail = new();
        readonly DigitalAxisSmoother toeHeel = new();

        public WeightController(WeightControlConfig config)
        {
            this.config = config;
        }

        /// <summary>+1 Ч вес на носу, -1 Ч на хвосте.</summary>
        public float NoseTail { get; private set; }
        /// <summary>+1 Ч вес на носках, -1 Ч на п€тках.</summary>
        public float ToeHeel { get; private set; }
        /// <summary>x Ч нос/хвост, y Ч носок/п€тка.</summary>
        public Vector2 Value => new(NoseTail, ToeHeel);

        /// <summary>
        /// rawInput уже в ос€х доски: x Ч нос/хвост, y Ч носок/п€тка.
        /// </summary>
        public void Tick(Vector2 rawInput, float deltaTime)
        {
            Vector2 target = CircleToSquare(rawInput);
            float limit = config.MaximumOffset;

            NoseTail = noseTail.Tick(Mathf.Clamp(target.x, -limit, limit),
                deltaTime, config.InputRiseSpeed, config.ReturnSpeed);
            ToeHeel = toeHeel.Tick(Mathf.Clamp(target.y, -limit, limit),
                deltaTime, config.InputRiseSpeed, config.ReturnSpeed);
        }

        /// <summary>
        /// —разу обнул€ет обе оси.
        /// </summary>
        public void Reset()
        {
            noseTail.Reset();
            toeHeel.Reset();
            NoseTail = 0f;
            ToeHeel = 0f;
        }

        /// <summary>
        /// Composite 2DVector нормализует диагональ до 0.71 по каждой оси.
        ///  руг раст€гиваетс€ до квадрата, чтобы диагональ давала полный вес на обе оси.
        /// </summary>
        static Vector2 CircleToSquare(Vector2 input)
        {
            float max = Mathf.Max(Mathf.Abs(input.x), Mathf.Abs(input.y));
            if (max < NeutralThreshold)
                return Vector2.zero;

            Vector2 square = input * (input.magnitude / max);
            return new Vector2(Mathf.Clamp(square.x, -1f, 1f), Mathf.Clamp(square.y, -1f, 1f));
        }
    }
}