using UnityEngine;
using TMPro;

public class LoadingDots : MonoBehaviour
{
    public TMP_Text loadingText;
    private float timer;
    private int dotCount;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.5f) // change dots every 0.5s
        {
            dotCount = (dotCount + 1) % 4; // 0,1,2,3
            loadingText.text = "Loading" + new string('.', dotCount);
            timer = 0f;
        }
    }
}
