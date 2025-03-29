using ElectricalProgressive.Content.Block.EAccumulator;
using ElectricalProgressive.Content.Block.EConnector;
using ElectricalProgressive.Content.Block.EGenerator;
using ElectricalProgressive.Content.Block.EMotor;
using Vintagestory.API.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using ElectricalProgressive.Content.Block;
using ElectricalProgressive.Content.Block.ESwitch;
using ElectricalProgressive.Interface;
using ElectricalProgressive.Utils;
using Vintagestory.API.MathTools;
using Vintagestory.API.Client;
using Vintagestory.API.Util;
using ElectricalProgressive.Content.Block.ECable;
using ElectricalProgressive.Content.Block.ETransformator;




[assembly: ModDependency("game", "1.20.0")]
[assembly: ModDependency("electricalprogressivecore", "0.9.0")]
[assembly: ModInfo(
    "Electrical Progressive: Basics",
    "electricalprogressivebasics",
    Website = "https://git",
    Description = "Brings electricity into the game!",
    Version = "0.9.0",
    Authors = new[] {
        "Tehtelev",
        "Kotl"
    }
)]

namespace ElectricalProgressive;

public class ElectricalProgressiveBasics : ModSystem
{

    private ICoreAPI api = null!;
    private ICoreClientAPI capi = null!;





    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        this.api = api;

        api.RegisterBlockClass("BlockECable", typeof(BlockECable));
        api.RegisterBlockEntityClass("BlockEntityECable", typeof(BlockEntityECable));

        api.RegisterBlockClass("BlockESwitch", typeof(BlockESwitch));

        api.RegisterBlockClass("BlockEAccumulator", typeof(BlockEAccumulator));
        api.RegisterBlockEntityClass("BlockEntityEAccumulator", typeof(BlockEntityEAccumulator));
        api.RegisterBlockEntityBehaviorClass("BEBehaviorEAccumulator", typeof(BEBehaviorEAccumulator));

        api.RegisterBlockClass("BlockConnector", typeof(BlockConnector));
        api.RegisterBlockEntityClass("BlockEntityEConnector", typeof(BlockEntityEConnector));



        api.RegisterBlockClass("BlockETransformator", typeof(BlockETransformator));
        api.RegisterBlockEntityClass("BlockEntityETransformator", typeof(BlockEntityETransformator));
        api.RegisterBlockEntityBehaviorClass("BEBehaviorETransformator", typeof(BEBehaviorETransformator));



        api.RegisterBlockClass("BlockEMotor", typeof(BlockEMotor));
        api.RegisterBlockEntityClass("BlockEntityEMotor", typeof(BlockEntityEMotor));
        api.RegisterBlockEntityBehaviorClass("BEBehaviorEMotorTier1", typeof(BEBehaviorEMotorTier1));
        api.RegisterBlockEntityBehaviorClass("BEBehaviorEMotorTier2", typeof(BEBehaviorEMotorTier2));
        api.RegisterBlockEntityBehaviorClass("BEBehaviorEMotorTier3", typeof(BEBehaviorEMotorTier3));

        api.RegisterBlockClass("BlockEGenerator", typeof(BlockEGenerator));
        api.RegisterBlockEntityClass("BlockEntityEGenerator", typeof(BlockEntityEGenerator));
        api.RegisterBlockEntityBehaviorClass("BEBehaviorEGeneratorTier1", typeof(BEBehaviorEGeneratorTier1));
        api.RegisterBlockEntityBehaviorClass("BEBehaviorEGeneratorTier2", typeof(BEBehaviorEGeneratorTier2));
        api.RegisterBlockEntityBehaviorClass("BEBehaviorEGeneratorTier3", typeof(BEBehaviorEGeneratorTier3));

        api.RegisterBlockEntityBehaviorClass("ElectricalProgressive", typeof(BEBehaviorElectricalProgressive));


    }






    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        this.capi = api;
    }

}