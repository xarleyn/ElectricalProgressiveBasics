using Vintagestory.API.Common;


namespace ElectricalProgressive.Content.Block.EMotor;

public class BEBehaviorEMotorTier3 : BEBehaviorEMotorBase
{
    protected override float[] def_Params => new[] { 10.0F, 512.0F, 2.0F, 0.95F, 1.0F, 0.5F, 0.05F };

    public BEBehaviorEMotorTier3(BlockEntity blockEntity) : base(blockEntity)
    {
        this.GetParams();
    }
}
