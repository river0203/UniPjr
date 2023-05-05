using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    // ����
    // 1. �ߴ������� �ڵ� Ȯ��
    // 2. ��� �߰�
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

    public void Destroy(GameObject go) // ������Ű�� ����� �������� ���
    {
        GameObject.Destroy(go); // ������Ű�� ���
    }
}
