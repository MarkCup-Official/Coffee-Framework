using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeFramework
{

    /// Using call back function to get input value, this will prevent the delay of input
    /// To set input you should use adapter to respond an input quest in SetKeyEvent
    /// 
    /// Rely on GameManager
    /// 
    ///  <summary>
    /// Every input be get in this project should use this input manager, and this class could be only called on main thread
    /// </summary>
    public class InputManager
    {
        //public
        public enum Key   //Use Joystick as an example
        {
            left,
            right,
            up,
            down,

            a,
            b,
            x,
            y,

            leftX,
            leftY,
            leftStick,

            rightX,
            rightY,
            rightStick,

            leftButton,
            rightButton,
            leftTrigger,
            rightTrigger,

            esc,
            menu,
        }

        /// <summary>
        /// Initialize the singleton if the singleton doesn't exist
        /// </summary>
        public static void Init()
        {
            if (inited)
            {
                return;
            }
            instance = new InputManager();
            inited = true;
        }

        /// <summary>
        /// Get whether a key be pressed now
        /// </summary>
        /// <param name="key">Key code</param>
        /// <returns>Whether a key be pressed now</returns>
        public static bool GetKey(Key key)
        {
            CheckAndInit();
            instance.GetInput();
            return instance.isHold[key];
        }

        /// <summary>
        /// Get whether a key be pressed down in this frame, should only be called in update
        /// </summary>
        /// <param name="key">Key code</param>
        /// <returns>Whether a key be pressed down in this frame</returns>
        public static bool GetKeyDown(Key key)
        {
            CheckAndInit();
            instance.GetInput();
            return instance.isDown[key];
        }

        /// <summary>
        /// Get whether a key be released in this frame, should only be called in update
        /// </summary>
        /// <param name="key">Key code</param>
        /// <returns>Whether a key be released in this frame</returns>
        public static bool GetKeyUp(Key key)
        {
            CheckAndInit();
            instance.GetInput();
            return instance.isUp[key];
        }

        /// <summary>
        /// Get whether a key be pressed down in this frame, should only be called in FixedUpdate.
        /// And only could be use when called GetKey in update before.
        /// </summary>
        /// <param name="key">Key code</param>
        /// <returns>Whether a key be pressed down in this frame</returns>
        public static bool GetKeyDownFixed(Key key)
        {
            CheckAndInit();
            instance.GetInput();
            return instance.isFixedDown[key];
        }

        /// <summary>
        /// Get whether a key be released in this frame, should only be called in FixedUpdate.
        /// And only could be use when called GetKey in update before.
        /// </summary>
        /// <param name="key">Key code</param>
        /// <returns>Whether a key be released in this frame</returns>
        public static bool GetKeyUpFixed(Key key)
        {
            CheckAndInit();
            instance.GetInput();
            return instance.isFixedUp[key];
        }

        public delegate (Key, bool)[] SetKey();

        /// <summary>
        /// Submit input when invoke
        /// </summary>
        public static event SetKey SetKeyEvent;

        //private
        private static InputManager instance = null;

        private static bool inited = false;

        private readonly Dictionary<Key, bool> isHold = new();
        private readonly Dictionary<Key, bool> isDown = new();
        private readonly Dictionary<Key, bool> isUp = new();

        private readonly Dictionary<Key, bool> lastFrameRaw = new();
        private readonly Dictionary<Key, bool> raw = new();

        private int updatedFrame = -1;

        private readonly Dictionary<Key, bool> isFixedDown = new();
        private readonly Dictionary<Key, bool> isFixedUp = new();

        private int fixedUpdatedFrame = -1;

        private readonly Key[] keys = (Key[])Enum.GetValues(typeof(Key));

        private static void CheckAndInit()
        {
            if (!inited)
            {
                Init();
            }
        }

        private InputManager()
        {
            foreach (Key key in keys)
            {
                isHold[key] = false;
                isDown[key] = false;
                isUp[key] = false;
                lastFrameRaw[key] = false;
                raw[key] = false;
                isFixedDown[key] = false;
                isFixedUp[key] = false;
            }
            updatedFrame = -1;
            fixedUpdatedFrame = -1;
        }

        private void GetInput()
        {
            //fixdeUpdate
            int nowFixedUpdateFrame = GameManager.fixedTickCount;
            if (fixedUpdatedFrame != nowFixedUpdateFrame)
            {
                fixedUpdatedFrame=nowFixedUpdateFrame;
                foreach (Key key in keys)
                {
                    isFixedDown[key] = false;
                    isFixedUp[key] = false;
                }
            }

            //update
            int nowUpdateFrame = GameManager.updateTickCount;
            if (updatedFrame != nowUpdateFrame)
            {
                updatedFrame = nowUpdateFrame;
                List<(Key, bool)> rawInput = InvokeInputeEvents();
                foreach (Key key in keys)
                {
                    raw[key] = false;
                }
                foreach (var (key, hold) in rawInput)
                {
                    if (hold)
                    {
                        raw[key] = true;
                    }
                }
                foreach (KeyValuePair<Key, bool> pair in raw)
                {
                    if (pair.Value)
                    {
                        isHold[pair.Key] = true;
                    }
                    else
                    {
                        isHold[pair.Key] = false;
                    }
                    if (!lastFrameRaw[pair.Key] && pair.Value)
                    {
                        isDown[pair.Key] = true;
                        isFixedDown[pair.Key] = true;
                    }
                    else
                    {
                        isDown[pair.Key] = false;
                    }
                    if (lastFrameRaw[pair.Key] && !pair.Value)
                    {
                        isUp[pair.Key] = true;
                        isFixedUp[pair.Key] = true;
                    }
                    else
                    {
                        isUp[pair.Key] = false;
                    }
                    lastFrameRaw[pair.Key] = pair.Value;
                }
            }
        }

        private List<(Key, bool)> InvokeInputeEvents()
        {
            List<(Key, bool)> res = new();
            if (SetKeyEvent != null)
            {
                foreach (SetKey handler in SetKeyEvent.GetInvocationList())
                {
                    try
                    {
                        (Key, bool)[] handlerResults = handler();
                        if (handlerResults != null)
                        {
                            res.AddRange(handlerResults);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Handler threw an exception: {ex.Message}");
                    }
                }
            }
            return res;
        }
    }
}