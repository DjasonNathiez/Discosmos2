using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    public Image image;
    public float fadeSpeed = 0.1f;

    public void Fade()
    {
        StartCoroutine(FadeCoroutine());
    }

    IEnumerator FadeCoroutine()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a + fadeSpeed);
        yield return new WaitForEndOfFrame();

        if (image.color.a < 1)
        {
            StartCoroutine(FadeCoroutine());
        }
        else
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        }
    }
}
