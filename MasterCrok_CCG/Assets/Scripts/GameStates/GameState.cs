using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class GameState
	{
		protected Module_Controller modules;
		protected GameState_Controller controller;
		protected Interactions interactions;

		public GameState(Module_Controller in_module, GameState_Controller in_controller, Interactions in_interactions)
		{
			this.modules = in_module;
			this.controller = in_controller;
			this.interactions = in_interactions;
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

