using UnityEngine;

[CreateAssetMenu(fileName = "SkillAnimation", menuName = "Game/Skill Animation Data")]
public class SkillAnimationSO : ScriptableObject
{
    [Header("Image Layers")]
    public Sprite backgroundImage;  // first fades in
    public Sprite mainImage;        // main skill art that appears after
    public bool useFadeTransition = true;
    public float fadeDuration = 0.5f;
    public float mainImageDelay = 0.2f; // delay before showing main image

    [Header("Text Animation")]
    public bool showSkillText = true;
    public bool textSlideIn = true;
    public float textFadeInDelay = 0.3f;
    public float textFadeInDuration = 0.5f;
    public float textStayDuration = 1.5f;

    [Header("Sound FX")]
    public AudioClip skillSFX;

    [Header("Color & Effects")]
    public Color backgroundTint = Color.black;
    public float shakeIntensity = 10f;
    public bool showTapToContinue = true;

    [Header("Timing")]
    public float totalDuration = 3f; // overall animation time
}
