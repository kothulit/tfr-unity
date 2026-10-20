using UnityEngine;

namespace TFR.Board
{
    /// <summary>
    /// Рисует прямоугольник BoardPhysicsModel и смещение его углов от плоскости.
    /// </summary>
    public sealed class BoardPhysicsView : MonoBehaviour
    {
        [SerializeField] Material lineMaterial;
        [SerializeField] float lineWidth = 0.01f;
        [SerializeField] float markerSize = 0.04f;
        /// <summary>Только для наглядности: реальные отклонения в сантиметрах почти не видны.</summary>
        [SerializeField] float heightScale = 3f;
        [SerializeField] Color planeColor = new(1f, 1f, 1f, 0.3f);
        [SerializeField] Color boardColor = Color.cyan;
        [SerializeField] Color downColor = Color.red;
        [SerializeField] Color upColor = Color.green;

        BoardPhysicsModel model;
        LineRenderer planeLine;
        LineRenderer boardLine;
        readonly LineRenderer[] stems = new LineRenderer[4];
        readonly Renderer[] markers = new Renderer[4];

        public void Bind(BoardPhysicsModel boardModel)
        {
            model = boardModel;
            if (lineMaterial == null)
                lineMaterial = new Material(Shader.Find("Sprites/Default"));

            planeLine = CreateLine("Plane", 4, true, planeColor);
            boardLine = CreateLine("Board", 4, true, boardColor);

            for (int i = 0; i < 4; i++)
            {
                stems[i] = CreateLine($"Stem{i}", 2, false, Color.white);
                markers[i] = CreateMarker($"Corner{(BoardCorner)i}");
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 p = model.Corners[i].PlanePosition;
                planeLine.SetPosition(i, new Vector3(p.x, 0f, p.y));
            }
        }

        void LateUpdate()
        {
            if (model == null)
                return;

            for (int i = 0; i < 4; i++)
            {
                BoardContactPoint corner = model.Corners[i];
                Vector3 plane = new(corner.PlanePosition.x, 0f, corner.PlanePosition.y);
                Vector3 point = plane + Vector3.up * corner.Height * heightScale;

                boardLine.SetPosition(i, point);
                stems[i].SetPosition(0, plane);
                stems[i].SetPosition(1, point);

                float t = model.MaxDeviation > 0f ? corner.Height / model.MaxDeviation : 0f;
                Color color = t < 0f
                    ? Color.Lerp(Color.white, downColor, -t)
                    : Color.Lerp(Color.white, upColor, t);

                stems[i].startColor = stems[i].endColor = color;
                markers[i].transform.localPosition = point;
                markers[i].material.color = color;

                markers[i].material.color = Color.Lerp(Color.gray, color, corner.Contact);
            }
        }

        LineRenderer CreateLine(string lineName, int count, bool loop, Color color)
        {
            var child = new GameObject(lineName);
            child.transform.SetParent(transform, false);

            var line = child.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = loop;
            line.positionCount = count;
            line.widthMultiplier = lineWidth;
            line.sharedMaterial = lineMaterial;
            line.startColor = line.endColor = color;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return line;
        }

        Renderer CreateMarker(string markerName)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = markerName;
            // Коллайдер на дочернем объекте вошёл бы в составное тело Rigidbody игрока.
            Destroy(marker.GetComponent<Collider>());
            marker.transform.SetParent(transform, false);
            marker.transform.localScale = Vector3.one * markerSize;
            return marker.GetComponent<Renderer>();
        }
    }
}