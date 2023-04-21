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
        // 플레이어가 이동한다
        // 입력
        if(Input.GetKey(KeyCode.A))
        {
            // 위치변환
            // 1. 이동 속도 설정 regidbody
            // 2. 위치 자체를 움직임
            transform.position += Vector3.left * Speed * Time.deltaTime;
            // 3. 힘을 가함

        }
        else if (Input.GetKey(KeyCode.D))
        {
            // 위치변환
            transform.position += Vector3.right * Speed * Time.deltaTime;
        }


        // 공격
        // 입력
        if (Input.GetKeyDown(KeyCode.J)) // 눌렀을 때 한 번
        {
            // 출력
            anim.Play("Player_kick");
        }

        // 점프
        // 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 점프
            rigid.velocity = Vector2.up * 30;
        }
    }
}
