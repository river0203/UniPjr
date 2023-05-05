using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers _instance = null;
    public static Managers Get() { return _instance; }

    InputManager _input = new InputManager();

    private void Awake()
    {
        if (_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");

            if (go == null)
            {
                go = new GameObject() { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            _instance = go.GetComponent<Managers>();
        }
        
    }
}