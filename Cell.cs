using MessagePack;
namespace SerializationBench
{
    [MessagePackObject]
    public class Cell
    {
        [Key(0)]
        public float X { get; set; }

        [Key(1)]
        public float Y { get; set; }

        [Key(2)]
        public float R { get; set; }

        [IgnoreMember] // Mass不进行序列化
        public float Mass { get => MathF.PI * R * R; }

        [Key(3)]
        public uint ID { get; set; }

        [Key(4)]
        public byte Color { get; set; }

        [Key(5)]
        public string Name { get; set; }

        private static uint MaxId = 1;

        [IgnoreMember]
        private Random rand = new();

        public Cell()
        {
            X = rand.Next(0,1000);
            Y = rand.Next(0, 1000);
            R = rand.Next(10, 30); ;
            ID = MaxId++;
            Color = (byte)rand.Next(255);
            Name = NameGenerator.GetRandomName();
        }

        public Cell(float x, float y, float r, uint id, byte color, string name)
        {
            X = x;
            Y = y;
            R = r;              
            ID = id;
            Color = color;
            Name = name;            
        }

        public override string ToString() => $"Cell(ID: {ID}, X: {X}, Y: {Y}, R: {R}, Color: {Color}, Name: {Name})";

    }
}
