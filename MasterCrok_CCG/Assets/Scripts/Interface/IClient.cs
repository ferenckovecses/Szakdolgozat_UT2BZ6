using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.SinglePlayer;

namespace Assets.Scripts.Interface
{
    public interface IClient
    {
        #region Instruction
        void Exit();
        void ContinueBattle(GameObject exitPanel);
        void CreatePlayerFields(int playerNumber, List<int> keys, int playerDeckSize);
        void DisplayNewCard(Card data, int playerKey, bool visibleForPlayer, bool isABlindDraw);
        void SetDragStatus(int playerKey, bool newStatus);
        void RevealCards(int playerKey);
        void ResetCardSkill(int playerKey, int cardPosition);
        void NewSkillCycle(int playerKey);
        void SetSkillStatus(int playerKey, bool newStatus);
        void PutCardsAway(int playerKey, bool isWinner);

        #endregion

        #region Player Specific Instructions
 void ChooseStatButton(ActiveStat stat);
 void ExitBattleButton();
 void RefreshDeckSize(int key, int newDeckSize);
 void DisplayCardDetaislWindow(Card data, int displayedCardId, bool skillDecision, int position);
 void HideCardDetailsWindow();
 void ChangeStatText(string newType);
 bool DisplayMessage(string msg);
 void HideMessage();
 void DisplayStatBox();
        #endregion

        #region Bot Specific Instructions
        void SummonCard(int playerKey, int cardHandIndex);
        #endregion

        #region Report
        void ReportSkillDecision(SkillState state, int key, int cardFieldPosition, int cardTypeID);
        void ReportSummon(int handIndex, int position);
        void ReportPlayerExit();
        #endregion

        #region Getter
        bool GetCardDetailWindowStatus();
        bool GetFixedDetailsStatus();
        #endregion

        #region Setter
        void SetDetailsStatus(bool newStatus);
        void SetFixedDetails(bool newStatus);
        void SetReportAgent(SinglePlayer_Report rep);
        #endregion

    }
}
