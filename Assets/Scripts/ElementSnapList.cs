using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public sealed class ElementSnapList : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    private RectTransform contentRect;

    [Header("Snap settings")]
    [SerializeField, Range(0f, 50f)]
    private float upBorderOffset;
    [SerializeField, Range(0f, 50f)]
    private float downBorderOffset;


    public readonly List<RectTransform> items = new List<RectTransform>();

    public int snapIndex                { get; private set; }
    public RectTransform snapItem       { get; private set; }


    private ScrollRect scrollRect;

    private RectTransform holder;

    private float upBorder, downBorder;

    private bool swapped;


    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 0);

        SetupCurrentItems();
        InitSnapSettings();
    }


    private void InitSnapSettings()
    {
        scrollRect = GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener((Vector2 vec) => UpdateSnap());

        holder = new GameObject("Holder", typeof(RectTransform)).GetComponent<RectTransform>();
        holder.SetParent(scrollRect.transform);
    }

    private void SetupCurrentItems()
    {
        //Unity get parent Rect Transform when we use GetComponentsInChildren, thats why we need filter content
        var childs = contentRect.GetComponentsInChildren<RectTransform>().Where(x => x.parent == contentRect);

        if (childs.Count() == 0)
        {
            Debug.LogWarning($"Content Rect has no child elements");
            return;
        }

        foreach (var child in childs)
            AddItem(child);         
    }


    private void UpdateSnap()
    {
        if (snapItem == null)
            return;

        if (snapItem.position.y < downBorder)
        {
            holder.position = new Vector3(snapItem.position.x, downBorder);
            if (!swapped)
                SwapForward();
        }
        else if (snapItem.position.y > upBorder)
        {
            holder.position = new Vector3(snapItem.position.x, upBorder);
            if (!swapped)
                SwapForward();
        }

        if (holder.position.y > downBorder && holder.position.y < upBorder && swapped)
            SwapBack();
    }

    private void SwapForward()
    {
        swapped = true;
        snapItem.SetParent(holder.parent); 
        holder.SetParent(contentRect);
        holder.SetSiblingIndex(snapIndex);

        Vector3 oldHolderPos = holder.position;
        holder.position = snapItem.position;
        snapItem.position = oldHolderPos;
    }

    private void SwapBack()
    {
        swapped = false;
        holder.SetParent(snapItem.parent);
        snapItem.SetParent(contentRect);
        snapItem.SetSiblingIndex(snapIndex);
        snapItem.position = holder.position;
    }

    private void CalculateBorders()
    {
        var scrollRectTransform = scrollRect.GetComponent<RectTransform>();

        upBorder = scrollRectTransform.position.y + scrollRectTransform.sizeDelta.y / 2 - holder.sizeDelta.y / 2 - upBorderOffset;
        downBorder = scrollRectTransform.position.y - scrollRectTransform.sizeDelta.y / 2 + holder.sizeDelta.y / 2 + downBorderOffset;
    }

    public void Snap(int index)
    {
        if(index < 0)
        {
            Debug.LogWarning($"Snap index can't be less then zero");
            return;
        }

        if (swapped)
            SwapBack();

        snapIndex = index;
        snapItem = items[index];
        holder.sizeDelta = new Vector2(snapItem.sizeDelta.x, snapItem.sizeDelta.y);

        CalculateBorders();
        UpdateSnap();
    }


    public void Snap(string index)
    {
        var parsedIndex = int.Parse(index);
        Snap(parsedIndex);
    }

    public void AddItem(RectTransform item)
    {
        AddContentRectSize(item);
        item.SetParent(contentRect);
        items.Add(item);
    }

    public void AddItem(GameObject item)
    {
        var rectTransformItem = item.GetComponent<RectTransform>();

        if(rectTransformItem == null)
        {
            Debug.LogWarning($"Item {item.name} is not Rect Transform");
            return;
        }

        AddItem(rectTransformItem);
    }

    public void RemoveItem(int index, bool destroy = true)
    {
        ClearContentRectSize(items[index]);
        items[index].SetParent(null);

        if (destroy)
            Destroy(items[index].gameObject);

        items.RemoveAt(index);
    }

    public void RemoveItem(GameObject item, bool destroy = true)
    {
        int index = items.FindIndex(x => x == item);
        
        if(index < 0)
        {
            Debug.LogWarning($"Item {item.name} is not exists at {name} list");
            return;
        }

        RemoveItem(index, destroy);
    }

    private void AddContentRectSize(RectTransform item)
    {
        contentRect.sizeDelta += new Vector2(0, item.sizeDelta.y);
    }

    private void ClearContentRectSize(RectTransform item)
    {
        contentRect.sizeDelta -= new Vector2(0, item.sizeDelta.y);
    }

}
