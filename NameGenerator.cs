namespace SerializationBench
{
    public class NameGenerator
    {
        private static readonly string[] NamesArray =
        {
        "AriaLunastar", "LuminousSkyline", "Ezralightshine", "KaizenHorizon", "Novastardust",
        "LiamMoonrise", "Zarabluestream", "Milomystical", "Noragoldenwing", "Finnforestmoon",
        "AvaloniaSky", "EllaRadiance", "TheoSunburst", "Miladreamscape", "RheaSunshower",
        "LucasStarlight", "Islablueridge", "EthanLunarbeam", "Lilashadowmist", "LeviStarbreeze",
        "ZoeMoonshadow", "Evandawnseeker", "IvyWhitelight", "OwenNightshade", "RubySunflare",
        "EliSkywhisper", "Maelightfall", "IanStardust", "Cleosunglow", "MaxMoonbeam",
        "EvaSolarflare", "LeoStarrynight", "Miarainshimmer", "ZaneSunspark", "Rosemoonbeam",
        "Bengalaxystar", "Lylalightscape", "RyanStarmist", "Bellalunarlight", "SamsonSundream",
        "Zoeyskycloud", "Lukesunray", "Evewhitestar", "Reysunwhisper", "Rosasunbloom",
        "Jackmoondust", "Lilystarrynight", "Zackmoonsong", "Gialunarbeam", "Nicknightbreeze",
        "NinaSolarray", "Judemoonbeam", "Evesunglow", "JaySolarwave", "Lolastarwhisper",
        "Jakenightstar", "Ellasunstream", "Zekemooncloud", "Miastarshower", "Kylenightdream",
        "Anastardrift", "Deansunsparkle", "Joywhitecloud", "Rexmoonfall", "Saralightshade",
        "Alexstarglow", "Ellesundust", "Jacesolardream", "Minalunarshade", "Troysundrift"
    };

        public static string GetRandomName()
        {
            Random random = new Random();
            return NamesArray[random.Next(NamesArray.Length)];
        }
    }
}
