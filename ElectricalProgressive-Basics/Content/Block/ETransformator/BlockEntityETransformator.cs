using ElectricalProgressive.Content.Block;
using ElectricalProgressive.Utils;
using System;
using System.Linq;
using Vintagestory.API.Common;

namespace ElectricalProgressive.Content.Block.ETransformator;

public class BlockEntityETransformator : BlockEntity
{
    private BEBehaviorElectricalProgressive? ElectricalProgressive => GetBehavior<BEBehaviorElectricalProgressive>();


    //передает значения из Block в BEBehaviorElectricalProgressive
    public (EParams, int) Eparams
    {
        get => this.ElectricalProgressive?.Eparams ?? (new EParams(), 0);
        set => this.ElectricalProgressive!.Eparams = value;
    }

    //передает значения из Block в BEBehaviorElectricalProgressive
    public EParams[] AllEparams
    {
        get => this.ElectricalProgressive?.AllEparams ?? new EParams[]
                    {
                        new EParams(),
                        new EParams(),
                        new EParams(),
                        new EParams(),
                        new EParams(),
                        new EParams()
                    };
        set
        {
            if (this.ElectricalProgressive != null)
            {
                this.ElectricalProgressive.AllEparams = value;
            }
        }
    }



    public override void OnBlockPlaced(ItemStack? byItemStack = null)
    {
        base.OnBlockPlaced(byItemStack);

        //задаем параметры блока/проводника
        var voltage = MyMiniLib.GetAttributeInt(byItemStack!.Block, "voltage", 32);
        var lowVoltage = MyMiniLib.GetAttributeInt(byItemStack!.Block, "lowVoltage", 32);
        var maxCurrent = MyMiniLib.GetAttributeFloat(byItemStack!.Block, "maxCurrent", 5.0F);
        var isolated = MyMiniLib.GetAttributeBool(byItemStack!.Block, "isolated", false);

        this.ElectricalProgressive!.Connection = Facing.DownAll;
        this.ElectricalProgressive.Eparams = (
            new EParams(voltage, maxCurrent, "", 0, 1, 1, false, isolated),
            FacingHelper.Faces(Facing.DownAll).First().Index);

    }
}
