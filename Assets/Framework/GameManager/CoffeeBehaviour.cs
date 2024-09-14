using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeFramework
{
public class CoffeeBehaviour : MonoBehaviour
{
    //private
    protected virtual void Awake()
    {
        Regist();
    }

    private void Regist()
    {
        //test whether functions are overrided
        var type=GetType();
        var updateMethodInfo=type.GetMethod("GameUpdate");
        GameManager.GameUpdate updateMethod=null;
        if(updateMethodInfo!=updateMethodInfo.GetBaseDefinition()){
            updateMethod=GameUpdate; 
        }
        var fixedUpdateMethodInfo=type.GetMethod("GameFixedUpdate");
        GameManager.GameFixedUpdate fixedUpdateMethod=null;
        if(fixedUpdateMethodInfo!=fixedUpdateMethodInfo.GetBaseDefinition()){
            fixedUpdateMethod=GameFixedUpdate; 
        }
        var pauseMethodInfo=type.GetMethod("GamePause");
        GameManager.GamePause pauseMethod=null;
        if(pauseMethodInfo!=pauseMethodInfo.GetBaseDefinition()){
            pauseMethod=GamePause; 
        }

        GameManager.RegistBehaviour(updateMethod,fixedUpdateMethod,pauseMethod);
    }

    public virtual void GameUpdate(float time)
    {

    }

    public virtual void GameFixedUpdate(float time)
    {

    }

    public virtual void GamePause(bool isPause)
    {

    }
}
}