using ElectricalProgressive.Content.Block;
using ElectricalProgressive.Content.Block.EGenerator;
using ElectricalProgressive.Utils;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace ElectricalProgressive.Content.Block.ETermoGenerator;

public class BlockETermoGenerator : BlockEBase
{
    public override bool CanAttachBlockAt(IBlockAccessor blockAccessor, Vintagestory.API.Common.Block block, BlockPos pos,
        BlockFacing blockFace, Cuboidi attachmentArea = null)
    {
        return true;
    }
    



    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack,
       BlockSelection blockSel, ref string failureCode)
    {
        var selection = new Selection(blockSel);
        Facing facing = Facing.None;

        try
        {
            facing = FacingHelper.From(selection.Face, selection.Direction);
        }
        catch
        {
            return false;
        }


        if (
            FacingHelper.Faces(facing).First() is { } blockFacing &&
            !world.BlockAccessor
                .GetBlock(blockSel.Position.AddCopy(blockFacing)).SideSolid[blockFacing.Opposite.Index]
        )
        {
            return false;
        }

        return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
    }




    //ставим блок
    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel,
        ItemStack byItemStack)
    {
        // если блок сгорел, то не ставим
        if (byItemStack.Block.Variant["type"] == "burned")
        {
            return false;
        }

        var selection = new Selection(blockSel);

        Facing facing = Facing.None;

        try
        {
            facing = FacingHelper.From(selection.Face, selection.Direction);
        }
        catch
        {
            return false;
        }

        if (
            base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack) &&
            world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityETermoGenerator entity
        )
        {
            entity.Facing = facing;                             //сообщаем направление

            //задаем параметры блока/проводника
            var voltage = MyMiniLib.GetAttributeInt(this, "voltage", 32);
            var maxCurrent = MyMiniLib.GetAttributeFloat(this, "maxCurrent", 5.0F);
            var isolated = MyMiniLib.GetAttributeBool(this, "isolated", false);
            var isolatedEnvironment = MyMiniLib.GetAttributeBool(this, "isolatedEnvironment", false);

            entity.Eparams = (
                new(voltage, maxCurrent, "", 0, 1, 1, false, isolated, isolatedEnvironment),
                FacingHelper.Faces(facing).First().Index);

               
            return true;
        }

        return false;
    }




    public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
    {
        base.OnNeighbourBlockChange(world, pos, neibpos);

        if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityETermoGenerator entity)
        {
            var faces = FacingHelper.Faces(entity.Facing);
            if (
            faces != null &&
            faces.Any() &&
            faces.First() is { } blockFacing &&
            !world.BlockAccessor.GetBlock(pos.AddCopy(blockFacing)).SideSolid[blockFacing.Opposite.Index])
            {
                world.BlockAccessor.BreakBlock(pos, null);
            }
        }
    }





    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer,
        float dropQuantityMultiplier = 1)
    {
        return new[] { OnPickBlock(world, pos) };
    }
}