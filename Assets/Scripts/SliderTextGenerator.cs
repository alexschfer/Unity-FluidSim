using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderTextGenerator : MonoBehaviour
{
    public Slider slider;
    public TMP_Text TMP_Text;

    public string text = "none";

    void Start() {
        slider.onValueChanged.AddListener(ValueUpdate);
    }

    void ValueUpdate(float value) {

        if (value < 10)
            TMP_Text.text = text + "  " + value;
        else
            TMP_Text.text = text + value;
    }
}
