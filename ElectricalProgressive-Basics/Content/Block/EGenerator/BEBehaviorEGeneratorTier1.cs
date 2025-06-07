using Vintagestory.API.Common;

namespace ElectricalProgressive.Content.Block.EGenerator;

public class BEBehaviorEGeneratorTier1 : BEBehaviorEGeneratorBase
{
    protected override float[] def_Params => new[] { 128.0F, 0.5F, 0.125F, 0.5F, 0.05F, 0.75F };

    public BEBehaviorEGeneratorTier1(BlockEntity blockEntity)
        : base(blockEntity)
    {
        this.GetParams();
    }
}
