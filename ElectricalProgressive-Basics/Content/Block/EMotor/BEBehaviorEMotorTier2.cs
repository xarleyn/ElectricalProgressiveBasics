using Vintagestory.API.Common;


namespace ElectricalProgressive.Content.Block.EMotor;

public class BEBehaviorEMotorTier2 : BEBehaviorEMotorBase
{
    protected override float[] def_Params => new[] { 10.0F, 256.0F, 1.0F, 0.85F, 0.5F, 0.25F, 0.05F };

    public BEBehaviorEMotorTier2(BlockEntity blockEntity) : base(blockEntity)
    {
        this.GetParams();
    }
}
