using UnityEngine;

public class MovingBackground : MonoBehaviour
{
    public float speed = 0.2f;
    public float distanceX = 10f;
    public float distanceY = 5f;

    RectTransform rect;

    // Static variables to remember position and time offset
    static Vector2 lastStartPos;
    static float lastTimeOffset;

    Vector2 startPos;
    float timeOffset;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        // Restore previous start position and time offset if available
        startPos = lastStartPos != Vector2.zero ? lastStartPos : rect.anchoredPosition;
        timeOffset = lastTimeOffset;
    }

    void Update()
    {
        // Use Time.time + offset so movement continues smoothly across scenes
        float t = Time.time + timeOffset;
        float x = Mathf.Sin(t * speed) * distanceX;
        float y = Mathf.Cos(t * speed * 0.5f) * distanceY;
        rect.anchoredPosition = startPos + new Vector2(x, y);

        // Store current start position and offset for next scene
        lastStartPos = startPos;
        lastTimeOffset = t - Time.time; // store offset relative to new scene
    }
}
