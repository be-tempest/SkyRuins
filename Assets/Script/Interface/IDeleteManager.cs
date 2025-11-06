using System.Collections.Generic;

public interface IDeleteManager
{
    bool DeleteBlocks(List<(int x, int y)> insertedBlocks);
}