using Enums;
using Globals;
using Godot;
using MobileEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileEntities.PlayerCharacters
{
    //ItemLogicHelper
    public partial class BaseCharacter : BaseMobileEntity
    {
        public void UseItem()
        {
            if (Inventory.ItemTypeOrder[Inventory.CurrentItemTypeIndex] == InventoryItemType.Torch)
            {
                if (Inventory.ItemsByType[InventoryItemType.Torch].Count > 0)
                {
                    var torch = InteractableScenePaths.TorchScenePath.Instantiate() as Torch;
                    _parentDungeonLevelSwapper.GetLatestBaseDungeonLevel().AddChild(torch);

                    torch.GlobalPosition = GlobalPosition;

                    torch.ZIndex = ZIndex - 1;

                    Inventory.ItemsByType[InventoryItemType.Torch].RemoveAt(0);

                }
            }
        }
    }
}
