using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers _instance = null;
    public static Managers GetInstance() { Init();  return _instance; }

    InputManager _input = new InputManager();

    ResourceManager _resource = new ResourceManager();

    public static ResourceManager GetResource()
    {
        return GetInstance()._resource;
    }

    private void Awake()
    {
        Init();
    }

    static void Init()
    {
        if (_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");

            if (go == null)
            {
                go = new GameObject() { name = "@Managers" };
                go.AddComponent<Managers>();
            }
            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();
        }
    }
}