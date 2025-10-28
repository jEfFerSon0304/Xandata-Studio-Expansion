using UnityEngine;
using UnityEngine.UI;

public class MovingClouds : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10f;      // how fast clouds move
    public float startX = -2500f;  // where clouds begin (off-screen)
    public float endX = 2500f;     // where clouds reset (off other side)

    [Header("Optional Fade Settings")]
    public float fadeDistance = 500f; // how far from edge to fade in/out

    RectTransform rect;
    Image image;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    void Update()
    {
        // Move the cloud horizontally
        rect.anchoredPosition += Vector2.right * speed * Time.deltaTime;

        // Loop it when it goes too far right
        if (rect.anchoredPosition.x > endX)
            rect.anchoredPosition = new Vector2(startX, rect.anchoredPosition.y);

        // Fade in/out near edges
        float x = rect.anchoredPosition.x;
        float alpha = 1f;

        if (x < startX + fadeDistance)
            alpha = Mathf.InverseLerp(startX, startX + fadeDistance, x);
        else if (x > endX - fadeDistance)
            alpha = Mathf.InverseLerp(endX, endX - fadeDistance, x);

        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}
