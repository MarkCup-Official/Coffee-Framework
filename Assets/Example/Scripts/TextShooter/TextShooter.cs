using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextShooter : MonoBehaviour
{
    public LocalizedString shootText;

    int index = -1;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            index++;
            if (index >= shootText.str.Length)
            {
                index = 0;
            }
            Shoot(shootText.str[index].ToString());
            Debug.Log(shootText.str[index]);
        }
    }

    public GameObject textPrefab;

    public void Shoot(string text)
    {
        var textObject = Instantiate(textPrefab);
        textObject.transform.position = transform.position+Vector3.up;
        textObject.GetComponent<TextMesh>().text = text;
    }
}
