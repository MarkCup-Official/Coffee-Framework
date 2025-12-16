using System.Collections.Generic;
using System.IO;
using UnityEngine;

// M
public class EasyLocalizeManager
{
    private static EasyLocalizeManager instance;
    public static EasyLocalizeManager GetInstance()
    {
        if (instance == null)
        {
            Init();
        }
        return instance;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        instance = new EasyLocalizeManager();
    }

    /// <summary>
    /// 当前语言（如 "cn"、"en"）
    /// </summary>
    private string language = "cn";

    /// <summary>
    /// 支持的语言列表，顺序与 CSV 中的列对应。
    /// </summary>
    private static readonly string[] SupportedLanguages = { "cn", "en" };

    /// <summary>
    /// 所有本地化数据：id -> (language->value)
    /// </summary>
    private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>();

    /// <summary>
    /// 下一个可用的未命名 ID 序号：unnamed_0, unnamed_1, ...
    /// </summary>
    private int nextUnnamedIndex = 0;

    /// <summary>
    /// 语言切换回调：参数为新语言代码。
    /// </summary>
    public System.Action<string> OnLanguageChanged;

    private const string PlayerPrefsLanguageKey = "EasyLocalizeManager.language";
    private const string CsvFileName = "easy_localize.csv";

    private string CsvPath
    {
        get
        {
            // Assets/Framework/EasyLocalize/easy_localize.csv
            return Path.Combine(Application.dataPath, "Resources/EasyLocalize/" + CsvFileName);
        }
    }

    private EasyLocalizeManager()
    {
        this.language = PlayerPrefs.GetString(PlayerPrefsLanguageKey, "cn");
        LoadFromCsv();
        PlayerPrefs.SetString(PlayerPrefsLanguageKey, this.language);
        PlayerPrefs.Save();

        Debug.Log("<color=green>[EasyLocalizeManager]</color> initialized with language: " + this.language);
    }

    /// <summary>
    /// 获取当前语言代码。
    /// </summary>
    public string GetCurrentLanguage()
    {
        return language;
    }

    /// <summary>
    /// 设置当前语言并触发回调。
    /// </summary>
    public void SetLanguage(string language)
    {
        this.language = language;
        PlayerPrefs.SetString(PlayerPrefsLanguageKey, this.language);
        PlayerPrefs.Save();
        Debug.Log("<color=green>[EasyLocalizeManager]</color> language changed to: " + this.language);
        OnLanguageChanged?.Invoke(this.language);
    }

    /// <summary>
    /// 编辑器使用：获得支持的语言列表。
    /// </summary>
    public static string[] GetSupportedLanguagesEditor()
    {
        return SupportedLanguages;
    }

    /// <summary>
    /// 生成一个新的唯一 ID（仅编辑器下调用）。
    /// </summary>
    public string GenerateNewId()
    {
        // 生成 unnamed_0, unnamed_1, ...
        string id;
        do
        {
            id = $"unnamed_{nextUnnamedIndex}";
            nextUnnamedIndex++;
        } while (entries.ContainsKey(id));

        if (!entries.ContainsKey(id))
        {
            entries[id] = new Entry
            {
                values = new Dictionary<string, string>()
            };
        }

        Debug.Log($"<color=cyan>[EasyLocalizeManager]</color> GenerateNewId -> 新增条目, id = {id}");
        SaveToCsv();
        return id;
    }

