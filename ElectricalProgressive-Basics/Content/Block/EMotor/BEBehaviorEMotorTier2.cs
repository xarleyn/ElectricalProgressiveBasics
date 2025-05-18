using System;
using System.Linq;
using System.Text;
using ElectricalProgressive.Interface;
using ElectricalProgressive.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;


namespace ElectricalProgressive.Content.Block.EMotor;

public class BEBehaviorEMotorTier2 : BEBehaviorMPBase, IElectricConsumer
{

    public BEBehaviorEMotorTier2(BlockEntity blockEntity) : base(blockEntity)
    {
        GetParams();
    }


    private static CompositeShape? compositeShape;

    private float powerRequest = I_max;         // Нужно энергии (сохраняется)
    private float powerReceive = 0;             // Дали энергии  (сохраняется)

    // Константы двигателя
    private static float I_min;                 // Минимальный ток
    private static float I_max;                 // Максимальный ток
    private static float torque_max;            // Максимальный крутящий момент
    private static float kpd_max;               // Пиковый КПД
    private static float speed_max;             // Максимальная скорость вращения
    private static float resistance_factor;     // множитель сопротивления
    private static float base_resistance;       // Базовое сопротивление
    private float torque;                       // Текущий крутящий момент
    private float I_value;                      // Ток потребления
    public float kpd;                           // КПД

    private float[] def_Params = { 10.0F, 100.0F, 0.5F, 0.75F, 0.5F, 0.1F, 0.05F };   //заглушка
    public float[] Params = { 0, 0, 0, 0, 0, 0, 0 };                              //сюда берем параметры из ассетов




    /// <summary>
    /// Извлекаем параметры из ассетов
    /// </summary>
    public void GetParams()
    {
        Params = MyMiniLib.GetAttributeArrayFloat(this.Block, "params", def_Params);
        I_min = Params[0];
        I_max = Params[1];
        torque_max = Params[2];
        kpd_max = Params[3];
        speed_max = Params[4];
        resistance_factor = Params[5];
        base_resistance = Params[6];
    }






    public override BlockFacing OutFacingForNetworkDiscovery
    {
        get
        {
            if (this.Blockentity is BlockEntityEMotor entity && entity.Facing != Facing.None)
            {
                return FacingHelper.Directions(entity.Facing).First();
            }

            return BlockFacing.NORTH;
        }
    }


    public override int[] AxisSign => this.OutFacingForNetworkDiscovery.Index switch
    {
        0 => new[]
        {
            +0,
            +0,
            -1
        },
        1 => new[]
        {
            -1,
            +0,
            +0
        },
        2 => new[]
        {
            +0,
            +0,
            -1
        },
        3 => new[]
        {
            -1,
            +0,
            +0
        },
        4 => new[]
        {
            +0,
            -1,
            +0
        },
        5 => new[]
        {
            +0,
            +1,
            +0
        },
        _ => this.AxisSign
    };



    public new BlockPos Pos => this.Position;

    /// <summary>
    /// Запрашивает энергию
    /// </summary>
    public float Consume_request()
    {
        return this.powerRequest;
    }

    /// <summary>
    /// Получает энергию
    /// </summary>
    public void Consume_receive(float amount)
    {
        this.powerReceive = amount;
    }


