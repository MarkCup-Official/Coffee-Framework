using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace CoffeeFramework
{
    /// Rely on IOManager  
    /// 
    /// 
    /// <summary>
    ///  
    /// </summary>
    public class Setting
    {
        //public 
        public enum SettingItem
        {
            Vloume,
        }

        public static Setting Instance { get; private set; }

        public T GetSettingItem<T>(SettingItem item)
        {
            try
            {
                return (T)Items[item];
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return default;
        }

        public void InitDefault((SettingItem item, object value)[] values)
        {
            foreach ((SettingItem item, object value) in values)
            {
                Items.Add(item, value);
            }
        }

        public void ReadFormFile(){
            
        }

        //private
        private readonly Dictionary<SettingItem, object> Items = new();
    }
}
