// IBoardSpawner.cs
using UnityEngine;

public interface IBoardSpawner
{
    void SpawnBlockAt(int x, int y); // 床（blockPrefab）を生成して GridStorage.GridObjects に入れる
    GameObject SpawnOccupant(int cellType, int x, int y); // cellType に応じて Occupant を生成して GridStorage.Occupants に入れる（既存があれば置換）
    //void RemoveOccupantAt(int x, int y); // Occupant を破棄して参照をクリア
}