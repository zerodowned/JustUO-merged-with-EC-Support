using System;
using Server.Mobiles;

namespace Server.Engines.Quests.Ninja
{
    public class EminosUndertakingQuest : QuestSystem
    {
        private static readonly Type[] m_TypeReferenceTable =
        {
            typeof (AcceptConversation),
            typeof (FindZoelConversation),
            typeof (RadarConversation),
            typeof (EnterCaveConversation),
            typeof (SneakPastGuardiansConversation),
            typeof (NeedToHideConversation),
            typeof (UseTeleporterConversation),
            typeof (GiveZoelNoteConversation),
            typeof (LostNoteConversation),
            typeof (GainInnInformationConversation),
            typeof (ReturnFromInnConversation),
            typeof (SearchForSwordConversation),
            typeof (HallwayWalkConversation),
            typeof (ReturnSwordConversation),
            typeof (SlayHenchmenConversation),
            typeof (ContinueSlayHenchmenConversation),
            typeof (GiveEminoSwordConversation),
            typeof (LostSwordConversation),
            typeof (EarnGiftsConversation),
            typeof (EarnLessGiftsConversation),
            typeof (FindEminoBeginObjective),
            typeof (FindZoelObjective),
            typeof (EnterCaveObjective),
            typeof (SneakPastGuardiansObjective),
            typeof (UseTeleporterObjective),
            typeof (GiveZoelNoteObjective),
            typeof (GainInnInformationObjective),
            typeof (ReturnFromInnObjective),
            typeof (SearchForSwordObjective),
            typeof (HallwayWalkObjective),
            typeof (ReturnSwordObjective),
            typeof (SlayHenchmenObjective),
            typeof (GiveEminoSwordObjective)
        };

        private bool m_SentRadarConversion;

        public EminosUndertakingQuest(PlayerMobile from)
            : base(from)
        {
        }

        // Serialization
        public EminosUndertakingQuest()
        {
        }

        public override Type[] TypeReferenceTable
        {
            get { return m_TypeReferenceTable; }
        }

        public override object Name
        {
            get
            {
                // Emino's Undertaking
                return 1063173;
            }
        }

        public override object OfferMessage
        {
            get
            {
                // Your value as a Ninja must be proven. Find Daimyo Emino and accept the test he offers.
                return 1063174;
            }
        }

        public override TimeSpan RestartDelay
        {
            get { return TimeSpan.MaxValue; }
        }

        public override bool IsTutorial
        {
            get { return true; }
        }

        public override int Picture
        {
            get { return 0x15D5; }
        }

        public static bool HasLostNoteForZoel(Mobile from)
        {
            var pm = from as PlayerMobile;

            if (pm == null)
                return false;

            var qs = pm.Quest;

            if (qs is EminosUndertakingQuest)
            {
                if (qs.IsObjectiveInProgress(typeof (GiveZoelNoteObjective)))
                {
                    var pack = from.Backpack;

                    return (pack == null || pack.FindItemByType(typeof (NoteForZoel)) == null);
                }
            }

            return false;
        }

        public static bool HasLostEminosKatana(Mobile from)
        {
            var pm = from as PlayerMobile;

            if (pm == null)
                return false;

            var qs = pm.Quest;

            if (qs is EminosUndertakingQuest)
            {
                if (qs.IsObjectiveInProgress(typeof (GiveEminoSwordObjective)))
                {
                    var pack = from.Backpack;

                    return (pack == null || pack.FindItemByType(typeof (EminosKatana)) == null);
                }
            }

            return false;
        }

        public override void Accept()
        {
            base.Accept();

            AddConversation(new AcceptConversation());
        }

        public override void Slice()
        {
            if (!m_SentRadarConversion &&
                (From.Map != Map.Malas || From.X < 407 || From.X > 431 || From.Y < 801 || From.Y > 830))
            {
                m_SentRadarConversion = true;
                AddConversation(new RadarConversation());
            }

            base.Slice();
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            var version = reader.ReadEncodedInt();

            m_SentRadarConversion = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt(0); // version

            writer.Write(m_SentRadarConversion);
        }
    }
}