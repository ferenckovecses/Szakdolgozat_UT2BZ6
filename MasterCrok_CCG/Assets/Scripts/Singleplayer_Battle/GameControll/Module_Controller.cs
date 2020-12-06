using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientControll;

namespace GameControll
{
	//Middleman pattern
	public class Module_Controller
	{
        public static Module_Controller instance = null;

	    //Vezérlő modulok
	    private GameState_Controller gameModule;
	    private Data_Controller dataModule;
	    private Input_Controller inputModule;
	    private Client_Controller clientModule;
	    private Skill_Controller skillModule;
	    private AI_Controller AI_module;

        public static Module_Controller CreateModuleController(GameState_Controller in_gameModule, CardFactory factory, Client client)
        {
            if(instance == null)
            {
                instance = new Module_Controller(in_gameModule,factory,client);
            }

            return instance;
        }

	    private Module_Controller(GameState_Controller in_gameModule, CardFactory factory, Client client)
	    {
	    	this.gameModule = in_gameModule;
	    	this.dataModule = new Data_Controller(factory);
	    	this.inputModule = new Input_Controller(this);
	    	this.clientModule = new Client_Controller(client, this);
	    	this.skillModule = new Skill_Controller(this);
	    	this.AI_module = new AI_Controller(this);
	    }

	    public AI_Controller GetAImodule()
        {
            return this.AI_module;
        }

        public Skill_Controller GetSkillModule()
        {
            return this.skillModule;
        }

        public Data_Controller GetDataModule()
        {
            return this.dataModule;
        }

        public Client_Controller GetClientModule()
        {
            return this.clientModule;
        }

        public Input_Controller GetInputModule()
        {
            return this.inputModule;
        }

        public GameState_Controller GetGameModule()
        {
        	return this.gameModule;
        }
	}
}
