using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
{
    public Action KeyAction = null;
    public Action<Define.MouseEvent> MouseEvent = null;

    bool _pressed = false;
    float _pressedTime = 0;

    public void OnUpdate()
    {
        // UI�̺�Ʈ
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Ű����
        if (Input.anyKey && KeyAction != null)
            KeyAction.Invoke();

        // ���콺
        if (MouseEvent != null)
        {
            if (Input.GetMouseButton(0))
            {
                if (!_pressed)
                {
                    MouseEvent.Invoke(Define.MouseEvent.PointerDown);
                    _pressedTime = Time.time;
                }
                MouseEvent.Invoke(Define.MouseEvent.Press);
                _pressed = true;
            }
            else
            {
                if (_pressed)
                {
                    if (Time.time < _pressedTime + 0.2f)
                    {
                        MouseEvent.Invoke(Define.MouseEvent.Click);
                    }
                    MouseEvent.Invoke(Define.MouseEvent.PointerUp);
                }
                _pressed = false;
                _pressedTime = 0;
            }
        }    
    }

    public void Clear()
    {
        KeyAction = null;
        MouseEvent = null;
    }
}
