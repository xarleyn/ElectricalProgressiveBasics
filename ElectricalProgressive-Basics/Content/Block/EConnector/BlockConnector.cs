using ElectricalProgressive.Content.Block;
using ElectricalProgressive.Content.Block.ECable;
using ElectricalProgressive.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace ElectricalProgressive.Content.Block.EConnector;

public class BlockConnector : Vintagestory.API.Common.Block
{


    private ICoreAPI api;

    DamageEntityByElectricity damageEntityByElectricity;

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);

        this.api = api;

        damageEntityByElectricity = new DamageEntityByElectricity(api);
    }




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
        // если это клиент, то не надо 
        if (world.Side == EnumAppSide.Client)
            return;

        // энтити не живой? выходим
        if (!entity.Alive)
            return;

        // получаем блокэнтити этого блока
        var blockentity = (BlockEntityEConnector)world.BlockAccessor.GetBlockEntity(pos);

        // если блокэнтити не найден, выходим
        if (blockentity == null)
            return;

        // передаем работу в наш обработчик урона
        damageEntityByElectricity.Damage(world, entity, pos, facing, blockentity.AllEparams, this);

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