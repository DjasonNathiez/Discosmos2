using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonComponent : MonoBehaviour, IPointerClickHandler
{
    public float pumpScale;

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOPunchScale(transform.localScale * pumpScale, 0.2f);
    }
}
