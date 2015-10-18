using System;
using Server.Ethics;
using Server.Items;
using Server.Misc;
using Server.Services;

namespace Server.Mobiles
{
    [CorpseName("a wisp corpse")]
    public class DarkWisp : BaseCreature
    {
        [Constructable]
        public DarkWisp()
            : base(AIType.AI_Mage, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a wisp";
            Body = 165;
            BaseSoundID = 466;

            SetStr(196, 225);
            SetDex(196, 225);
            SetInt(196, 225);

            SetHits(118, 135);

            SetDamage(17, 18);

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Energy, 50);

            SetResistance(ResistanceType.Physical, 35, 45);
            SetResistance(ResistanceType.Fire, 20, 40);
            SetResistance(ResistanceType.Cold, 10, 30);
            SetResistance(ResistanceType.Poison, 5, 10);
            SetResistance(ResistanceType.Energy, 50, 70);

            SetSkill(SkillName.EvalInt, 80.0);
            SetSkill(SkillName.Magery, 80.0);
            SetSkill(SkillName.MagicResist, 80.0);
            SetSkill(SkillName.Tactics, 80.0);
            SetSkill(SkillName.Wrestling, 80.0);

            Fame = 4000;
            Karma = -4000;

            VirtualArmor = 40;

            AddItem(new LightSource());
        }

        public DarkWisp(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType
        {
            get { return InhumanSpeech.Wisp; }
        }

        public override Ethic EthicAllegiance
        {
            get { return Ethic.Evil; }
        }

        public override TimeSpan ReacquireDelay
        {
            get { return TimeSpan.FromSeconds(1.0); }
        }

        public override OppositionGroup OppositionGroup
        {
            get { return OppositionGroup.FeyAndUndead; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Average);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            SARegionDrops.GetSADrop(c);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}