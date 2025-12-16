using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// LocalizedString Inspector Drawer:
/// - 折叠：灰色不可编辑预览框（默认语言）+ Reload 按钮
/// - 展开：ID + 所有语言可编辑
/// - Reload：调用 EasyLocalizeManager.Init() 重新读取本地化信息，并回填当前字段
/// </summary>
[CustomPropertyDrawer(typeof(LocalizedString))]
public class LocalizedStringDrawer : PropertyDrawer
{
    // 右侧按钮区宽度（可按需调整）
    private const float ReloadBtnWidth = 56f; // "Reload"

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var languages = EasyLocalizeManager.GetSupportedLanguagesEditor();
        int langCount = languages?.Length ?? 0;

        int lineCount = 1; // 折叠预览行（含 Reload）
        if (property.isExpanded)
            lineCount += 1 + langCount; // ID + all languages

        return EditorGUIUtility.singleLineHeight * lineCount
             + EditorGUIUtility.standardVerticalSpacing * (lineCount - 1);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var idProp = property.FindPropertyRelative("id");
        var stringsProp = property.FindPropertyRelative("strings");

        EnsureIdAndRegisterIfNeeded(idProp, stringsProp);

        var languages = EasyLocalizeManager.GetSupportedLanguagesEditor();
        if (languages == null || languages.Length == 0)
        {
            EditorGUI.HelpBox(position, "No supported languages.", MessageType.Warning);
            EditorGUI.EndProperty();
            return;
        }

        // 先拉一次，保证预览最新
        PullFromManagerIfExists(idProp, stringsProp);

        string defaultLang = languages[0];

        var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // 折叠预览行（灰色预览框 + Reload）
        bool reloadClicked = DrawCollapsedPreviewFieldWithReload(rect, property, label, stringsProp, defaultLang);

