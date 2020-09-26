using UnityEngine;
using UnityEngine.UI;
using ClientControll;

public enum SkillState{NotDecided,Store,Use,Pass};

public class CardBehaviour : MonoBehaviour
{
	//Státusz változók
	private bool isDragged;
	private bool isAboveActiveField;
	private bool isSummoned;
    private bool isCardVisible;
    private bool isSkillDecidedInThisTurn;
    private SkillState skill;

	//Pozíciónáló elemek
	private Vector2 startingPosition;
	private GameObject activeField;
	private GameObject startParent;
	private GameObject mainCanvas;
	private GameObject playerField;

    //Adattárolók
    private Card cardData;
    private int siblingIndex;

	private void Awake()
    {
        mainCanvas = GameObject.Find("Base_Canvas");
        playerField = gameObject.transform.parent.parent.gameObject;
        this.siblingIndex = gameObject.transform.GetSiblingIndex();
        skill = SkillState.NotDecided;
        isSkillDecidedInThisTurn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragged)
        {
            if(GetParentField().GetDetailsStatus())
            {
                GetParentField().SetDetailsStatus(false);
                HideDetails();
            }

        	//Pozíció változtatása
            gameObject.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            //Minden további elem felett álljon a kártya
            gameObject.transform.SetParent(mainCanvas.transform);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
    	if(collision.gameObject.tag == "PlayerField")
    	{
	        isAboveActiveField = true;
        	activeField = collision.gameObject;
    	}
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
    	if(collision.gameObject.tag == "PlayerField")
    	{
	        isAboveActiveField = false;
	        activeField = null;
    	}
    }

    public void PressCard()
    {
        //Csak olyan kártya jeleníthető meg nagyban, ami amúgy is látható
        if(isCardVisible)
        {
            bool skillButtonsRequired = false;
            GetParentField().SetDetailsStatus(true);

            //Döntéshozó gombok megjelenítése a fix részletes nézet mellé
            if(isSummoned && GetParentField().GetSkillStatus() && !isSkillDecidedInThisTurn)
            {
                skillButtonsRequired = true;
            }

            GiveDetails(skillButtonsRequired);
        }
    }

    public void ReleaseCard()
    {
        GetParentField().SetDetailsStatus(false);
        HideDetails();
    }

    public void StartDrag()
    {
    	//Megnézzük hogy a kártya a miénk-e, illetve nem aktivált-e már, és mozgatható-e
    	if(playerField == GetFieldOfCard() && !isSummoned
            && GetParentField().GetDraggableStatus() )
    	{
	    	this.transform.rotation = Quaternion.identity;
	        startParent = gameObject.transform.parent.gameObject;
	        startingPosition = gameObject.transform.position;
	        isDragged = true;
    	}
    }

    public void EndDrag()
    {
    	if(isDragged)
    	{
	        isDragged = false;
	        GetParentField().SetDetailsStatus(false);
	        if (isAboveActiveField)
	        {
	            gameObject.transform.SetParent(activeField.transform);
	            ActivateCard(false);
	        }
	        else
	        {
	            gameObject.transform.position = startingPosition;
	            gameObject.transform.SetParent(startParent.transform);
                
                //Kézbe visszahelyezéskor eredeti pozícióját foglalja el és rendezzük a kezet
                gameObject.transform.SetSiblingIndex(siblingIndex);
                GetParentField().SortHand();
	        }
    	}
    }

    public void ActivateCard(bool blindSummon = false)
    {
        if(!blindSummon)
        {
            GetParentField().PutCardOnField(gameObject);
        }

    	isSummoned = true;
        this.siblingIndex = gameObject.transform.GetSiblingIndex();
    }

    public void SetupCard(Card data)
    {
        this.cardData = data;
    }

    private void Reveal()
    {
        gameObject.GetComponent<Image>().sprite = cardData.GetArt();
    }

    private GameObject GetFieldOfCard()
    {
    	return this.transform.parent.parent.gameObject;
    }

    private void GiveDetails(bool skillButtonsRequired)
    {
        this.siblingIndex = gameObject.transform.GetSiblingIndex();

        GetParentField().DisplayCardDetails(cardData, siblingIndex, skillButtonsRequired);
    }

    private void HideDetails()
    {
        GetParentField().HideCardDetails();
    }

    public void SetVisibility(bool status)
    {
        this.isCardVisible = status;

        if(this.isCardVisible)
        {
            Reveal();
        }
    }

    public Sprite GetArt()
    {
        return this.cardData.GetArt();
    }

    public void SetSkillState(SkillState newState)
    {
        this.skill = newState;
        isSkillDecidedInThisTurn = true;
    }

    public SkillState GetSkillState()
    {
        return this.skill;
    }

    public void TerminateCard()
    {
        if(GetParentField().GetDetailsStatus())
        {
            HideDetails();
        }
    }

    private Player_UI GetParentField()
    {
        return playerField.GetComponent<Player_UI>();
    }

    //Ha tartalékoltuk a képességet, akkor az új ciklusban megint lehet dönteni a sorsáról 
    public void NewSkillCycle()
    {
        if(skill == SkillState.Store)
        {
            isSkillDecidedInThisTurn = false;
        }
    }

    public void ResetSkill()
    {
        this.skill = SkillState.NotDecided;
        isSkillDecidedInThisTurn = false;
    }
}
