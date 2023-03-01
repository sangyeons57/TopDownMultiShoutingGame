using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Playerselecter : MonoBehaviour
{
    public string selectedColor { get; set; }
    private string selectedColor_before;

    private void Start()
    {
        selectedColor = "";
        selectedColor_before = "";
    }

    private void Update()
    {
        if (selectedColor_before != "") transform.Find(selectedColor_before).GetComponent<Image>().color = new Color(197 / 255f, 197 / 255f, 197 / 255f);
        if (selectedColor != "")
        {
            transform.Find(selectedColor).GetComponent<Image>()
                .color = new Color(30 / 255f, 30 / 255f, 30 / 255f);
            selectedColor_before = selectedColor;
        }
    }

    public void select(string color)
    {
        selectedColor= color;
    }
}
