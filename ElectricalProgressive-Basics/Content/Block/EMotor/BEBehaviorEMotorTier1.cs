using Vintagestory.API.Common;


namespace ElectricalProgressive.Content.Block.EMotor;

public class BEBehaviorEMotorTier1 : BEBehaviorEMotorBase
{
    protected override float[] def_Params => new[] { 10.0F, 128.0F, 0.5F, 0.75F, 0.5F, 0.125F, 0.05F };

    public BEBehaviorEMotorTier1(BlockEntity blockEntity) : base(blockEntity)
    {
        this.GetParams();
    }
}
