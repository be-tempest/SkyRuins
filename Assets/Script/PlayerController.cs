using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private int x;
    private int y;
    private BoardManager board;

    // BoardManager が生成後に呼ぶ Init を想定
    public void Init(int startX, int startY, BoardManager manager)
    {
        x = startX;
        y = startY;
        board = manager;

        // すでに board.PlaceObjectOnBlock でブロックの子として生成済みのはずなので、
        // 位置合わせだけ行う
        transform.SetParent(board.grid[x, y].transform);
        transform.localPosition = Vector3.up * 0.5f;
    }

    void Update()
    {
        if (board == null) return;
        if (board.phase != BoardManager.GamePhase.PlayerMove) return;

        int newX = x;
        int newY = y;

        if (Input.GetKeyDown(KeyCode.UpArrow)) newY++;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) newY--;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) newX--;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) newX++;
        else return;

        // 範囲チェック（1..coreSize）
        if (newX < 1 || newY < 1 || newX > board.coreSize || newY > board.coreSize) return;

        int cell = board.gridData[newX, newY];

        if (cell == 2) return; // 障害物は移動不可

        if (cell == 1)
        {
            // アイテム取得
            board.ConsumeItemAt(newX, newY);
            Debug.Log("アイテムを取得しました！");
        }

        // BoardManager に reparent と配列更新を任せる
        board.MovePlayerTo(x, y, newX, newY, this.gameObject);

        // 内部座標更新
        x = newX;
        y = newY;

        // 移動したらフェーズを進める（Slideへ）
        board.phase = BoardManager.GamePhase.Slide;
    }
}
