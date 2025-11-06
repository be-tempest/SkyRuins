using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISlideManager
{
    IEnumerator SlideInsertedBlocksCoroutine(List<(int x, int y)> insertedBlocks, float duration, Action<SlideResult> onComplete = null);
}

[Serializable]
public class SlideResult
{
    public List<CellChange> Changes = new List<CellChange>();
    public Vector2Int PlayerPos = new Vector2Int(-1, -1);
    public bool Success = true;
}

[Serializable]
public class CellChange
{
    public Vector2Int Pos;
    public int NewValue;
    public CellChange(Vector2Int pos, int newValue) { Pos = pos; NewValue = newValue; }
}