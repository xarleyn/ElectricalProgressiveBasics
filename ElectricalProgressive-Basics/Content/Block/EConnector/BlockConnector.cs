using ElectricalProgressive.Content.Block;
using ElectricalProgressive.Content.Block.ECable;
using ElectricalProgressive.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace ElectricalProgressive.Content.Block.EConnector;

public class BlockConnector : Vintagestory.API.Common.Block {


    private const float DAMAGE_AMOUNT = 0.1f;
    private const double KNOCKBACK_STRENGTH = 0.4;
    // Интервал в миллисекундах (2 секунды)
    private const long DAMAGE_INTERVAL_MS = 2000;

    // Ключ для хранения времени удара
    private const string key = "damageByElectricity";


    public global::ElectricalProgressive.ElectricalProgressive? System =>
        this.api?.ModLoader.GetModSystem<global::ElectricalProgressive.ElectricalProgressive>();



    /// <summary>
    /// Кто-то или что-то коснулось блока и теперь получит урон
    /// </summary>
    /// <param name="world"></param>
    /// <param name="entity"></param>
    /// <param name="pos"></param>
    /// <param name="facing"></param>
    /// <param name="collideSpeed"></param>
    /// <param name="isImpact"></param>
    public override void OnEntityCollide(
        IWorldAccessor world,
        Entity entity,
        BlockPos pos,
        BlockFacing facing,
        Vec3d collideSpeed,
        bool isImpact
    )
    {
        if (world.Side == EnumAppSide.Client)
        {
            return;
        }


        bool doDamage = false;

        var blockentity = (BlockEntityEConnector)world.BlockAccessor.GetBlockEntity(pos);

        for (int i = 0; i <= 5; i++) //перебор всех граней
        {
            var networkInformation = this.System?.GetNetworks(pos, FacingHelper.FromFace(FacingHelper.BlockFacingFromIndex(i)));      //получаем информацию о сети

            if (networkInformation?.NumberOfProducers > 0 || networkInformation?.NumberOfAccumulators > 0) //если в сети есть генераторы или аккумы
            {
                if (blockentity != null && blockentity.AllEparams != null) //энтити существует?
                {
                    var par = blockentity.AllEparams[i];
                    if (!par.burnout)           //не сгорел?
                    {
                        if (!par.isolated)      //не изолированный?
                        {
                            doDamage = true;   //значит урон разрешаем
                            break;
                        }
                    }

                }
            }
        }


        if (!doDamage)
            return;

        // Текущее время в миллисекундах с запуска сервера
        long now = world.ElapsedMilliseconds;


        double last = entity.Attributes.GetDouble(key);

        if (last > now) last = 0;

        // Если прошло >= 2 секунд, наносим урон и сбрасываем таймер
        if (now - last >= DAMAGE_INTERVAL_MS)
        {
            // 1) Наносим урон
            var dmg = new DamageSource()
            {
                Source = EnumDamageSource.Block,
                SourceBlock = this,
                Type = EnumDamageType.Electricity,
                SourcePos = pos.ToVec3d()
            };
            entity.ReceiveDamage(dmg, DAMAGE_AMOUNT);

            // 2) Вычисляем вектор от блока к сущности и отталкиваем
            Vec3d center = pos.ToVec3d().Add(0.5, 0.5, 0.5);
            Vec3d diff = entity.ServerPos.XYZ - center;
            diff.Y = 0.2; // небольшой подъём
            diff.Normalize();

            entity.Attributes.SetDouble("kbdirX", diff.X * KNOCKBACK_STRENGTH);
            entity.Attributes.SetDouble("kbdirY", diff.Y * KNOCKBACK_STRENGTH);
            entity.Attributes.SetDouble("kbdirZ", diff.Z * KNOCKBACK_STRENGTH);

            // 3) Запоминаем время удара
            entity.Attributes.SetDouble(key, now);
        }
    }


    public override void OnBlockBroken(IWorldAccessor world, BlockPos position, IPlayer byPlayer, float dropQuantityMultiplier = 1) {
            if (this.api is ICoreClientAPI) {
                return;
            }

            if (world.BlockAccessor.GetBlockEntity(position) is BlockEntityECable  entity) {
                if (byPlayer is { CurrentBlockSelection: { } blockSelection }) {
                    var connection = entity.Connection & ~Facing.AllAll;

                    if (connection != Facing.None) {
                        var stackSize = FacingHelper.Count(Facing.AllAll);

                        if (stackSize > 0) {
                            entity.Connection = connection;
                            entity.MarkDirty(true);
                            return;
                        }
                    }
                }
            }

            base.OnBlockBroken(world, position, byPlayer, dropQuantityMultiplier);
        }
        
        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos) {
            base.OnNeighbourBlockChange(world, pos, neibpos);

            if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityECable entity) {
                var blockFacing = BlockFacing.FromVector(neibpos.X - pos.X, neibpos.Y - pos.Y, neibpos.Z - pos.Z);
                var selectedFacing = FacingHelper.FromFace(blockFacing);

                if ((entity.Connection & ~ selectedFacing) == Facing.None) {
                    world.BlockAccessor.BreakBlock(pos, null);

                    return;
                }



            }
        }
    }