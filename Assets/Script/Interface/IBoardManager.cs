using UnityEngine;

public interface IBoardManager
{
    int GetGridDataAt(int x, int y);
    void SetGridDataAt(int x, int y, int value);
    void RemoveOccupantAtPublic(int x, int y);
    GameObject GetOccupantObjectAt(int x, int y);
    int GetPlayerX();
    int GetPlayerY();
    GameObject GetPlayerObject();
    void SetPlayerPosition(int x, int y);
    void ConsumeItemAt(int x, int y);
    void MovePlayerTo(int oldX, int oldY, int newX, int newY, GameObject playerGO);
}
