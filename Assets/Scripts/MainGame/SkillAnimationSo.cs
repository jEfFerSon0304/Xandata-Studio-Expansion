using TMPro;
using UnityEngine;

[System.Serializable]
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
    public bool showTapToContinue = true;

    [Header("Shake Settings")]
    public bool shakeMainImage = true;
    public float shakeIntensity = 10f;
    public float shakeDuration = 0.4f; // how long the shake lasts

    [Header("Text Animation Style")]
    public SkillTextAnimator textAnimator = new SkillTextAnimator();

    [Header("Text Style (Per Skill Animation)")]
    public TMP_FontAsset skillFont;
    public Color skillTextColor = Color.white;

    public enum SkillTextEffectType
    {
        None,
        FadeIn,
        SlideFromLeft,
        SlideFromRight,
        SlideUp,
        SlideDown,
        Bounce,
        Wave,
        Flip,
        Impact,
        Pop
    }

    [Header("Custom Text Behavior")]
    public SkillTextEffectType textEffect = SkillTextEffectType.FadeIn;
    public AnimationCurve customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float textMoveDistance = 200f;
    public float textScaleFactor = 1.2f;



    [Header("Timing")]
    public float totalDuration = 3f; // overall animation time
}
