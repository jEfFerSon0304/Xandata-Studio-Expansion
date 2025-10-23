using UnityEngine;
using TMPro;

public class RandomLoadingText : MonoBehaviour
{
    public TMP_Text randomText;
    public string[] phrases = { "Did you know? Cats can't taste sweetness.", "Tip: Stay hydrated!", "Loading your adventure...", "Fun fact: Bananas are berries!" };
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 5f) // change text every 2 seconds
        {
            randomText.text = phrases[Random.Range(0, phrases.Length)];
            timer = 0f;
        }
    }
}
