using UnityEngine;

public class BoardData : MonoBehaviour
{
    // 盤面サイズ
    [SerializeField] private int _fullSize = 9;
    public int fullSize => _fullSize;

    [SerializeField] private int _coreSize = 7;
    public int coreSize => _coreSize;

    // 盤面データ配列
    private int[,] _gridData;
    public int[,] gridData => _gridData;

    private GameObject[,] _gridObjects;
    public GameObject[,] gridObjects => _gridObjects;

    private GameObject[,] _occupants;
    public GameObject[,] occupants => _occupants;

    // GridData 情報
    [SerializeField] private int _playerNum = 1;
    public int playerNum => _playerNum;

    [SerializeField] private int _obstacleNum = 2;
    public int obstacleNum => _obstacleNum;

    [SerializeField] private int _item1Num = 3;
    public int item1Num => _item1Num;

    [SerializeField] private int _item2Num = 4;
    public int item2Num => _item2Num;

    [SerializeField] private int _item3Num = 5;
    public int item3Num => _item3Num;


    // Prefab
    [Header("Prefabs")]
    [SerializeField] private GameObject _blockPrefab;
    public GameObject blockPrefab => _blockPrefab;

    [SerializeField] private GameObject _obstaclePrefab;
    public GameObject obstaclePrefab => _obstaclePrefab;

    [SerializeField] private GameObject[] _itemPrefabs = new GameObject[3];
    public GameObject[] itemPrefabs => _itemPrefabs;

    public void Init()
    {
        int expected = _coreSize + 2;
        if (_fullSize != expected)
        {
            _fullSize = expected;
            Debug.Log($"[GridStorage] fullSize adjusted to coreSize + 2 = {fullSize}");
        }

        _gridObjects = new GameObject[_fullSize, _fullSize];
        _occupants = new GameObject[_fullSize, _fullSize];
        _gridData = new int[_fullSize, _fullSize];
    }
    
    // 範囲チェック
    public bool IsValidIndex(int x, int y)
    {
        return x >= 0 && y >= 0 && x < _fullSize && y < _fullSize;
    }

    public bool IsInsideCore(int x, int y)
    {
        return x >= 1 && y >= 1 && x <= _coreSize && y <= _coreSize;
    }

    public void SetGridData(int num, int x, int y)
    {
        if (!IsValidIndex(x, y)) return;
        _gridData[x, y] = num;
    }

    public void SetGridObjects(GameObject obj, int x, int y)
    {
        if (!IsValidIndex(x, y)) return;
        _gridObjects[x, y] = obj;
    }

    public void SetOccupants(GameObject obj, int x, int y)
    {
        if (!IsValidIndex(x, y)) return;
        _occupants[x, y] = obj;
    }
}
