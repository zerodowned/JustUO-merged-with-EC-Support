using System.Collections.Generic;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Quests.Haven
{
    public class SchmendrickApprenticeCorpse : Corpse
    {
        private static int m_HairHue;
        private Lantern m_Lantern;

        [Constructable]
        public SchmendrickApprenticeCorpse()
            : base(GetOwner(), GetHair(), GetFacialHair(), GetEquipment())
        {
            Direction = Direction.West;

            foreach (var item in EquipItems)
            {
                DropItem(item);
            }

            m_Lantern = new Lantern();
            m_Lantern.Movable = false;
            m_Lantern.Protected = true;
            m_Lantern.Ignite();
        }

        public SchmendrickApprenticeCorpse(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (ItemID == 0x2006) // Corpse form
            {
                list.Add("a human corpse");
                list.Add(1049144, Name); // the remains of ~1_NAME~ the apprentice
            }
            else
            {
                list.Add(1049145); // the remains of a wizard's apprentice
            }
        }

        public override void OnSingleClick(Mobile from)
        {
            var hue = Notoriety.GetHue(NotorietyHandlers.CorpseNotoriety(from, this));

            if (ItemID == 0x2006) // Corpse form
                from.Send(new MessageLocalized(Serial, ItemID, MessageType.Label, hue, 3, 1049144, "", Name));
                    // the remains of ~1_NAME~ the apprentice
            else
                from.Send(new MessageLocalized(Serial, ItemID, MessageType.Label, hue, 3, 1049145, "", ""));
                    // the remains of a wizard's apprentice
        }

        public override void Open(Mobile from, bool checkSelfLoot)
        {
            if (!from.InRange(GetWorldLocation(), 2))
                return;

            var player = from as PlayerMobile;

            if (player != null)
            {
                var qs = player.Quest;

                if (qs is UzeraanTurmoilQuest)
                {
                    var obj = qs.FindObjective(typeof (FindApprenticeObjective));

                    if (obj != null && !obj.Completed)
                    {
                        Item scroll = new SchmendrickScrollOfPower();

                        if (player.PlaceInBackpack(scroll))
                        {
                            player.SendLocalizedMessage(1049147, "", 0x22);
                                // You find the scroll and put it in your pack.
                            obj.Complete();
                        }
                        else
                        {
                            player.SendLocalizedMessage(1049146, "", 0x22);
                                // You find the scroll, but can't pick it up because your pack is too full.  Come back when you have more room in your pack.
                            scroll.Delete();
                        }

                        return;
                    }
                }
            }

            from.SendLocalizedMessage(1049143, "", 0x22);
                // This is the corpse of a wizard's apprentice.  You can't bring yourself to search it without a good reason.
        }

        public override void OnLocationChange(Point3D oldLoc)
        {
            if (m_Lantern != null && !m_Lantern.Deleted)
                m_Lantern.Location = new Point3D(X, Y + 1, Z);
        }

        public override void OnMapChange()
        {
            if (m_Lantern != null && !m_Lantern.Deleted)
                m_Lantern.Map = Map;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Lantern != null && !m_Lantern.Deleted)
                m_Lantern.Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            if (m_Lantern != null && m_Lantern.Deleted)
                m_Lantern = null;

            base.Serialize(writer);

            writer.Write(0); // version

            writer.Write(m_Lantern);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();

            m_Lantern = (Lantern) reader.ReadItem();
        }

        private static Mobile GetOwner()
        {
            var apprentice = new Mobile();

            apprentice.Hue = Utility.RandomSkinHue();
            apprentice.Female = false;
            apprentice.Body = 0x190;
            apprentice.Name = NameList.RandomName("male");

            apprentice.Delete();

            return apprentice;
        }

        private static List<Item> GetEquipment()
        {
            var list = new List<Item>();

            list.Add(new Robe(QuestSystem.RandomBrightHue()));
            list.Add(new WizardsHat(Utility.RandomNeutralHue()));
            list.Add(new Shoes(Utility.RandomNeutralHue()));

            /*
            int hairHue = Utility.RandomHairHue();

            switch ( Utility.Random( 8 ) )
            {
            case 0: list.Add( new Afro( hairHue ) ); break;
            case 1: list.Add( new KrisnaHair( hairHue ) ); break;
            case 2: list.Add( new PageboyHair( hairHue ) ); break;
            case 3: list.Add( new PonyTail( hairHue ) ); break;
            case 4: list.Add( new ReceedingHair( hairHue ) ); break;
            case 5: list.Add( new TwoPigTails( hairHue ) ); break;
            case 6: list.Add( new ShortHair( hairHue ) ); break;
            case 7: list.Add( new LongHair( hairHue ) ); break;
            }

            switch ( Utility.Random( 5 ) )
            {
            case 0: list.Add( new LongBeard( hairHue ) ); break;
            case 1: list.Add( new MediumLongBeard( hairHue ) ); break;
            case 2: list.Add( new Vandyke( hairHue ) ); break;
            case 3: list.Add( new Mustache( hairHue ) ); break;
            case 4: list.Add( new Goatee( hairHue ) ); break;
            }
            * */

            list.Add(new Spellbook());

            return list;
        }

        private static HairInfo GetHair()
        {
            m_HairHue = Race.Human.RandomHairHue();

            return new HairInfo(Race.Human.RandomHair(false), m_HairHue);
        }

        private static FacialHairInfo GetFacialHair()
        {
            m_HairHue = Race.Human.RandomHairHue();

            return new FacialHairInfo(Race.Human.RandomFacialHair(false), m_HairHue);
        }
    }
}