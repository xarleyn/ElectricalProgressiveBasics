using ElectricalProgressive.Content.Block.ECable;
using ElectricalProgressive.Utils;
using System.Linq;
using Vintagestory.API.Common;

namespace ElectricalProgressive.Content.Block.EConnector;

public class BlockEntityEConnector : BlockEntityECable {
    private BEBehaviorElectricalProgressive? ElectricalProgressive => GetBehavior<BEBehaviorElectricalProgressive>();


    public override void OnBlockPlaced(ItemStack? byItemStack = null) {
        base.OnBlockPlaced(byItemStack);

        var electricity = this.ElectricalProgressive;

        if (electricity != null) {
            electricity.Connection = Facing.AllAll;

            electricity.Eparams = (new EParams(128, 1024.0F, "", 0, 1, 1, false, true),0);
            electricity.Eparams = (new EParams(128, 1024.0F, "", 0, 1, 1, false, true), 1);
            electricity.Eparams = (new EParams(128, 1024.0F, "", 0, 1, 1, false, true), 2);
            electricity.Eparams = (new EParams(128, 1024.0F, "", 0, 1, 1, false, true), 3);
            electricity.Eparams = (new EParams(128, 1024.0F, "", 0, 1, 1, false, true), 4);
            electricity.Eparams = (new EParams(128, 1024.0F, "", 0, 1, 1, false, true), 5);

        }
    }



    //передает значения из Block в BEBehaviorElectricalProgressive
    public (EParams, int) Eparams
    {
        get => this.ElectricalProgressive!.Eparams;
        set => this.ElectricalProgressive!.Eparams = value;
    }

    //передает значения из Block в BEBehaviorElectricalProgressive
    public EParams[] AllEparams
    {
        get => this.ElectricalProgressive?.AllEparams ?? null;
        set
        {
            if (this.ElectricalProgressive != null)
            {
                this.ElectricalProgressive.AllEparams = value;
            }
        }
    }
}