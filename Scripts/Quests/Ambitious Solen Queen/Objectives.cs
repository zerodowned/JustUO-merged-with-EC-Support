using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Ambitious
{
    public class KillQueensObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Kill 5 red/black solen queens.
                return ((AmbitiousQueenQuest) System).RedSolen ? 1054062 : 1054063;
            }
        }

        public override int MaxProgress
        {
            get { return 5; }
        }

        public override void RenderProgress(BaseQuestGump gump)
        {
            if (!Completed)
            {
                // Red/Black Solen Queens killed:
                gump.AddHtmlLocalized(70, 260, 270, 100, ((AmbitiousQueenQuest) System).RedSolen ? 1054064 : 1054065,
                    BaseQuestGump.Blue, false, false);
                gump.AddLabel(70, 280, 0x64, CurProgress.ToString());
                gump.AddLabel(100, 280, 0x64, "/");
                gump.AddLabel(130, 280, 0x64, MaxProgress.ToString());
            }
            else
            {
                base.RenderProgress(gump);
            }
        }

        public override bool IgnoreYoungProtection(Mobile from)
        {
            if (Completed)
                return false;

            var redSolen = ((AmbitiousQueenQuest) System).RedSolen;

            if (redSolen)
                return from is RedSolenQueen;
            return @from is BlackSolenQueen;
        }

        public override void OnKill(BaseCreature creature, Container corpse)
        {
            var redSolen = ((AmbitiousQueenQuest) System).RedSolen;

            if (redSolen)
            {
                if (creature is RedSolenQueen)
                    CurProgress++;
            }
            else
            {
                if (creature is BlackSolenQueen)
                    CurProgress++;
            }
        }

        public override void OnComplete()
        {
            System.AddObjective(new ReturnAfterKillsObjective());
        }
    }

    public class ReturnAfterKillsObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* You've completed your task of slaying solen queens. Return to
                * the ambitious queen who asked for your help.
                */
                return 1054067;
            }
        }

        public override void OnComplete()
        {
            System.AddConversation(new GatherFungiConversation());
        }
    }

    public class GatherFungiObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Gather zoogi fungus until you have 50 of them, then give them
                * to the ambitious queen you are helping.
                */
                return 1054069;
            }
        }

        public override void OnComplete()
        {
            System.AddConversation(new EndConversation());
        }
    }

    public class GetRewardObjective : QuestObjective
    {
        public GetRewardObjective(bool bagOfSending, bool powderOfTranslocation, bool gold)
        {
            BagOfSending = bagOfSending;
            PowderOfTranslocation = powderOfTranslocation;
            Gold = gold;
        }

        public GetRewardObjective()
        {
        }

        public override object Message
        {
            get
            {
                // Return to the ambitious solen queen for your reward.
                return 1054148;
            }
        }

        public bool BagOfSending { get; set; }
        public bool PowderOfTranslocation { get; set; }
        public bool Gold { get; set; }

        public override void OnComplete()
        {
            System.AddConversation(new End2Conversation());
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            var version = reader.ReadEncodedInt();

            BagOfSending = reader.ReadBool();
            PowderOfTranslocation = reader.ReadBool();
            Gold = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt(0); // version

            writer.Write(BagOfSending);
            writer.Write(PowderOfTranslocation);
            writer.Write(Gold);
        }
    }
}