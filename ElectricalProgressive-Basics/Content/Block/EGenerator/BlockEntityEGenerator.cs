using System;
using System.Collections.Generic;
using ElectricalProgressive.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace ElectricalProgressive.Content.Block.EGenerator;

public class BlockEntityEGenerator : BlockEntity
{
    private Facing facing = Facing.None;


    private BEBehaviorElectricalProgressive? ElectricalProgressive => GetBehavior<BEBehaviorElectricalProgressive>();

    public Facing Facing
    {
        get => this.facing;
        set
        {
            if (value != this.facing)
            {
                this.ElectricalProgressive.Connection =
                    FacingHelper.FullFace(this.facing = value);
            }
        }
    }

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


    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);

        tree.SetBytes("electricalprogressive:facing", SerializerUtil.Serialize(this.facing));
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        base.FromTreeAttributes(tree, worldAccessForResolve);

        try
        {
            this.facing = SerializerUtil.Deserialize<Facing>(tree.GetBytes("electricalprogressive:facing"));
        }
        catch (Exception exception)
        {
            this.Api?.Logger.Error(exception.ToString());
        }
    }
}
