using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class CardDisplay_Controller : MonoBehaviour
{
	public TMP_Text cardName;
	public TMP_Text skillName;
	public TMP_Text skillDetail;
	public TMP_Text power;
	public TMP_Text intelligence;
	public TMP_Text reflex;
	public Image art;

	public void SetupDisplay(Card data)
	{
		this.cardName.text = data.GetCardName();
		this.skillName.text = data.GetSkillName() + ":";
		this.skillDetail.text = data.GetCardSkill();
		this.power.text = "Erő: " + data.GetPower().ToString();
		this.intelligence.text = "Intelligencia: " + data.GetIntelligence().ToString();
		this.reflex.text = "Reflex: " + data.GetReflex().ToString();
		this.art.sprite = data.GetArt();
	}
}
