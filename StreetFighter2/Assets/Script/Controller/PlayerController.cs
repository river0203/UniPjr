using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;

    [SerializeField]
    private float Speed;

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // �÷��̾ �̵��Ѵ�
        // �Է�
        if(Input.GetKey(KeyCode.A))
        {
            // ��ġ��ȯ
            // 1. �̵� �ӵ� ���� regidbody
            // 2. ��ġ ��ü�� ������
            transform.position += Vector3.left * Speed * Time.deltaTime;
            // 3. ���� ����

        }
        else if (Input.GetKey(KeyCode.D))
        {
            // ��ġ��ȯ
            transform.position += Vector3.right * Speed * Time.deltaTime;
        }


        // ����
        // �Է�
        if (Input.GetKeyDown(KeyCode.J)) // ������ �� �� ��
        {
            // ���
            anim.Play("Player_kick");
        }

        // ����
        // �Է�
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // ����
            rigid.velocity = Vector2.up * 30;
        }
    }
}
