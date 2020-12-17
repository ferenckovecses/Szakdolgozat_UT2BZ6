using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class GameState
	{
		protected Module_Controller modules;
		protected GameState_Controller controller;

		public GameState(Module_Controller in_module, GameState_Controller in_controller)
		{
			this.modules = in_module;
			this.controller = in_controller;
		}

		public virtual void Init()
		{
			//Fázis hatásai
			//
			//
			ChangeState();
		}

		public virtual void ChangeState()
		{
			//Fázis váltások
		}
	}	
}

