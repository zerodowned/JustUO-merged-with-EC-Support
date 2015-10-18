using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a speckled scorpion corpse")]
    public class SpeckledScorpion : Scorpion
    {
        [Constructable]
        public SpeckledScorpion()
        {
            Name = "a speckled scorpion";
        }

        public SpeckledScorpion(Serial serial)
            : base(serial)
        {
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.4)
                c.DropItem(new SpeckledPoisonSac());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }
}