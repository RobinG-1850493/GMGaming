using System;
using System.Collections.Generic;

namespace GooseLib.Inventory
{
    public class Inventory
    {
        public const int DEFAULT_INVENTORY_SIZE = 8;
        
        private InventoryItem[] _slots;
        public int Size { get; private set; }

        public event Action<int, InventoryItem> OnItemChanged;
        public event Action<int> OnSlotCleared;

        public Inventory(int size = DEFAULT_INVENTORY_SIZE)
        {
            Size = size;
            _slots = new InventoryItem[size];
        }

        public InventoryItem GetItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Size)
                return null;
            
            return _slots[slotIndex];
        }

        public bool AddItem(InventoryItem item)
        {
            if (item == null) return false;

            // stack item if possible
            for (int i = 0; i < Size; i++)
            {
                if (_slots[i] != null && _slots[i].CanStack(item))
                {
                    int spaceInSlot = _slots[i].MaxStack - _slots[i].Quantity;
                    int amountToAdd = Math.Min(spaceInSlot, item.Quantity);
                    
                    _slots[i].Quantity += amountToAdd;
                    item.Quantity -= amountToAdd;
                    
                    OnItemChanged?.Invoke(i, _slots[i]);
                    
                    if (item.Quantity <= 0)
                        return true;
                }
            }

            // add to first empty slot
            for (int i = 0; i < Size; i++)
            {
                if (_slots[i] == null)
                {
                    _slots[i] = item.Clone();
                    OnItemChanged?.Invoke(i, _slots[i]);
                    return true;
                }
            }

            // inventory full
            return false;
        }

        public InventoryItem RemoveItem(int slotIndex, int quantity = 1)
        {
            if (slotIndex < 0 || slotIndex >= Size || _slots[slotIndex] == null)
                return null;

            InventoryItem item = _slots[slotIndex];
            int amountToRemove = Math.Min(quantity, item.Quantity);
            
            InventoryItem removedItem = new InventoryItem(item.Name, item.Description, item.Icon, amountToRemove, item.MaxStack, item.Data);
            
            item.Quantity -= amountToRemove;
            
            if (item.Quantity <= 0)
            {
                _slots[slotIndex] = null;
                OnSlotCleared?.Invoke(slotIndex);
            }
            else
            {
                OnItemChanged?.Invoke(slotIndex, item);
            }

            return removedItem;
        }

        public bool SetItem(int slotIndex, InventoryItem item)
        {
            if (slotIndex < 0 || slotIndex >= Size)
                return false;

            _slots[slotIndex] = item;
            
            if (item == null)
                OnSlotCleared?.Invoke(slotIndex);
            else
                OnItemChanged?.Invoke(slotIndex, item);

            return true;
        }

        public bool SwapItems(int slotA, int slotB)
        {
            if (slotA < 0 || slotA >= Size || slotB < 0 || slotB >= Size)
                return false;

            InventoryItem temp = _slots[slotA];
            _slots[slotA] = _slots[slotB];
            _slots[slotB] = temp;

            OnItemChanged?.Invoke(slotA, _slots[slotA]);
            OnItemChanged?.Invoke(slotB, _slots[slotB]);

            return true;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < Size; i++)
            {
                if (_slots[i] != null)
                    return false;
            }
            return true;
        }

        public bool IsFull()
        {
            for (int i = 0; i < Size; i++)
            {
                if (_slots[i] == null)
                    return false;
            }
            return true;
        }

        public int FindItem(string itemName)
        {
            for (int i = 0; i < Size; i++)
            {
                if (_slots[i] != null && _slots[i].Name == itemName)
                    return i;
            }
            return -1;
        }

        public List<InventoryItem> GetAllItems()
        {
            List<InventoryItem> items = new List<InventoryItem>();
            for (int i = 0; i < Size; i++)
            {
                if (_slots[i] != null)
                    items.Add(_slots[i]);
            }
            return items;
        }

        public bool ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Size)
                return false;

            _slots[slotIndex] = null;
            OnSlotCleared?.Invoke(slotIndex);
            return true;
        }
    }
}