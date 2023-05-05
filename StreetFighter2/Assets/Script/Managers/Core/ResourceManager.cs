using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    // 랩핑
    // 1. 중단점으로 코드 확인
    // 2. 기능 추가
    public T Load<T>(string path) where T : Object 
    {
        T obj = Resources.Load<T>(path);

        if (obj == null)
        {
            Debug.Log($"ResourceManager : Is wrong path : {path}");
            return null;
        }

        return obj;
    }

    public GameObject Instantiate(string path)
    {
        GameObject go = Load<GameObject>($"Prefabs/{path}");

        GameObject.Instantiate(go);

        return go;
    }

    public void Destroy(GameObject go) // 삭제시키는 기능을 가져오는 기능
    {
        GameObject.Destroy(go); // 삭제시키는 기능
    }
}