        // 点击整行可展开/折叠（点到 Reload 按钮不会触发，因为上面已处理 Use）
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            property.isExpanded = !property.isExpanded;
            Event.current.Use();
        }

        // Reload：重新读本地化并回填
        if (reloadClicked)
        {
            EasyLocalizeManager.Init();
            PullFromManagerIfExists(idProp, stringsProp);

            // 让 Inspector 立刻刷新显示
            GUI.changed = true;
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // 展开内容：ID + 全语言可编辑
        if (property.isExpanded)
        {
            DrawIdField(rect, idProp, stringsProp);
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            for (int i = 0; i < languages.Length; i++)
            {
                DrawLanguageEditableField(rect, idProp, stringsProp, languages[i]);
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        EditorGUI.EndProperty();
    }

    /// <summary>
    /// 折叠时显示：Foldout + Label + 灰色不可编辑预览框 + Reload 按钮
    /// 返回是否点击了 Reload
    /// </summary>
    private bool DrawCollapsedPreviewFieldWithReload(
        Rect rect,
        SerializedProperty property,
        GUIContent label,
        SerializedProperty stringsProp,
        string defaultLang)
    {
        // Foldout 三角
        property.isExpanded = EditorGUI.Foldout(
            new Rect(rect.x, rect.y, 14f, rect.height),
            property.isExpanded,
            GUIContent.none,
            true
        );

        // label 区域
        float labelWidth = EditorGUIUtility.labelWidth;
        Rect labelRect = new Rect(rect.x + 14f, rect.y, labelWidth - 14f, rect.height);
        EditorGUI.LabelField(labelRect, label);

        // 右侧：预览框 + Reload
        Rect rightRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height);
        Rect btnRect = new Rect(rightRect.xMax - ReloadBtnWidth, rightRect.y, ReloadBtnWidth, rightRect.height);
        Rect fieldRect = new Rect(rightRect.x, rightRect.y, rightRect.width - ReloadBtnWidth - 4f, rightRect.height);

        string preview = GetLanguageValue(stringsProp, defaultLang) ?? string.Empty;

        // 灰色、不可编辑预览框
        bool oldEnabled = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.TextField(fieldRect, preview);
        GUI.enabled = oldEnabled;

        // Reload 按钮（点击后不要让“整行点击”触发展开/折叠）
        bool clicked = GUI.Button(btnRect, "Reload");
        if (clicked)
        {
            // 吃掉事件，避免下面整行点击逻辑触发
            if (Event.current != null) Event.current.Use();
        }

        return clicked;
    }

    private void EnsureIdAndRegisterIfNeeded(SerializedProperty idProp, SerializedProperty stringsProp)
    {
        string oldId = idProp.stringValue;
        if (!string.IsNullOrEmpty(oldId)) return;

        var manager = EasyLocalizeManager.GetInstance();
        oldId = manager.GenerateNewId();
        idProp.stringValue = oldId;

        manager.RegisterEntry(oldId, GetItemsFromProperty(stringsProp));
    }

    private void PullFromManagerIfExists(SerializedProperty idProp, SerializedProperty stringsProp)
    {
        var manager = EasyLocalizeManager.GetInstance();
        string id = idProp.stringValue;
        if (string.IsNullOrEmpty(id)) return;

        if (manager.HasEntry(id))
        {
            var items = manager.GetItems(id);
            if (items != null)
                SetItemsToProperty(stringsProp, items);
        }
    }

    private void DrawIdField(Rect rect, SerializedProperty idProp, SerializedProperty stringsProp)
    {
        string oldId = idProp.stringValue;

        EditorGUI.BeginChangeCheck();
        string newId = EditorGUI.DelayedTextField(rect, "ID", oldId);
        if (!EditorGUI.EndChangeCheck()) return;

        var manager = EasyLocalizeManager.GetInstance();

        if (!string.IsNullOrEmpty(oldId) && oldId != newId)
            manager.RemoveEntry(oldId);

        if (manager.HasEntry(newId))
        {
            var items = manager.GetItems(newId) ?? new List<LocalizedString.LocalizedStringItem>();
            SetItemsToProperty(stringsProp, items);
        }
        else
        {
            manager.RegisterEntry(newId, GetItemsFromProperty(stringsProp));
        }

        idProp.stringValue = newId;
    }

    private void DrawLanguageEditableField(Rect rect, SerializedProperty idProp, SerializedProperty stringsProp, string lang)
    {
        var itemProp = FindOrCreateItem(stringsProp, lang);
        var valueProp = itemProp.FindPropertyRelative("value");

        EditorGUI.BeginChangeCheck();
        string newValue = EditorGUI.TextField(rect, lang, valueProp.stringValue);
        if (!EditorGUI.EndChangeCheck()) return;

        valueProp.stringValue = newValue;
        EasyLocalizeManager.GetInstance().RegisterEntry(idProp.stringValue, GetItemsFromProperty(stringsProp));
    }

    private SerializedProperty FindOrCreateItem(SerializedProperty listProp, string language)
    {
        for (int i = 0; i < listProp.arraySize; i++)
        {
            var element = listProp.GetArrayElementAtIndex(i);
            var langProp = element.FindPropertyRelative("language");
            if (langProp.stringValue == language)
                return element;
        }

        int newIndex = listProp.arraySize;
        listProp.InsertArrayElementAtIndex(newIndex);
        var newElement = listProp.GetArrayElementAtIndex(newIndex);
        newElement.FindPropertyRelative("language").stringValue = language;
        newElement.FindPropertyRelative("value").stringValue = string.Empty;
        return newElement;
    }

    private string GetLanguageValue(SerializedProperty listProp, string language)
    {
        for (int i = 0; i < listProp.arraySize; i++)
        {
            var element = listProp.GetArrayElementAtIndex(i);
            var langProp = element.FindPropertyRelative("language");
            if (langProp.stringValue == language)
                return element.FindPropertyRelative("value").stringValue;
        }
        return string.Empty;
    }

    private List<LocalizedString.LocalizedStringItem> GetItemsFromProperty(SerializedProperty listProp)
    {
        var list = new List<LocalizedString.LocalizedStringItem>();
        for (int i = 0; i < listProp.arraySize; i++)
        {
            var element = listProp.GetArrayElementAtIndex(i);
            var langProp = element.FindPropertyRelative("language");
            var valueProp = element.FindPropertyRelative("value");
            list.Add(new LocalizedString.LocalizedStringItem
            {
                language = langProp.stringValue,
                value = valueProp.stringValue
            });
        }
        return list;
    }

    private void SetItemsToProperty(SerializedProperty listProp, List<LocalizedString.LocalizedStringItem> items)
    {
        listProp.arraySize = 0;
        if (items == null) return;

        for (int i = 0; i < items.Count; i++)
        {
            int index = listProp.arraySize;
            listProp.InsertArrayElementAtIndex(index);
            var element = listProp.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("language").stringValue = items[i].language;
            element.FindPropertyRelative("value").stringValue = items[i].value;
        }
    }
}
#endif
