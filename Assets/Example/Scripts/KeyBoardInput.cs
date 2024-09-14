using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoffeeFramework.InputManager;

public class KeyBoardInput : MonoBehaviour
{
    public KeyCode
       left = KeyCode.LeftArrow,
       right = KeyCode.RightArrow,
       jump = KeyCode.Space;


    public void Awake()
    {
        SetKeyEvent += SetKey;
    }
    private void OnDestroy()
    {
        SetKeyEvent -= SetKey;
    }

    public (Key, bool)[] SetKey()
    {
        return new (Key, bool)[] {
            (Key.left, Input.GetKey(left)),
         (Key.right, Input.GetKey(right)),
         (Key.a, Input.GetKey(jump))};
    }
}
