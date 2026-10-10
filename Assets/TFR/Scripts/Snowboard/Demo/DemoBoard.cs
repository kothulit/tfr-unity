using UnityEngine;

namespace TFR
{
    public class DemoBoard : MonoBehaviour
    {
        [SerializeField] public Material material;
        [SerializeField] public int meshColumnsNumber = 9; 
        [SerializeField] public int meshRowsNumber = 3;
        [SerializeField] public float ColumnSpacing = 1f;
        [SerializeField] public float RowSpacing = 1f;
        [SerializeField] public float BoardClearance = 0.5f;
    }
}
