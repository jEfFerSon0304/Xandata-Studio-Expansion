using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillCardUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text descText;
    public TMP_Text costText;

    public Button flipButton;
    public CharacterDataSO.SkillData skill;
    public MainGameManager manager;

    private bool showingFront = true;

    public void Setup(CharacterDataSO.SkillData s)
    {
        skill = s;
        icon.sprite = s.skillIcon;
        nameText.text = s.skillName;
        descText.text = s.description;
        costText.text = s.energyCost.ToString();

        flipButton.onClick.RemoveAllListeners();
        flipButton.onClick.AddListener(Flip);

        // Ultimate visual highlight (optional now)
        if (s.isUltimate)
        {
            costText.color = Color.red;
            nameText.fontStyle = FontStyles.Bold;
        }
    }

    void Flip()
    {
        showingFront = !showingFront;
        icon.gameObject.SetActive(showingFront);
        descText.gameObject.SetActive(!showingFront);
    }

    public void OnDropzoneUse()
    {
        manager.TryUseSkill(skill);
    }
}
