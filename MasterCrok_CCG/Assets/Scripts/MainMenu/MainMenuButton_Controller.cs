using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuButton_Controller : MonoBehaviour
{

	public TMP_Text titleText;
	public TMP_Text descriptionText;
	public Image art;

    public void SetupButton(MainMenuButtonData data)
    {
    	titleText.text = data.title;
    	descriptionText.text = data.description;
    	art.sprite = data.art;
    }
}
