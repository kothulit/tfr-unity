using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.Rendering.DebugUI.Table;

namespace TFR
{
    public class DemoBoardCreator
    {
        readonly DemoBoard demoBoard;
        private int columns => demoBoard.meshColumnsNumber;
        private int rows => demoBoard.meshRowsNumber;
        private float columnSpacing => demoBoard.ColumnSpacing;
        private float rowSpacing => demoBoard.RowSpacing;
        private float boardClearance => demoBoard.BoardClearance;

        public DemoBoardCreator(
            DemoBoard demoBoardm
            )
        {
            this.demoBoard = demoBoardm;
        }

        public void CreateGameObject()
        {
            GameObject boardObject = new GameObject("Deformable board");
            boardObject.transform.localPosition = Vector3.zero;
            var filter = boardObject.AddComponent<MeshFilter>();
            var renderer = boardObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = demoBoard.material;

            var boardMesh = new Mesh { name = "Snowboard calculation mesh" };
            boardMesh.MarkDynamic(); //Меш будет меняться в рантайме

            var vertices = new Vector3[columns * rows];
            var loads = new float[vertices.Length];

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                    vertices[Index(row, column)] = FlatPosition(row, column);
            }

            var triangles = new int[(rows - 1) * (columns - 1) * 6];
            int triangle = 0;
            for (int row = 0; row < rows - 1; row++)
            {
                for (int column = 0; column < columns - 1; column++)
                {
                    int a = Index(row, column);
                    int b = Index(row, column + 1);
                    int c = Index(row + 1, column);
                    int d = Index(row + 1, column + 1);
                    triangles[triangle++] = a;
                    triangles[triangle++] = b;
                    triangles[triangle++] = c;
                    triangles[triangle++] = b;
                    triangles[triangle++] = d;
                    triangles[triangle++] = c;
                }
            }

            boardMesh.vertices = vertices;
            boardMesh.triangles = triangles;
            boardMesh.RecalculateNormals();
            boardMesh.RecalculateBounds();
            filter.sharedMesh = boardMesh;
        }

        private int Index(int row, int column)
        {
            return row * columns + column;
        }

        private Vector3 FlatPosition(int row, int column)
        {
            return new Vector3((column - 4) * columnSpacing, boardClearance, (1 - row) * rowSpacing);
        }
    }
}
