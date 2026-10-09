using UnityEngine;
using VContainer;

public class Board : MonoBehaviour
{
    [SerializeField] private Material boardMaterial;

    private void Awake()
    {
        CreateBoard();
    }

    public void CreateBoard()
    {
        var boardObject = new GameObject("Deformable Snowboard");
        boardObject.transform.SetParent(transform);
        var filter = boardObject.AddComponent<MeshFilter>();
        var renderer = boardObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = boardMaterial;
    }
}
