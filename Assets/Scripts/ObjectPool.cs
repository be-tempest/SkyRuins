using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace SkyRuins
{
    // オブジェクトプール・プール管理用
    
    public class ObjectPool : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private uint initPoolSize; // プールの初期サイズ
        [SerializeField] private PooledObject objectToPool; // プールするオブジェクトのプレハブ

        private Stack<PooledObject> stack; // コレクション内のプールされたオブジェクトを格納する

        private void Awake()
        {
            SetupPool();
        }

        // プールを作成する
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
            // プールが空なら新しいものを生成
            if (stack.Count == 0)
            {
                PooledObject newInstance = Instantiate(objectToPool, this.transform);
                newInstance.Pool = this;
                return newInstance;
            }

            // それ以外の場合はプールから次のものを返す
            PooledObject nextInstance = stack.Pop();
            nextInstance.gameObject.SetActive(true);
            return nextInstance;
        }

        // プールにオブジェクトを戻す
        public void ReturnToPool(PooledObject pooledObject)
        {
            stack.Push(pooledObject);
            pooledObject.gameObject.SetActive(false);
            pooledObject.transform.SetParent(this.transform);
        }
    }
}