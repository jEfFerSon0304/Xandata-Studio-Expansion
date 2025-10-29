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

    // Static array to remember all clouds' positions
    static Vector2[] cloudPositions;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        int totalClouds = transform.parent.childCount;
        int index = transform.GetSiblingIndex();

        // Initialize positions array on first scene
        if (cloudPositions == null)
        {
            cloudPositions = new Vector2[totalClouds];
            for (int i = 0; i < totalClouds; i++)
            {
                cloudPositions[i] = transform.parent.GetChild(i).GetComponent<RectTransform>().anchoredPosition;
            }
        }

        // Restore position for this cloud
        rect.anchoredPosition = cloudPositions[index];

        // Persist across scenes
        if (transform.parent.parent == null) // ensure root
            DontDestroyOnLoad(transform.parent.gameObject);
    }

    void Update()
    {
        // Move cloud horizontally
        rect.anchoredPosition += Vector2.right * speed * Time.deltaTime;

        // Loop when past endX
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

        // Save current position to static array
        int index = rect.GetSiblingIndex();
        cloudPositions[index] = rect.anchoredPosition;
    }
}
