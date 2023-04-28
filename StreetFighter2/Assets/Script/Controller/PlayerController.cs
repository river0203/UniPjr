using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    KeyCode LeftKey;
    [SerializeField]
    KeyCode RightKey;
    [SerializeField]
    KeyCode JumpKey;

    Timer timer;

    private void Awake()
    {
        timer = GetComponent<Timer>();

        timer.OnPrame -= OnMove;
        timer.OnPrame += OnMove;
    }

    void OnMove()
    {
        // 입력받기
        if (Input.GetKey(LeftKey))
        {
            // 이동하기
            Debug.Log($"Left : {gameObject.name}");
        }
        else if (Input.GetKey(RightKey))
        {
            // 이동하기
            Debug.Log($"Rigth{gameObject.name}");
        }
        else if (Input.GetKey(JumpKey))
        {
            // 이동하기
            Debug.Log($"Jump{gameObject.name}");
        }
    }
}
