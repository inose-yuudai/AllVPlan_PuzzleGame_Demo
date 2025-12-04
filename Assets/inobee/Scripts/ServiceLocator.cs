using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmoteOrchestra.Core
{
    /// <summary>
    /// シングルトンを使わずにサービスを管理する
    /// </summary>
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator _instance;
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void Register<T>(T service)
        {
            // インスタンスがない場合は自動生成
            if (_instance == null)
            {
                EnsureInstance();
            }

            Type type = typeof(T);
            if (_instance._services.ContainsKey(type))
            {
                Debug.LogWarning($"Service {type.Name} already registered. Overwriting...");
                _instance._services[type] = service;
            }
            else
            {
                _instance._services.Add(type, service);
            }
        }

        public static T Get<T>()
        {
            if (_instance == null)
            {
                EnsureInstance();
            }

            Type type = typeof(T);
            if (_instance._services.TryGetValue(type, out object service))
            {
                return (T)service;
            }

            Debug.LogWarning($"Service {type.Name} not found!");
            return default;
        }

        public static void Unregister<T>()
        {
            if (_instance == null)
                return;

            Type type = typeof(T);
            if (_instance._services.ContainsKey(type))
            {
                _instance._services.Remove(type);
            }
        }

        // インスタンスが存在しない場合は自動生成
        private static void EnsureInstance()
        {
            if (_instance != null)
                return;

            // シーン内から探す
            _instance = FindObjectOfType<ServiceLocator>();

            // 見つからなければ新規作成
            if (_instance == null)
            {
                GameObject go = new GameObject("ServiceLocator");
                _instance = go.AddComponent<ServiceLocator>();
                DontDestroyOnLoad(go);
                Debug.Log("ServiceLocator was automatically created.");
            }
        }
    }
}