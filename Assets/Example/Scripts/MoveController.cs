using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoffeeFramework;

public class MoveController : CoffeeBehaviour
{
    public override void GameUpdate(float time)
    {
        if(InputManager.GetKeyDown(InputManager.Key.a)){
        Debug.Log("Down");
        }
        if(InputManager.GetKeyUp(InputManager.Key.a)){
        Debug.Log("Up");
        }
    }

    public override void GameFixedUpdate(float time)
    {
        if(InputManager.GetKeyDownFixed(InputManager.Key.a)){
        Debug.Log("FixedDown");
        }
        if(InputManager.GetKeyUpFixed(InputManager.Key.a)){
        Debug.Log("FixedUp");
        }
    }
}
