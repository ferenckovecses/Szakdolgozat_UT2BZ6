using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;


namespace ClientSide
{
	public class NameForm_Controller : MonoBehaviour
	{

		public Button btn;
		public TMP_InputField nameField;

		public void SendName()
		{
			if(VerifyName())
			{
				GameObject.Find("Profile_Controller").GetComponent<Profile_Controller>().SetNewName(nameField.text);
			}
		}

		private bool VerifyName()
		{
				if(nameField.text.Length > 15 || nameField.text.Length < 1)
				{
					string msg = "A név hossza nem megfelelő!\nMinimum 1 és maximum 15 karakter lehet!";
					Notification_Controller.DisplayNotification(msg);
					return false;
				}
				Regex rgx = new Regex(@"^[A-Za-z0-9áÁöÖüÜőŐúÚűŰéÉíÍ]+$");
				if(rgx.IsMatch(nameField.text))
				{
					return true;
				}

				else 
				{
					string msg = "Szabálytalan név formátum!\nKérem csak betűket és számokat használjon!";
					Notification_Controller.DisplayNotification(msg);
					return false;
				}
		}
	}
}