using ElectricalProgressive.Content.Block.ECable;
using ElectricalProgressive.Utils;
using Vintagestory.API.Common;

namespace ElectricalProgressive.Content.Block.ESwitch;

public class BlockESwitch : Vintagestory.API.Common.Block
{
    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel,
        ItemStack byItemStack)
    {
        var selection = new Selection(blockSel);
        var face = FacingHelper.FromFace(selection.Face);

        if (
            !(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityECable blockEntity &&
              blockEntity.GetBehavior<BEBehaviorElectricalProgressive>() is { } electricity &&
              (blockEntity.Switches & face) == 0 &&
              (electricity.Connection & face) != 0)
        )
        {
            return false;
        }

        blockEntity.Switches = blockEntity.Switches & ~face | selection.Facing;
        blockEntity.SwitchesState |= face;
        blockEntity.MarkDirty(true);

        return true;
    }
}
