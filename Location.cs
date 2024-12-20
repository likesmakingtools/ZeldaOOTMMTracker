using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaOOTMMTracker
{
    class Location
    {
        private bool isOOT = false;

        public string ID;
        public string Name;
        public string Game;
        public List<string> ItemLocations;
        public List<string> Items;

        public Location(int index, string group)
        {
            ID = $"loc{index}";
            Game = string.Empty;
            ItemLocations = new List<string>();
            Items = new List<string>();

            string[] locations = group.Replace("  ", "").Split("\n");
            int startIndex = 1;
            if (group.StartsWith("Location List")) startIndex = 2;

            Name = locations[startIndex - 1];
            for (int i = startIndex; i < locations.Length; i++)
            {
                string[] details = locations[i].Split(": ");
                if (details.Length > 1)
                {
                    ItemLocations.Add(details[0]);
                    Items.Add(details[1]);
                    if (details[0].StartsWith("OOT"))
                    {
                        isOOT = true;
                        Game = "Ocarina of Time";
                    }
                    if (details[0].StartsWith("MM"))
                        Game = "Majora's Mask";
                }
            }
        }

        public bool IsOOT() { return isOOT;}
    }

    class MyData
    {
        Dictionary<string, bool> data = new Dictionary<string, bool>();
    }
}
