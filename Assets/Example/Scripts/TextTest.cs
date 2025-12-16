using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextTest : MonoBehaviour
{
    public UnityEngine.UI.Text text;
    public LocalizedString localizedString = new LocalizedString();

    void OnEnable()
    {
        // 订阅语言切换回调
        EasyLocalizeManager.GetInstance().OnLanguageChanged += OnLanguageChanged;
    }

    void OnDisable()
    {
        if (EasyLocalizeManager.GetInstance() != null)
        {
            EasyLocalizeManager.GetInstance().OnLanguageChanged -= OnLanguageChanged;
        }
    }

    void Start()
    {
        UpdateText();
    }

    private void OnLanguageChanged(string _)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (text != null)
        {
            text.text = localizedString.str;
        }
    }
}
