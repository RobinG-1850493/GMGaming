using Microsoft.Xna.Framework.Graphics;

namespace GooseLib.Inventory
{
    public class InventoryItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Texture2D Icon { get; set; }
        public int Quantity { get; set; }
        public int MaxStack { get; set; }
        public object Data { get; set; } 

        public InventoryItem(string name, string description, Texture2D icon, int quantity = 1, int maxStack = 1, object data = null)
        {
            Name = name;
            Description = description;
            Icon = icon;
            Quantity = quantity;
            MaxStack = maxStack;
            Data = data;
        }

        public bool CanStack(InventoryItem other)
        {
            return other != null && 
                   Name == other.Name && 
                   Quantity < MaxStack && 
                   other.Quantity < other.MaxStack;
        }

        public InventoryItem Clone()
        {
            return new InventoryItem(Name, Description, Icon, Quantity, MaxStack, Data);
        }
    }
}