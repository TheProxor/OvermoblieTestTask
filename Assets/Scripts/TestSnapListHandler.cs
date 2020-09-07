using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSnapListHandler : MonoBehaviour
{
    [SerializeField]
    private ElementSnapList elementSnapList;
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private InputField inputField;
    [SerializeField]
    private Color normalColor = Color.white;
    [SerializeField]
    private Color snapColor = Color.cyan;


    private int index = 0;


    private void Start()
    {
        inputField.text = index.ToString();
        slider.value = index;

        slider.maxValue = elementSnapList.items.Count;
    }

    public void OnUpdateSlider()
    {
        inputField.text = slider.value.ToString();
        index = (int)slider.value;
    }

    public void OnUpdateInputField()
    {
        slider.value = index = int.Parse(inputField.text);
    }

    public void OnSnap()
    {
        elementSnapList.Snap(index);
        foreach (var item in elementSnapList.items)
            item.GetComponent<Graphic>().color = normalColor;
        elementSnapList.snapItem.GetComponent<Graphic>().color = snapColor;
    }


}
