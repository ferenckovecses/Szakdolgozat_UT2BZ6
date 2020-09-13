using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.SinglePlayer
{
    public class SinglePlayer_Report
    {
        SinglePlayer_Controller controller;

        public SinglePlayer_Report(SinglePlayer_Controller cont)
        {
            this.controller = cont;
        }

        public void ReportStatChange(ActiveStat newStat)
        {
            controller.HandleStatChange(newStat);
        }

        public void ReportSkillStatusChange(SkillState state, int playerKey, bool haveFinished, int cardPosition)
        {
            controller.HandleSkillStatusChange(state, playerKey, haveFinished, cardPosition);
        }

        public void ReportSummon(int indexInHand, int playerKey)
        {
            controller.HandleSummon(indexInHand, playerKey);
        }

        public void ReportExit()
        {
            controller.HandleExit();
        }
    }
}
