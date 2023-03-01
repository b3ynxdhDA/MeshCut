using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FadeController : MonoBehaviour
{
    private float fadeSpeed = 0.01f;
    private float red, green, blue, alfa;

    [SerializeField] bool isFadeOut = false;
    [SerializeField] bool isFadeIn = false;

    Image fadeImage;

    void Start()
    {
        fadeImage = GetComponent<Image>();
        red = fadeImage.color.r;
        green = fadeImage.color.g;
        blue = fadeImage.color.b;
        alfa = fadeImage.color.a;
    }

    void Update()
    {
        if (isFadeOut)
        {
            StartFadeOut();
        }
        if (isFadeIn)
        {
            StartFadeIn();
        }
    }

    public void StartFadeIn()
    {
        alfa -= fadeSpeed;
        SetAlpha();
        if(alfa <= 0)
        {
            isFadeIn = false;
            fadeImage.enabled = false;
        }
    }

    public void StartFadeOut()
    {
        fadeImage.enabled = true;
        alfa += fadeSpeed;
        SetAlpha();
        if (alfa >= 1)
        {
            isFadeOut = false;
        }
    }

    void SetAlpha()
    {
        fadeImage.color = new Color(red, green, blue, alfa);
    }
}
