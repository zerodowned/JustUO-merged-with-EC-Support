namespace Server.Items
{
    public class AlchemistsBandage : Item
    {
        [Constructable]
        public AlchemistsBandage()
            : base(0xE21)
        {
            LootType = LootType.Blessed;
            Weight = 1.0;
            Hue = 0x482;
        }

        public AlchemistsBandage(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1075452; }
        } // Alchemist's Bandage

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