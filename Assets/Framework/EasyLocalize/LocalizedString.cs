using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class LocalizedString
{
    /// <summary>
    /// 根据当前语言返回文本。
    /// 运行时通过 EasyLocalizeManager 来获取；编辑器下如果没有 Manager，则直接使用本地列表/回退文本。
    /// </summary>
    public string str
    {
        get
        {
            var manager = EasyLocalizeManager.GetInstance();
            if (manager != null)
            {
                return manager.GetLocalizedString(this);
            }

            // 没有 Manager（如纯编辑器环境），退化为：当前语言无值则返回第一种语言，否则显示 Text Lost
            if (strings != null && strings.Count > 0)
            {
                // 优先返回第一个有内容的语言
                foreach (var item in strings)
                {
                    if (!string.IsNullOrEmpty(item.value))
                        return item.value;
                }
            }

            return $"Text Lost: {id}";
        }
    }
    public string id;
    public List<LocalizedStringItem> strings;

    [System.Serializable]
    public class LocalizedStringItem
    {
        public string language;
        public string value;
    }

    public LocalizedString()
    {
        this.strings = new List<LocalizedStringItem>();
    }
}
