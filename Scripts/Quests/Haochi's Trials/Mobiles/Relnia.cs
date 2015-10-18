using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Samurai
{
    public class Relnia : BaseQuester
    {
        [Constructable]
        public Relnia()
            : base("the Gypsy")
        {
        }

        public Relnia(Serial serial)
            : base(serial)
        {
        }

        public override int TalkNumber
        {
            get { return -1; }
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Hue = 0x83FF;

            Female = true;
            Body = 0x191;
            Name = "Disheveled Relnia";
        }

        public override void InitOutfit()
        {
            HairItemID = 0x203C;
            HairHue = 0x654;

            AddItem(new ThighBoots(0x901));
            AddItem(new FancyShirt(0x5F3));
            AddItem(new SkullCap(0x6A7));
            AddItem(new Skirt(0x544));
        }

        public override void OnTalk(PlayerMobile player, bool contextMenu)
        {
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            var player = from as PlayerMobile;

            if (player != null)
            {
                var qs = player.Quest;

                if (qs is HaochisTrialsQuest)
                {
                    var obj = qs.FindObjective(typeof (FourthTrialCatsObjective));

                    if (obj != null && !obj.Completed)
                    {
                        var gold = dropped as Gold;

                        if (gold != null)
                        {
                            obj.Complete();
                            qs.AddObjective(new FourthTrialReturnObjective(false));

                            SayTo(from, 1063241); // I thank thee.  This gold will be a great help to me and mine!

                            gold.Consume(); // Intentional difference from OSI: don't take all the gold of poor newbies!
                            return gold.Deleted;
                        }
                    }
                }
            }

            return base.OnDragDrop(from, dropped);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadEncodedInt();
        }
    }
}