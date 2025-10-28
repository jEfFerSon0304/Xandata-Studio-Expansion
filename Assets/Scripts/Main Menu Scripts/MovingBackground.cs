using UnityEngine;

public class MovingBackground : MonoBehaviour
{
    public float speed = 0.2f;
    public float distanceX = 10f;
    public float distanceY = 5f;

    RectTransform rect;
    Vector2 startPos;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * speed) * distanceX;
        float y = Mathf.Cos(Time.time * speed * 0.5f) * distanceY;
        rect.anchoredPosition = startPos + new Vector2(x, y);
    }
}
