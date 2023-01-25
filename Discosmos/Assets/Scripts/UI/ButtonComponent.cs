using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public MeshRenderer targetButton;
    public TextMeshPro targetText;
    public TMP_FontAsset mouseOverEnterFont;
    public float enterScale = 2;
    public TMP_FontAsset mouseOverExitFont;
    public float exitScale = 1;
    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = targetButton.transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetButton.material.SetInt("_Couleur", 0);
        targetButton.transform.DOScale(baseScale * enterScale, 0.5f);
        targetText.font = mouseOverEnterFont;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetButton.material.SetInt("_Couleur", 1);
        targetButton.transform.DOScale(baseScale * exitScale, 0.5f);
        targetText.font = mouseOverExitFont;
    }
}