    /// <summary>
    /// 注册或更新一个本地化条目（编辑器在 Inspector 修改时调用）。
    /// </summary>
    public void RegisterEntry(string id, List<LocalizedString.LocalizedStringItem> items)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("<color=yellow>[EasyLocalizeManager]</color> RegisterEntry 被调用但 id 为空，已忽略。");
            return;
        }

        if (!entries.TryGetValue(id, out var entry))
        {
            entry = new Entry
            {
                values = new Dictionary<string, string>()
            };
            entries[id] = entry;
            Debug.Log($"<color=cyan>[EasyLocalizeManager]</color> RegisterEntry -> 新增条目, id = {id}");
        }
        else
        {
            Debug.Log($"<color=cyan>[EasyLocalizeManager]</color> RegisterEntry -> 更新条目内容, id = {id}");
        }

        if (items != null)
        {
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.language))
                    continue;

                if (!entry.values.ContainsKey(item.language))
                {
                    entry.values.Add(item.language, item.value ?? string.Empty);
                }
                else
                {
                    entry.values[item.language] = item.value ?? string.Empty;
                }
            }
        }

        // 更新 unnamed 计数（如果是 unnamed_ 前缀）
        if (id.StartsWith("unnamed_"))
        {
            int index;
            if (int.TryParse(id.Substring("unnamed_".Length), out index))
            {
                if (index >= nextUnnamedIndex)
                    nextUnnamedIndex = index + 1;
            }
        }

        SaveToCsv();
    }

    /// <summary>
    /// 是否存在指定 id 的条目（编辑器用）。
    /// </summary>
    public bool HasEntry(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return entries.ContainsKey(id);
    }

    /// <summary>
    /// 获取某个 id 下所有语言对应的值，供编辑器同步到 LocalizedString（如果不存在则返回 null）。
    /// </summary>
    public List<LocalizedString.LocalizedStringItem> GetItems(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (!entries.TryGetValue(id, out var entry) || entry.values == null)
            return null;

        var list = new List<LocalizedString.LocalizedStringItem>();
        foreach (var kv in entry.values)
        {
            list.Add(new LocalizedString.LocalizedStringItem
            {
                language = kv.Key,
                value = kv.Value
            });
        }
        return list;
    }

    /// <summary>
    /// 删除一个 id 对应的条目（用于改名时清理旧名字）。
    /// </summary>
    public void RemoveEntry(string id)
    {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("<color=yellow>[EasyLocalizeManager]</color> RemoveEntry 被调用但 id 为空，已忽略。");
                return;
            }
        if (entries.Remove(id))
        {
                Debug.Log($"<color=red>[EasyLocalizeManager]</color> RemoveEntry -> 删除条目, id = {id}");
            SaveToCsv();
        }
            else
            {
                Debug.LogWarning($"<color=yellow>[EasyLocalizeManager]</color> RemoveEntry -> 未找到要删除的条目, id = {id}");
            }
    }

    /// <summary>
    /// 重命名一个条目的 ID（用于在 Inspector 中修改 ID 文本）。
    /// </summary>
    public void RenameEntry(string oldId, string newId, List<LocalizedString.LocalizedStringItem> items)
    {
            if (string.IsNullOrEmpty(newId))
            {
                Debug.LogWarning("<color=yellow>[EasyLocalizeManager]</color> RenameEntry 被调用但 newId 为空，已忽略。");
            return;
            }

        if (oldId == newId)
        {
            // 只是重新保存内容
            RegisterEntry(newId, items);
            return;
        }

        // 如果旧 ID 存在，先取出其数据
        Entry entry = null;
        if (!string.IsNullOrEmpty(oldId) && entries.TryGetValue(oldId, out var oldEntry))
        {
            entry = oldEntry;
            entries.Remove(oldId);
            Debug.Log($"<color=cyan>[EasyLocalizeManager]</color> RenameEntry -> 重命名条目, oldId = {oldId}, newId = {newId}");
        }

        if (entry == null)
        {
            entry = new Entry
            {
                values = new Dictionary<string, string>()
            };
            Debug.Log($"<color=cyan>[EasyLocalizeManager]</color> RenameEntry -> 旧 ID 不存在, 通过新 ID 创建新条目, newId = {newId}");
        }

        // 覆盖为 items 中的内容
        entry.values.Clear();
        if (items != null)
        {
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.language))
                    continue;

                entry.values[item.language] = item.value ?? string.Empty;
            }
        }

        entries[newId] = entry;

        // 维护 unnamed 计数
        if (newId.StartsWith("unnamed_"))
        {
            int index;
            if (int.TryParse(newId.Substring("unnamed_".Length), out index))
            {
                if (index >= nextUnnamedIndex)
                    nextUnnamedIndex = index + 1;
            }
        }

        SaveToCsv();
    }

    /// <summary>
    /// 根据 LocalizedString 获取文本（运行时使用）。
    /// </summary>
    public string GetLocalizedString(LocalizedString localizedString)
    {
        if (localizedString == null || string.IsNullOrEmpty(localizedString.id))
        {
            return string.Empty;
        }

        var id = localizedString.id;
        if (!entries.TryGetValue(id, out var entry))
        {
            return $"Text Lost: {id}";
        }

        // 1. 优先当前语言
        if (entry.values != null && entry.values.TryGetValue(language, out string value) && !string.IsNullOrEmpty(value))
        {
            return value;
        }

        // 2. 退回第一个有内容的语言（按 SupportedLanguages 顺序）
        foreach (var lang in SupportedLanguages)
        {
            if (entry.values != null && entry.values.TryGetValue(lang, out string v) && !string.IsNullOrEmpty(v))
            {
                return v;
            }
        }

        // 3. 所有语言都为空
        return $"Text Lost: {id}";
    }

    /// <summary>
    /// 直接通过 ID 获取文本。
    /// </summary>
    public string GetLocalizedString(string id)
    {
        if (string.IsNullOrEmpty(id))
            return string.Empty;

        if (!entries.TryGetValue(id, out var entry))
            return $"Text Lost: {id}";

        if (entry.values != null && entry.values.TryGetValue(language, out string value) && !string.IsNullOrEmpty(value))
        {
            return value;
        }

        foreach (var lang in SupportedLanguages)
        {
            if (entry.values != null && entry.values.TryGetValue(lang, out string v) && !string.IsNullOrEmpty(v))
            {
                return v;
            }
        }

        return $"Text Lost: {id}";
    }

    /// <summary>
    /// 兼容旧接口：通过 int 型 key 获取文本，内部转为字符串 ID。
    /// </summary>
    public string GetLocalizedString(int key)
    {
        return GetLocalizedString(key.ToString());
    }

    #region CSV 读写

    private const string CsvResourcePath = "EasyLocalize/easy_localize";
    private void LoadFromCsv()
    {
        entries.Clear();

#if UNITY_EDITOR
        if (!File.Exists(CsvPath))
        {
            // 如果不存在，创建一个空文件
            SaveToCsv();
            return;
        }

        var rows = CSVHelper.Read(CsvPath, hasHeader: true);
#else
        var ta = Resources.Load<TextAsset>(CsvResourcePath);
        if (ta == null)
        {
            Debug.LogError($"[EasyLocalizeManager] CSV not found in Resources: {CsvResourcePath}");
            return;
        }

        var rows = CSVHelper.ReadFromText(ta.text, hasHeader: true);
    
#endif


        int maxUnnamed = -1;

        foreach (var row in rows)
        {
            // 期望格式：id, cn, en, ...
            if (row.Count < 1)
                continue;

            string id = row[0];

            var entry = new Entry
            {
                values = new Dictionary<string, string>()
            };

            for (int i = 0; i < SupportedLanguages.Length; i++)
            {
                int colIndex = 1 + i;
                if (colIndex >= row.Count)
                    break;

                string lang = SupportedLanguages[i];
                string value = row[colIndex];
                entry.values[lang] = value;
            }

            entries[id] = entry;

            if (id.StartsWith("unnamed_"))
            {
                int index;
                if (int.TryParse(id.Substring("unnamed_".Length), out index))
                {
                    if (index > maxUnnamed)
                        maxUnnamed = index;
                }
            }
        }

        nextUnnamedIndex = maxUnnamed + 1;
    }

    private void SaveToCsv()
    {
#if UNITY_EDITOR
        // 仅在编辑器且非播放模式下允许写入 CSV，避免在运行时或打包环境修改工程文件
        if (Application.isPlaying)
        {
            Debug.LogWarning("<color=yellow>[EasyLocalizeManager]</color> SaveToCsv 被调用但当前处于播放模式，已阻止写入 CSV。");
            return;
        }

        var header = new List<string> { "id" };
        header.AddRange(SupportedLanguages);

        var rows = new List<List<string>>();

        foreach (var kv in entries)
        {
            string id = kv.Key;
            var entry = kv.Value;

            var row = new List<string> { id };

            foreach (var lang in SupportedLanguages)
            {
                string value = string.Empty;
                if (entry.values != null && entry.values.TryGetValue(lang, out string v))
                {
                    value = v ?? string.Empty;
                }
                row.Add(value);
            }

            rows.Add(row);
        }

        // 确保目录存在
        var dir = Path.GetDirectoryName(CsvPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        CSVHelper.Write(CsvPath, rows, header);
#else
        // 非编辑器环境（打包后）完全禁止写文件
        Debug.LogWarning("[EasyLocalizeManager] SaveToCsv 调用已被忽略：当前不在 Unity 编辑器环境中。");
#endif
    }

    #endregion

    private class Entry
    {
        public Dictionary<string, string> values;
    }
}
