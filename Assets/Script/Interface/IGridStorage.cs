// IGridStorage.cs
using UnityEngine;

public interface IGridStorage
{
    int FullSize { get; }
    int CoreSize { get; }

    GameObject[,] GridObjects { get; }
    GameObject[,] Occupants { get; }
    int[,] GridData { get; }

    bool IsValidIndex(int x, int y);
    bool IsInsideCore(int x, int y);

    Vector3 WorldPosition(int x, int y);

    void SetCell(int x, int y, int value, GameObject obj = null);
    int GetCell(int x, int y);
    void ClearCell(int x, int y);
}