    public void Update()
    {
        //смотрим надо ли обновить модельку когда сгорает прибор
        if (this.Api.World.BlockAccessor.GetBlockEntity(this.Blockentity.Pos) is BlockEntityEMotor entity && entity.AllEparams != null)
        {
            bool hasBurnout = entity.AllEparams.Any(e => e.burnout);
            if (hasBurnout)
            {
                ParticleManager.SpawnBlackSmoke(this.Api.World, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
            if (hasBurnout && entity.Block.Variant["type"] != "burned")
            {
                string type = "type";
                string variant = "burned";

                this.Api.World.BlockAccessor.ExchangeBlock(Api.World.GetBlock(Block.CodeWithVariant(type, variant)).BlockId, Pos);
            }
        }

        this.Blockentity.MarkDirty(true);
    }





    public override void WasPlaced(BlockFacing connectedOnFacing)
    {

    }


    public float getPowerReceive()
    {
        return this.powerReceive;
    }

    public float getPowerRequest()
    {
        return this.powerRequest;
    }


    // не удалять
    public override float GetResistance()
    {

        if (isBurned)
        {
            return 9999.0F;
        }

        var spd = Math.Abs(Network?.Speed * GearedRatio ?? 0.0f);
        float base_resistance = 0.05F; // Добавляем базовое сопротивление

       
        return base_resistance +
               ((Math.Abs(spd) > speed_max)
                   ? resistance_factor * (float)Math.Pow((spd / speed_max), 2f)
                   : resistance_factor * spd / speed_max);
    



    }






    /// <summary>
    /// Основной метод поведения двигателя возвращающий момент и сопротивление
    /// </summary>
    public override float GetTorque(long tick, float speed, out float resistance)
    {

        torque = 0f;                            // Текущий крутящий момент
        resistance = GetResistance();         // Вычисляем текущее сопротивление двигателя    
        I_value = 0;                        // Ток потребления

        float I_amount = this.powerReceive;     // Доступно тока/энергии 

        if (I_amount <= I_min)                   // Если ток меньше минимального, двигатель не работает
            return torque;

        I_value = Math.Min(I_amount, I_max);    // Берем, что дают


        if (I_value < I_min)                                        // Если ток меньше минимального, двигатель не работает
            torque = 0.0F;
        else
            torque = I_value / I_max * torque_max;     // линейная


        torque *= kpd_max; // учитываем КПД


        this.powerRequest = I_max;                                        // Запрашиваем энергии столько, сколько нужно  для работы (работает как положено)


        return this.propagationDir == this.OutFacingForNetworkDiscovery     // Возвращаем все значения
            ? 1f * torque
            : -1f * torque;

    }


    public bool isBurned => this.Block.Variant["type"] == "burned";


    /// <summary>
    /// Выдается игре шейп для отрисовки ротора
    /// </summary>
    /// <returns></returns>
    protected override CompositeShape? GetShape()
    {
        if (this.Api is { } api && this.Blockentity is BlockEntityEMotor entity && entity.Facing != Facing.None && entity.Block.Variant["type"] != "burned") //какой тип )

        {
            var direction = this.OutFacingForNetworkDiscovery;

            if (BEBehaviorEMotorTier2.compositeShape == null)
            {
                string tier = entity.Block.Variant["tier"];             //какой тир
                string type = "rotor";
                string[] types = new string[2] { "tier", "type" };//типы генератора
                string[] variants = new string[2] { tier, type };//нужные вариант генератора

                var location = this.Block.CodeWithVariants(types, variants);
                BEBehaviorEMotorTier2.compositeShape = api.World.BlockAccessor.GetBlock(location).Shape.Clone();
            }

            var shape = BEBehaviorEMotorTier2.compositeShape.Clone();

            if (direction == BlockFacing.NORTH)
            {
                shape.rotateY = 0;
            }

            if (direction == BlockFacing.EAST)
            {
                shape.rotateY = 270;
            }

            if (direction == BlockFacing.SOUTH)
            {
                shape.rotateY = 180;
            }

            if (direction == BlockFacing.WEST)
            {
                shape.rotateY = 90;
            }

            if (direction == BlockFacing.UP)
            {
                shape.rotateX = 90;
            }

            if (direction == BlockFacing.DOWN)
            {
                shape.rotateX = 270;
            }

            return shape;
        }

        return null;
    }

    protected override void updateShape(IWorldAccessor worldForResolve)
    {
        this.Shape = this.GetShape();
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        return false;
    }



    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetFloat("electricalprogressive:powerRequest", powerRequest);
        tree.SetFloat("electricalprogressive:powerReceive", powerReceive);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        base.FromTreeAttributes(tree, worldAccessForResolve);
        powerRequest = tree.GetFloat("electricalprogressive:powerRequest");
        powerReceive = tree.GetFloat("electricalprogressive:powerReceive");
    }


    /// <summary>
    /// Подсказка при наведении на блок
    /// </summary>
    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder stringBuilder)
    {
        base.GetBlockInfo(forPlayer, stringBuilder);

        //проверяем не сгорел ли прибор
        if (this.Api.World.BlockAccessor.GetBlockEntity(this.Blockentity.Pos) is BlockEntityEMotor entity)
        {
            if (isBurned)
            {
                stringBuilder.AppendLine(Lang.Get("Burned"));
            }
            else
            {
                stringBuilder.AppendLine(StringHelper.Progressbar(powerReceive / I_max * 100));
                stringBuilder.AppendLine("└ " + Lang.Get("Consumption") + ": " + powerReceive + "/" + I_max + " " + Lang.Get("W"));
            }

        }
        stringBuilder.AppendLine();
    }


}
