using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public Action OnPrame;

    // 시간 계산
    // 최대 시간
    [SerializeField]
    float MaxTime;

    float _currentTime;

    private void Update()
    {
        _currentTime += Time.deltaTime;

        if (MaxTime <= _currentTime) // 최대 시간이 되면 이 아래 코드는 실행 안 시킬거임
            return;

        OnPrame.Invoke();
    }
}
