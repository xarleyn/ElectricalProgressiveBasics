using Vintagestory.API.Common;

namespace ElectricalProgressive.Content.Block.EGenerator;

public class BEBehaviorEGeneratorTier2 : BEBehaviorEGeneratorBase
{
    protected override float[] def_Params => new[] { 256, 0.75F, 0.25F, 1F, 0.05F, 0.85F };

    public BEBehaviorEGeneratorTier2(BlockEntity blockEntity) : base(blockEntity)
    {
        this.GetParams();
    }

    public override void GetParams()
    {
        base.GetParams();

        // Раньше эти значения переопределялись в расчете сопротивления, но непонятно зачем. Ведь они совпадают с прописанными в ассетах.
        // Хардкод ломал бы изменения ассетов
        // base_resistance = 0.05F; // Добавляем базовое сопротивление
        // kpd_max = 0.85F; // Максимальный КПД
    }
}
