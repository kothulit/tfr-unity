using System.Collections.Generic;
using UnityEngine;

namespace TFR.Board
{
    /// <summary>
    /// ”глы перечислены по обходу контура, чтобы их можно было соедин€ть линией подр€д.
    /// </summary>
    public enum BoardCorner
    {
        NoseToe = 0,
        NoseHeel = 1,
        TailHeel = 2,
        TailToe = 3
    }

    /// <summary>
    /// ”гол физической модели доски.
    /// </summary>
    public sealed class BoardContactPoint
    {
        /// <summary>+1 Ч сторона носа, -1 Ч сторона хвоста.</summary>
        public readonly int NoseSide;
        /// <summary>+1 Ч сторона носка, -1 Ч сторона п€тки.</summary>
        public readonly int ToeSide;
        /// <summary>ѕоложение в плоскости доски: x Ч носок/п€тка, y Ч нос/хвост.</summary>
        public readonly Vector2 PlanePosition;

        /// <summary>¬ысота над плоскостью доски. ќтрицательна€ Ч ниже плоскости.</summary>
        public float Height;
        /// <summary>”гол закантовки в этой точке, градусы. Ђ+ї Ч носочный кант ниже. ќбновл€ет BoardGroundContact.</summary>
        public float EdgeAngle;
        /// <summary>0 Ч точка в воздухе, 1 Ч касаетс€ склона. ќбновл€ет BoardGroundContact.</summary>
        public float Contact;

        /// <summary>ƒл€ отладки: прижатие, боковой коэффициент трени€ и бокова€ сила на последнем шаге.</summary>
        public float NormalForce;
        public float SideMu;
        public float SideForce;

        public BoardContactPoint(int noseSide, int toeSide, float length, float width)
        {
            NoseSide = noseSide;
            ToeSide = toeSide;
            PlanePosition = new Vector2(toeSide * width * 0.5f, noseSide * length * 0.5f);
        }

        /// <summary>Ћокальна€ точка: x Ч к носку, y Ч вверх, z Ч к носу.</summary>
        public Vector3 LocalPosition => new(PlanePosition.x, Height, PlanePosition.y);
    }

    /// <summary>
    /// ѕр€моугольник доски с четырьм€ углами, высоты которых завис€т от переноса веса.
    /// </summary>
    public sealed class BoardPhysicsModel
    {
        readonly BoardContactPoint[] corners = new BoardContactPoint[4];

        public float Length { get; }
        public float Width { get; }
        /// <summary>ѕредел отклонени€ угла от плоскости в обе стороны.</summary>
        public float MaxDeviation { get; }

        public IReadOnlyList<BoardContactPoint> Corners => corners;
        public BoardContactPoint this[BoardCorner corner] => corners[(int)corner];

        public BoardPhysicsModel(float length, float width, float maxDeviation)
        {
            Length = length;
            Width = width;
            MaxDeviation = Mathf.Abs(maxDeviation);

            corners[(int)BoardCorner.NoseToe] = new(1, 1, length, width);
            corners[(int)BoardCorner.NoseHeel] = new(1, -1, length, width);
            corners[(int)BoardCorner.TailHeel] = new(-1, -1, length, width);
            corners[(int)BoardCorner.TailToe] = new(-1, 1, length, width);
        }

        /// <summary>
        /// ѕересчитывает высоты углов.
        /// noseTail: +1 Ч вес на носу, -1 Ч на хвосте.
        /// toeHeel: +1 Ч вес на носках, -1 Ч на п€тках.
        /// Ќагруженна€ сторона опускаетс€, противоположна€ поднимаетс€.
        /// ѕри смещении к одному концу противоположный конец закручиваетс€ в обратную сторону.
        /// </summary>
        public void ApplyWeight(float noseTail, float toeHeel)
        {
            noseTail = Mathf.Clamp(noseTail, -1f, 1f);
            toeHeel = Mathf.Clamp(toeHeel, -1f, 1f);

            foreach (BoardContactPoint corner in corners)
            {
                float endFactor = 1f - Mathf.Abs(noseTail) + noseTail * corner.NoseSide;
                float height = -toeHeel * corner.ToeSide * endFactor * MaxDeviation;
                corner.Height = Mathf.Clamp(height, -MaxDeviation, MaxDeviation);
            }
        }

        /// <summary>
        /// ѕоворачивает кант нагруженного конца на twistDegrees: Ђ+ї Ч к носку, Ђ-ї Ч к п€тке.
        /// ƒругой конец остаЄтс€ в плоскости доски.
        /// noseTail: +1 Ч вес на носу, -1 Ч на хвосте.
        /// </summary>
        public void ApplyEndEdge(float noseTail, float twistDegrees)
        {
            noseTail = Mathf.Clamp(noseTail, -1f, 1f);
            float offset = Mathf.Clamp(Width * 0.5f * Mathf.Tan(twistDegrees * Mathf.Deg2Rad),
                -MaxDeviation, MaxDeviation);
            foreach (BoardContactPoint corner in corners)
            {
                float endWeight = Mathf.Max(0f, noseTail * corner.NoseSide);
                corner.Height = -corner.ToeSide * endWeight * offset;
            }
        }

        public void ResetHeights()
        {
            foreach (BoardContactPoint corner in corners)
                corner.Height = 0f;
        }
    }
}