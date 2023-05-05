using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triagle : MonoBehaviour
{


    void Start()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        sprite.sprite = Managers.GetResource().Load<Sprite>("0007");
    }
}
