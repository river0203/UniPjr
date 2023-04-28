using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public Action OnPrame;

    // �ð� ���
    // �ִ� �ð�
    [SerializeField]
    float MaxTime;

    float _currentTime;

    private void Update()
    {
        _currentTime += Time.deltaTime;

        if (MaxTime <= _currentTime) // �ִ� �ð��� �Ǹ� �� �Ʒ� �ڵ�� ���� �� ��ų����
            return;

        OnPrame.Invoke();
    }
}
