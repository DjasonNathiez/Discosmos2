using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOut : MonoBehaviour
{
    public Image image;
    public float fadeSpeed = 0.1f;

    public void Start()
    {
        StartCoroutine(FadeCoroutine());
    }

    IEnumerator FadeCoroutine()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - fadeSpeed);
        yield return new WaitForEndOfFrame();

        if (image.color.a > 0)
        {
            StartCoroutine(FadeCoroutine());
        }
        else
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }
    }
}
