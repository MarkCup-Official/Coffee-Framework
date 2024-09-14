using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

namespace CoffeeFramework
{
    /// <summary>
    /// Sigleton GameManager
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        //public

        public static GameManager Instance { get; private set; }

        public static int fixedTickCount{get;private set;}
        public static int updateTickCount{get;private set;}

        /// <summary>
        /// To regist a coffee behaciour object
        /// </summary>
        /// <param name="update"></param>
        /// <param name="fixedUpdate"></param>
        public static void RegistBehaviour(GameUpdate update = null, GameFixedUpdate fixedUpdate = null, GamePause pause = null)
        {
            if (Instance)
            {
                if (update != null)
                {
                    Instance.OnUpdate += update;
                }
                if (fixedUpdate != null)
                {
                    Instance.OnFixedUpdate += fixedUpdate;
                }
                if (pause != null)
                {
                    Instance.OnPauseChange += pause;
                }
            }
            else
            {
                holdRegists.Add((update, fixedUpdate, pause));
            }
        }

        /// <summary>
        /// whether to replace old exist game manager
        /// </summary>
        public bool replace = false;

        /// <summary>
        /// whether to inherit gameobjects from old game manager when replacing    
        /// </summary>
        public bool inherit = false;

        /// <summary>
        /// the time between tow fixed updates
        /// </summary>
        public float fixedUpdateTimeGap = 0.02f;

        /// <summary>
        /// Game Update delegate
        /// </summary>
        /// <param name="time">as same as Time.deltaTime</param>
        public delegate void GameUpdate(float time);

        /// <summary>
        /// Game Fixed Update delegate
        /// </summary>
        /// <param name="time">the fixed update gap time</param>
        public delegate void GameFixedUpdate(float time);

        /// <summary>
        /// pause delegate
        /// </summary>
        /// <param name="isPause"></param>
        public delegate void GamePause(bool isPause);

        //private

        protected void Destroy()
        {
            Debug.Log("Old game manager is being destroying.");
            Destroy(gameObject);
        }

        private static List<(GameUpdate, GameFixedUpdate, GamePause)> holdRegists = new();

        private event GameUpdate OnUpdate;

        private event GameFixedUpdate OnFixedUpdate;

        private event GamePause OnPauseChange;

        private void Awake()
        {
            if (Instance != null)
            {
                if (replace)
                {
                    Debug.Log("Destroying old game manager.");
                    Init(Instance);
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
                Init();
                Instance = this;
            }
        }

        private void Init()
        {
            HandleHold();
        }

        private void Init(GameManager oldManager)
        {
            foreach (var update in oldManager.OnUpdate.GetInvocationList())
            {
                OnUpdate += (GameUpdate)update;
            }
            foreach (var fixdeUpdate in oldManager.OnFixedUpdate.GetInvocationList())
            {
                OnFixedUpdate += (GameFixedUpdate)fixdeUpdate;
            }
            HandleHold();
        }

        private void HandleHold()
        {
            foreach ((var update, var fixedUpdate, var pause) in holdRegists)
            {
                if (update != null)
                {
                    OnUpdate += update;
                }
                if (fixedUpdate != null)
                {
                    OnFixedUpdate += fixedUpdate;
                }
                if (pause != null)
                {
                    OnPauseChange += pause;
                }
            }
        }

        private float timmer = 0;

        private void Update()
        {
            timmer += Time.deltaTime;
            while (timmer > fixedUpdateTimeGap)
            {
                timmer -= fixedUpdateTimeGap;
                OnFixedUpdate?.Invoke(fixedUpdateTimeGap);
                fixedTickCount+=1;
            }
            OnUpdate?.Invoke(Time.deltaTime);
            updateTickCount+=1;
        }
    }

}
