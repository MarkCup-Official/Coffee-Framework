using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sigleton GameManager
/// </summary>
public class GameManager : MonoBehaviour
{
    //public
    public static GameManager Instance { get; private set; }

    public bool replaceOldGameManager = false;

    public float fixedUpdateTimeGap = 0.02f;

    public delegate void GameUpdate(float time);
    public event GameUpdate OnUpdate;
    public delegate void GameFixedUpdate(float time);
    public event GameFixedUpdate OnFixedUpdate;

    public delegate void Pause(bool isPause);
    public event GameUpdate OnPauseChange;

    protected void Destroy()
    {
        Debug.Log("Old game manager is being destroying.");
        Destroy(gameObject);
    }

    //private
    private void Awake()
    {
        if (Instance != null)
        {
            if (replaceOldGameManager)
            {
                Debug.Log("Destroying old game manager.");
                Instance.Destroy();
                Instance = this;
            }
            else
            {
                Debug.Log("There are already have a GameManager.");
                Destroy();
            }
        }
        else
        {
            Instance = this;
        }
    }

    private float timmer = 0;

    private void Update()
    {
        timmer += Time.deltaTime;
        while (timmer > fixedUpdateTimeGap)
        {
            timmer -= fixedUpdateTimeGap;
            OnFixedUpdate.Invoke(fixedUpdateTimeGap);
        }
        OnUpdate.Invoke(Time.deltaTime);
    }
}
