using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileEntities.PlayerCharacters
{
    public class Inventory
    {
        public Dictionary<InventoryItemType, List<InventoryItem>> ItemsByType = new Dictionary<InventoryItemType, List<InventoryItem>>();

        public List<InventoryItemType> ItemTypeOrder = new List<InventoryItemType>();

        public int CurrentItemTypeIndex = 0;
    }
}
