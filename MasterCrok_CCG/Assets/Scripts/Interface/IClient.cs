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
        void DisplayNewCard(Card data, int playerKey, bool visibleForPlayer, bool isABlindDraw, DrawTarget target);
        #endregion

        #region Player Specific Instructions
        void ChooseStatButton(ActiveStat stat);
        void ExitBattleButton();
        void DisplayCardDetaislWindow(Card data, int displayedCardId, bool skillDecision, int position);
        void HideCardDetailsWindow();
        void ChangeStatText(string newType);
        bool DisplayMessage(string msg);
        void HideMessage();
        void DisplayStatBox();
        void DisplayListOfCards(List<Card> cards, CardSelectionAction action);
        void CloseCardList();
        #endregion

        #region Report
        void ReportSkillDecision(SkillState state, int key, int cardFieldPosition, int cardTypeID);
        void ReportSummon(int handIndex, int position);
        void ReportPlayerExit();
        void ReportCardSelection(int cardID);
        void ReportSelectionCancel();
        void ReportDisplayRequest(CardListType listType, int positionId);
        #endregion

        #region Getter
        bool GetCardDetailWindowStatus();
        bool GetFixedDetailsStatus();
        bool GetCardListStatus();
        #endregion

        #region Setter
        void SetDetailsStatus(bool newStatus);
        void SetFixedDetails(bool newStatus);
        void SetReportAgent(SinglePlayer_Report rep);
        #endregion

    }
}
