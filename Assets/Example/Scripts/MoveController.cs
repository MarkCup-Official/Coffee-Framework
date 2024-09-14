using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoffeeFramework;

public class MoveController : CoffeeBehaviour
{
    public float speed=10;
    public float jumpSpeed=10;
    Rigidbody2D rb;
    protected override void Awake(){
        base.Awake();
        rb=GetComponent<Rigidbody2D>();
    }

    public override void GameFixedUpdate(float time)
    {
        if(InputManager.GetKey(InputManager.Key.left)){
            rb.velocity=new Vector2(-1*speed,rb.velocity.y);
        }
        if(InputManager.GetKey(InputManager.Key.right)){
            rb.velocity=new Vector2(1*speed,rb.velocity.y);
        }
        if(InputManager.GetKeyDownFixed(InputManager.Key.a)){
            rb.velocity=new Vector2(rb.velocity.x,jumpSpeed);
        }
    }
}
