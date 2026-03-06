using UnityEngine;
using SkyRuins.Board;

namespace SkyRuins
{
    // オブジェクトプール・オブジェクト用
    
    public class PooledObject : MonoBehaviour
    {
        private ObjectPool pool; // このオブジェクトが属するプールへの参照
        public ObjectPool Pool { get => pool; set => pool = value; }

        // オブジェクトをスタックに戻す
        public void Release()
        {
            pool.ReturnToPool(this);
        }
    }
}