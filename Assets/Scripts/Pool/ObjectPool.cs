using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Pool
{
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private uint initPoolSize;
        [SerializeField] private PooledObject objectToPool;
        // コレクション内のプールされたオブジェクトを格納する
        private Stack<PooledObject> stack;

        private void Awake()
        {
            SetupPool();
        }

        // プールを作成する（ラグが目立たないときに呼び出す）
        private void SetupPool()
        {
            stack = new Stack<PooledObject>();
            PooledObject instance = null;
            for (int i = 0; i < initPoolSize; i++)
            {
                instance = Instantiate(objectToPool, this.transform);
                instance.Pool = this;
                instance.gameObject.SetActive(false);
                stack.Push(instance);
            }
        }

        // プールから最初のアクティブなゲームオブジェクトを返す
        public PooledObject GetPooledObject()
        {
            if (stack.Count == 0)
            {
                PooledObject newInstance = Instantiate(objectToPool, this.transform);
                newInstance.Pool = this;
                return newInstance;
            }
            // それ以外の場合は、リストから次のものをグラブする
            PooledObject nextInstance = stack.Pop();
            nextInstance.gameObject.SetActive(true);
            return nextInstance;
        }

        public void ReturnToPool(PooledObject pooledObject)
        {
            stack.Push(pooledObject);
            pooledObject.gameObject.SetActive(false);
            pooledObject.transform.SetParent(this.transform);
        }
    }
}