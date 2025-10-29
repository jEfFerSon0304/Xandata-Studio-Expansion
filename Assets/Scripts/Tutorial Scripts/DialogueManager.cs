using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [Header("Scene References")]
    public FadeScript screenFade;
    public StarlaPopup starlaAnimator;
    public GameObject dialogueBox;
    public CanvasGroup dialogueGroup;
    public TextMeshProUGUI dialogueText;
    public AudioClip[] voiceLines;

    [Header("Settings")]
    public float dialogueFadeDuration = 0.5f;
    public float typeSpeed = 0.04f;

    [Header("UI References")]
    public EnergyIconGlow energyIconController; // assign your Energy icon here

    private AudioSource voiceSource;
    private int index = 0;
    private bool isTyping = false;
    private bool dialogueFullyShown = false;

    // Music duck settings
    public float musicDuckVolume = 0.3f;
    public float musicRestoreVolume = 1f;

    [Header("Line 4-5 UI References")]
    public GameObject tidalCard;
    public GameObject dropZone;
    public CardRevealController cardRevealController;

    private bool cardDropped = false;

    // Prevent multiple swoops
    private bool tidalCardShown = false;
    private bool dropZoneShown = false;
    private bool energyIconShown = false;

    [Header("Card Animation Settings")]
    public float swoopHeight = 200f;
    public float swoopDuration = 0.6f;

    private string[] lines = {
        "Hey there, Explorer! You must be new around here, huh? Starla is your guide to the islands! Ready to see how the Wild Clash works?",
        "Alright! Before you dive into battle, Starla has to tell you about something super important—Energy!",
        "Every ability you use costs Energy. See those glowing orbs? You start with one and you'll recharge another after every single round!",
        "Let's try it out! You've got three Energy right now, so let's use it to activate this card — Tidal Reversal!",
        "Go ahead! Drag that card right into the drop zone! Come on, come on, let's see it in action!",
        "Whoa, nice one! That's how you use your abilities! See? Every move you make can change the game, Starla told you so!",
        "Some abilities power you up, others drain your rivals dry, and some... do both. So, Explorer, you have to pick your moments carefully!",
        "Now here's the goal: you have to outsmart your opponents and be the first one to collect three Stars!",
        "First to three wins the crown! No ties, and no do-overs! So you better make every single turn count!",
        "That's all there is to teach you! Go out there and make a name for yourself—maybe you'll be the next champion of Wild Clash! Starla will see you around the island!"
    };

    private Coroutine typingCoroutine;
    private Coroutine swoopCoroutineCard;
    private Coroutine swoopCoroutineDropZone;

    private void Start()
    {
        voiceSource = SoundManager.Instance.sfxSource;
        dialogueBox.SetActive(false);
        dialogueText.text = "";
        tidalCard.SetActive(false);
        dropZone.SetActive(false);

        // Start hidden, scale 0
        energyIconController.energyIcon.transform.localScale = Vector3.zero;
        Color c = energyIconController.energyIcon.color;
        c.a = 0f;
        energyIconController.energyIcon.color = c;
        energyIconController.energyIcon.gameObject.SetActive(false);

        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        yield return screenFade.FadeIn();
        yield return starlaAnimator.PopUp();

        dialogueBox.SetActive(true);
        yield return FadeInDialogueBox();

        typingCoroutine = StartCoroutine(TypeLine(lines[index]));
    }

    private IEnumerator FadeInDialogueBox()
    {
        dialogueGroup.alpha = 0f;
        float t = 0f;
        while (t < dialogueFadeDuration)
        {
            t += Time.deltaTime;
            dialogueGroup.alpha = Mathf.Lerp(0f, 1f, t / dialogueFadeDuration);
            yield return null;
        }
        dialogueGroup.alpha = 1f;
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueFullyShown = false;
        dialogueText.text = "";

        SoundManager.Instance.SetMusicVolume(musicDuckVolume);
        starlaAnimator.StartTalking();

        if (index < voiceLines.Length && voiceLines[index] != null)
        {
            voiceSource.Stop();
            voiceSource.clip = voiceLines[index];
            voiceSource.loop = false;
            voiceSource.Play();
        }

        // --- Line 2 Energy swoop at start ---
        if (index == 1 && !energyIconShown)
        {
            energyIconShown = true;
            energyIconController.energyIcon.gameObject.SetActive(true);
            energyIconController.ShowIcon(); // swoop + glow
        }

        // Trigger card/drop zone if Line 4 or 5
        if (index == 3 && !tidalCardShown)
        {
            tidalCardShown = true;
            swoopCoroutineCard = StartCoroutine(SwoopIn(tidalCard));
        }
        if (index == 4 && !dropZoneShown)
        {
            dropZoneShown = true;
            cardDropped = false;
            swoopCoroutineDropZone = StartCoroutine(SwoopIn(dropZone));
        }

        // Typewriter effect
        for (int i = 0; i < line.Length; i++)
        {
            dialogueText.text += line[i];
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        dialogueFullyShown = true;

        if (voiceSource.isPlaying)
            yield return new WaitUntil(() => !voiceSource.isPlaying);

        starlaAnimator.StopTalking();
        SoundManager.Instance.SetMusicVolume(musicRestoreVolume);
    }

    public void OnDialogueBoxClick()
    {
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogueText.text = lines[index];
            isTyping = false;
            dialogueFullyShown = true;

            voiceSource.Stop();
            starlaAnimator.StopTalking();
            SoundManager.Instance.SetMusicVolume(musicRestoreVolume);

            // Make sure swoops appear if skipped
            if (index == 1 && !energyIconShown)
            {
                energyIconShown = true;
                energyIconController.energyIcon.gameObject.SetActive(true);
                energyIconController.ShowIcon();
            }
            if (index == 3 && !tidalCardShown)
            {
                tidalCardShown = true;
                swoopCoroutineCard = StartCoroutine(SwoopIn(tidalCard));
            }
            if (index == 4 && !dropZoneShown)
            {
                dropZoneShown = true;
                cardDropped = false;
                swoopCoroutineDropZone = StartCoroutine(SwoopIn(dropZone));
            }

            if (index == lines.Length - 1)
            {
                StartCoroutine(FadeAndLoadMainMenu());
            }
            return;
        }

        // Fade out Energy icon when advancing to Line 3
        if (index == 1 && energyIconShown)
        {
            StartCoroutine(FadeOutCanvasGroup(energyIconController.energyIcon.GetComponent<CanvasGroup>(), 0.5f));
            energyIconShown = false;
        }

        if (dialogueFullyShown)
        {
            if (index == 4 && !cardDropped) return;

            if (index < lines.Length - 1)
            {
                index++;
                typingCoroutine = StartCoroutine(TypeLine(lines[index]));
            }
            else
            {
                StartCoroutine(FadeAndLoadMainMenu());
            }
        }
    }

    // --- Swoop animation ---
    private IEnumerator SwoopIn(GameObject obj)
    {
        obj.SetActive(true);

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        RectTransform rt = obj.GetComponent<RectTransform>();
        Vector3 targetPos = rt.localPosition;
        Vector3 startPos = targetPos + new Vector3(0, swoopHeight, 0);
        rt.localPosition = startPos;

        float elapsed = 0f;
        while (elapsed < swoopDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swoopDuration;
            float height = 4 * swoopHeight * t * (1 - t); // arc motion
            rt.localPosition = Vector3.Lerp(startPos, targetPos, t) + new Vector3(0, height, 0);
            yield return null;
        }
        rt.localPosition = targetPos;
    }

    private IEnumerator FadeOutCanvasGroup(CanvasGroup cg, float duration)
    {
        if (cg == null) yield break;
        float startAlpha = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
            yield return null;
        }
        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }

    public void OnCardDropped()
    {
        cardDropped = true;

        dropZone.SetActive(false);
        tidalCard.SetActive(false);

        if (index == 4 && cardRevealController != null)
        {
            StartCoroutine(PlayRevealThenContinue());
        }
    }

    private IEnumerator PlayRevealThenContinue()
    {
        yield return cardRevealController.RevealCoroutine("Stampede");
        index++;
        typingCoroutine = StartCoroutine(TypeLine(lines[index]));
    }

    private IEnumerator FadeAndLoadMainMenu()
    {
        dialogueBox.SetActive(false);
        starlaAnimator.gameObject.SetActive(false);

        yield return screenFade.FadeOut();
        SceneManager.LoadScene("MainMenu");
    }
}
