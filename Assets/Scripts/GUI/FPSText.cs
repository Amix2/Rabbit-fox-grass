using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSText : MonoBehaviour
{
    TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = $"{Math.Round(Time.smoothDeltaTime*1000f, 1)}ms";
    }
}
