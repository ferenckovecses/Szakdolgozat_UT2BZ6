using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RuleMenu_Controller : MonoBehaviour
{
	public TMP_Text ruleField;
	public TMP_Text amountField;
	
	[TextArea(10,10)]
	public List<string> textData = new List<string>();

	private int index = 0;

	void Awake()
	{
		RefreshText();
	}

	private void RefreshText()
	{
		ruleField.text = textData[index];
		amountField.text = $"{index+1}/{textData.Count}";
	}

	public void TurnPage(int amount)
	{
		if( (index + amount) >= 0 
			&& (index + amount) <= (textData.Count - 1))
		{
			index += amount;
			RefreshText();
		}
	}

	public void Return()
	{
		SceneManager.LoadScene("Main_Menu");
	}

}
