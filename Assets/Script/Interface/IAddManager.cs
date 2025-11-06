using System.Collections.Generic;

public interface IAddManager
{
    List<(int x, int y)> AddBlocks(Dictionary<BoardManager3.Direction, int> addCount);
}