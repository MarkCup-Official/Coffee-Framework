using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchLanguage : MonoBehaviour
{
    public void SwitchTo(string language)
    {
        EasyLocalizeManager.GetInstance().SetLanguage(language);
    }
}
