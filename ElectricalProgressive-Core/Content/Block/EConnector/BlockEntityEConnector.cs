using ElectricalProgressive.Content.Block.ECable;
using ElectricalProgressive.Utils;
using System.Linq;
using Vintagestory.API.Common;

namespace ElectricalProgressive.Content.Block.EConnector;

public class BlockEntityEConnector : BlockEntityECable {
    private BEBehaviorElectricalProgressive? ElectricityAddon => GetBehavior<BEBehaviorElectricalProgressive>();


    public override void OnBlockPlaced(ItemStack? byItemStack = null) {
        base.OnBlockPlaced(byItemStack);

        var electricity = this.ElectricityAddon;

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



    //передает значения из Block в BEBehaviorElectricityAddon
    public (EParams, int) Eparams
    {
        get => this.ElectricityAddon!.Eparams;
        set => this.ElectricityAddon!.Eparams = value;
    }

    //передает значения из Block в BEBehaviorElectricityAddon
    public EParams[] AllEparams
    {
        get => this.ElectricityAddon?.AllEparams ?? null;
        set
        {
            if (this.ElectricityAddon != null)
            {
                this.ElectricityAddon.AllEparams = value;
            }
        }
    }
}