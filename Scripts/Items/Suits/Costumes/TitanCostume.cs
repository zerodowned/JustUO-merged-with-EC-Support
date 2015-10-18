namespace Server.Items
{
    public class TitanCostume : BaseCostume
    {
        [Constructable]
        public TitanCostume()
        {
            Name = "a titan halloween costume";
            CostumeBody = 76;
        }

        public TitanCostume(Serial serial) : base(serial)
        {
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