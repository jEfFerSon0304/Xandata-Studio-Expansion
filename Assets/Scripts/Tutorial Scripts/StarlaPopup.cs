using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StarlaPopup : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpHeight = 200f;
    public float jumpDuration = 1f;

    [Header("Talking Settings")]
    public Image starlaImage;
    public Sprite idleSprite;
    public Sprite talkingSprite;
    public float talkingSpeed = 0.15f;

    private Vector3 targetPosition;
    private Coroutine talkingRoutine;

    void Start()
    {
        targetPosition = transform.localPosition;
        transform.localPosition = targetPosition - new Vector3(0, jumpHeight, 0);
        starlaImage.sprite = idleSprite;
    }

    public IEnumerator PopUp()
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = targetPosition;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / jumpDuration;
            float height = 4 * jumpHeight * t * (1 - t);
            transform.localPosition = Vector3.Lerp(startPos, endPos, t) + new Vector3(0, height, 0);
            yield return null;
        }

        transform.localPosition = endPos;
    }

    public void StartTalking()
    {
        if (talkingRoutine != null) StopCoroutine(talkingRoutine);
        talkingRoutine = StartCoroutine(TalkAnimation());
    }

    public void StopTalking()
    {
        if (talkingRoutine != null)
            StopCoroutine(talkingRoutine);

        starlaImage.sprite = idleSprite;
    }

    private IEnumerator TalkAnimation()
    {
        while (true)
        {
            starlaImage.sprite = talkingSprite;
            yield return new WaitForSeconds(talkingSpeed);
            starlaImage.sprite = idleSprite;
            yield return new WaitForSeconds(talkingSpeed);
        }
    }
}
