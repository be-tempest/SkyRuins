using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideManager : MonoBehaviour, ISlideManager
{
    public GridStorage gridStorage;
    public BoardSpawner boardSpawner;
    public BoardManager3 boardManager;

    public enum Direction { Left, Up, Right, Down }

    private int coreSize => gridStorage != null ? gridStorage.CoreSize : 7;
    private int fullSize => gridStorage != null ? gridStorage.FullSize : 9;

    void Awake()
    {
        if (gridStorage == null) Debug.LogError("[SlideManager] gridStorage not assigned!");
    }

    public IEnumerator SlideInsertedBlocksCoroutine(List<(int x, int y)> insertedBlocks, float duration, Action<SlideResult> onComplete = null)
    {
        var result = new SlideResult();
        if (gridStorage == null)
        {
            Debug.LogError("[SlideManager] Cannot slide because gridStorage is null.");
            result.Success = false;
            onComplete?.Invoke(result);
            yield break;
        }

        var grid = gridStorage.GridObjects;
        var occupants = gridStorage.Occupants;
        var gridData = gridStorage.GridData;

        // snapshot before
        int[,] before = new int[fullSize, fullSize];
        for (int x = 0; x < fullSize; x++) for (int y = 0; y < fullSize; y++) before[x, y] = gridData[x, y];

        var groups = new Dictionary<Direction, List<(int x,int y)>>()
        {
            { Direction.Left, new List<(int,int)>() },
            { Direction.Up, new List<(int,int)>() },
            { Direction.Right, new List<(int,int)>() },
            { Direction.Down, new List<(int,int)>() }
        };

        foreach (var (x,y) in insertedBlocks)
        {
            if (x == 0) groups[Direction.Left].Add((x,y));
            else if (x == coreSize + 1) groups[Direction.Right].Add((x,y));
            else if (y == 0) groups[Direction.Down].Add((x,y));
            else if (y == coreSize + 1) groups[Direction.Up].Add((x,y));
        }

        Direction[] order = new[] { Direction.Left, Direction.Up, Direction.Right, Direction.Down };

        foreach (var dir in order)
        {
            if (gridStorage == null) { result.Success = false; onComplete?.Invoke(result); yield break; }
            var list = groups[dir];
            if (list == null || list.Count == 0) continue;

            var doneFlags = new List<bool>(new bool[list.Count]);

            for (int i = 0; i < list.Count; i++)
            {
                if (gridStorage == null) { doneFlags[i] = true; continue; }
                int localIndex = i;
                var (x,y) = list[localIndex];
                IEnumerator routine;
                if (dir == Direction.Left || dir == Direction.Right)
                {
                    int row = y;
                    routine = SlideRowAnimated(row, dir, duration);
                }
                else
                {
                    int col = x;
                    routine = SlideColumnAnimated(col, dir, duration);
                }
                StartCoroutine(RunAndFlag(routine, doneFlags, localIndex));
            }

            while (true)
            {
                bool allDone = true;
                for (int k = 0; k < doneFlags.Count; k++) if (!doneFlags[k]) { allDone = false; break; }
                if (allDone) break;
                yield return null;
            }

            yield return new WaitForSeconds(0.03f);
        }

        // snapshot after
        int[,] after = new int[fullSize, fullSize];
        for (int x = 0; x < fullSize; x++) for (int y = 0; y < fullSize; y++) after[x, y] = gridData[x, y];

        // build change list
        for (int x = 0; x < fullSize; x++)
        {
            for (int y = 0; y < fullSize; y++)
            {
                if (before[x,y] != after[x,y])
                {
                    result.Changes.Add(new CellChange(new Vector2Int(x,y), after[x,y]));
                }
            }
        }

        // find PLAYER pos
        int px=-1, py=-1;
        for (int x = 0; x < fullSize; x++)
        {
            for (int y = 0; y < fullSize; y++)
            {
                if (gridData[x,y] == boardManager.PLAYER)
                {
                    px = x; py = y;
                    // don't break: prefer last found in case duplicates (shouldn't happen)
                }
            }
        }
        result.PlayerPos = new Vector2Int(px, py);

        onComplete?.Invoke(result);
        yield break;
    }

    private IEnumerator RunAndFlag(IEnumerator routine, List<bool> doneFlags, int index)
    {
        yield return StartCoroutine(routine);
        doneFlags[index] = true;
    }

    private IEnumerator MoveTransformOverTime(Transform t, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t01 = Mathf.Clamp01(elapsed / duration);
            t.position = Vector3.Lerp(from, to, t01);
            yield return null;
        }
        t.position = to;
    }

    private IEnumerator SlideRowAnimated(int row, Direction dir, float duration)
    {
        var grid = gridStorage.GridObjects;
        var occupants = gridStorage.Occupants;
        var gridData = gridStorage.GridData;

        var movers = new List<(GameObject go, Vector3 from, Vector3 to)>();
        if (dir == Direction.Left)
        {
            for (int x = 0; x < fullSize; x++)
            {
                if (grid[x,row] != null)
                {
                    Vector3 from = grid[x,row].transform.position;
                    Vector3 to = gridStorage.WorldPosition(x+1, row);
                    movers.Add((grid[x,row], from, to));
                }
            }
        }
        else // Right
        {
            for (int x = 0; x < fullSize; x++)
            {
                if (grid[x,row] != null)
                {
                    Vector3 from = grid[x,row].transform.position;
                    Vector3 to = gridStorage.WorldPosition(x-1, row);
                    movers.Add((grid[x,row], from, to));
                }
            }
        }

        var routines = new List<Coroutine>();
        foreach (var m in movers) routines.Add(StartCoroutine(MoveTransformOverTime(m.go.transform, m.from, m.to, duration)));
        foreach (var c in routines) yield return c;

        if (dir == Direction.Left)
        {
            for (int x = fullSize - 1; x >= 1; x--)
            {
                grid[x,row] = grid[x-1,row];
                occupants[x,row] = occupants[x-1,row];
                gridData[x,row] = gridData[x-1,row];
            }
            grid[0,row] = null; occupants[0,row] = null; gridData[0,row] = 0;
        }
        else // Right
        {
            for (int x = 0; x <= fullSize - 2; x++)
            {
                grid[x,row] = grid[x+1,row];
                occupants[x,row] = occupants[x+1,row];
                gridData[x,row] = gridData[x+1,row];
            }
            grid[fullSize-1,row] = null; occupants[fullSize-1,row] = null; gridData[fullSize-1,row] = 0;
        }

        yield break;
    }

    private IEnumerator SlideColumnAnimated(int col, Direction dir, float duration)
    {
        var grid = gridStorage.GridObjects;
        var occupants = gridStorage.Occupants;
        var gridData = gridStorage.GridData;

        var movers = new List<(GameObject go, Vector3 from, Vector3 to)>();
        if (dir == Direction.Up)
        {
            for (int y = 0; y < fullSize; y++)
            {
                if (grid[col,y] != null)
                {
                    Vector3 from = grid[col,y].transform.position;
                    Vector3 to = gridStorage.WorldPosition(col, y-1);
                    movers.Add((grid[col,y], from, to));
                }
            }
        }
        else // Down
        {
            for (int y = 0; y < fullSize; y++)
            {
                if (grid[col,y] != null)
                {
                    Vector3 from = grid[col,y].transform.position;
                    Vector3 to = gridStorage.WorldPosition(col, y+1);
                    movers.Add((grid[col,y], from, to));
                }
            }
        }

        var routines = new List<Coroutine>();
        foreach (var m in movers) routines.Add(StartCoroutine(MoveTransformOverTime(m.go.transform, m.from, m.to, duration)));
        foreach (var c in routines) yield return c;

        if (dir == Direction.Up)
        {
            for (int y = 1; y <= fullSize - 1; y++)
            {
                grid[col,y-1] = grid[col,y];
                occupants[col,y-1] = occupants[col,y];
                gridData[col,y-1] = gridData[col,y];
            }
            grid[col,fullSize-1] = null; occupants[col,fullSize-1] = null; gridData[col,fullSize-1] = 0;
        }
        else // Down
        {
            for (int y = fullSize - 2; y >= 0; y--)
            {
                grid[col,y+1] = grid[col,y];
                occupants[col,y+1] = occupants[col,y];
                gridData[col,y+1] = gridData[col,y];
            }
            grid[col,0] = null; occupants[col,0] = null; gridData[col,0] = 0;
        }

        yield break;
    }
}