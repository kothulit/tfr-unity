using System;
using UnityEngine;

namespace TFR.Board
{
    [Serializable]
    public sealed class BoardFrictionSettings
    {
        /// <summary>Угол закантовки, градусы → μ вдоль канта.</summary>
        public AnimationCurve AlongEdge = new(new Keyframe(0f, 0.05f), new Keyframe(60f, 0.02f));
        /// <summary>Впереди поднятый кант: доска скользит на базе и тормозит при закантовке.</summary>
        public AnimationCurve TowardBase = new(new Keyframe(0f, 0.1f), new Keyframe(60f, 1.5f));
        /// <summary>Впереди опущенный кант: ловля канта.</summary>
        public AnimationCurve AwayFromBase = new(new Keyframe(0f, 2f), new Keyframe(60f, 8f));
        /// <summary>Ширина зоны «плашмя», градусы. Внутри неё боковое трение смешивается.</summary>
        public float FlatBlendAngle = 2f;
        /// <summary>Высота над склоном, на которой влияние угла доски пропадает.</summary>
        public float ContactFadeHeight = 0.03f;

        /// <summary>Радиус бокового выреза, метры. 0 — выреза нет.</summary>
        public float SidecutRadius = 15f;
        /// <summary>С этого угла закантовки вырез начинает вести доску по дуге, градусы.</summary>
        public float SidecutEngageStart = 2f;
        /// <summary>С этого угла вырез ведёт доску полностью, градусы.</summary>
        public float SidecutEngageFull = 15f;

        /// <summary>Сопротивление снега на один угол, Н·с²/м².</summary>
        public float AlongDrag = 0.5f;
        public float SideDrag = 5f;
        /// <summary>Сопротивление воздуха на всё тело, Н·с²/м².</summary>
        public float AirDrag = 0.3f;
    }
}