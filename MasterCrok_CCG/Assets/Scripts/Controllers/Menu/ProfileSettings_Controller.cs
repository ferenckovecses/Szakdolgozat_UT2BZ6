using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientSide
{
	public class ProfileSettings_Controller : MonoBehaviour
	{

		public void ChangeName()
		{
			Profile_Controller.settingsState = ProfileSettings.ChangeName;
			Profile_Controller.needsToBeUpdated = true;
			Destroy(this.gameObject);
		}

		public void ResetProfile()
		{
			Profile_Controller.settingsState = ProfileSettings.NewProfile;
			Profile_Controller.needsToBeUpdated = true;
			Destroy(this.gameObject);
		}

		public void Back()
		{
			Destroy(this.gameObject);
		}
	}
}

