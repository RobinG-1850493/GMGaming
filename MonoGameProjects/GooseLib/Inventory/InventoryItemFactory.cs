using GooseLib.Weapons;
using Microsoft.Xna.Framework.Graphics;

namespace GooseLib.Inventory
{
    public static class InventoryItemFactory
    {
        public static InventoryItem CreateWeaponItem(Weapon weapon)
        {
            return new InventoryItem(
                name: weapon.Name,
                description: $"Damage: {weapon.Damage}, Range: {weapon.Range}",
                icon: weapon.Texture,
                quantity: 1,
                maxStack: 1,
                data: weapon
            );
        }

        public static InventoryItem CreateConsumableItem(string name, string description, Texture2D icon, int quantity = 1, int maxStack = 99)
        {
            return new InventoryItem(
                name: name,
                description: description,
                icon: icon,
                quantity: quantity,
                maxStack: maxStack
            );
        }

        public static InventoryItem CreateMaterialItem(string name, string description, Texture2D icon, int quantity = 1, int maxStack = 64)
        {
            return new InventoryItem(
                name: name,
                description: description,
                icon: icon,
                quantity: quantity,
                maxStack: maxStack
            );
        }
    }
}