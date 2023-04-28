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
        // �Է¹ޱ�
        if (Input.GetKey(LeftKey))
        {
            // �̵��ϱ�
            Debug.Log($"Left : {gameObject.name}");
        }
        else if (Input.GetKey(RightKey))
        {
            // �̵��ϱ�
            Debug.Log($"Rigth{gameObject.name}");
        }
        else if (Input.GetKey(JumpKey))
        {
            // �̵��ϱ�
            Debug.Log($"Jump{gameObject.name}");
        }
    }
}
