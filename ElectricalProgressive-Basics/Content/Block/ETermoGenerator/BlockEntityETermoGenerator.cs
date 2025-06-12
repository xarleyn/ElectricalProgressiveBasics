using System;
using ElectricalProgressive;
using ElectricalProgressive.Content.Block;
using ElectricalProgressive.Content.Block.Termoplastini;
using ElectricalProgressive.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ElectricalProgressive.Content.Block.ETermoGenerator;

public class BlockEntityETermoGenerator : BlockEntityGenericTypedContainer
{
    private Facing facing = Facing.None;

    private BEBehaviorElectricalProgressive? ElectricalProgressive => GetBehavior<BEBehaviorElectricalProgressive>();

    public Facing Facing
    {
        get => this.facing;
        set
        {
            if (value != this.facing)
            {
                this.ElectricalProgressive.Connection =
                    FacingHelper.FullFace(this.facing = value);
            }
        }
    }

    //передает значения из Block в BEBehaviorElectricalProgressive
    public (EParams, int) Eparams
    {
        get => this.ElectricalProgressive?.Eparams ?? (new EParams(), 0);
        set => this.ElectricalProgressive!.Eparams = value;
    }

    //передает значения из Block в BEBehaviorElectricalProgressive
    public EParams[] AllEparams
    {
        get => this.ElectricalProgressive?.AllEparams ?? new EParams[]
                    {
                        new EParams(),
                        new EParams(),
                        new EParams(),
                        new EParams(),
                        new EParams(),
                        new EParams()
                    };
        set
        {
            if (this.ElectricalProgressive != null)
            {
                this.ElectricalProgressive.AllEparams = value;
            }
        }
    }




    ICoreClientAPI capi;
    ICoreServerAPI sapi;
    private InventoryTermoGenerator inventory;
    private GuiBlockEntityETermoGenerator clientDialog;

    
    private float prevGenTemp = 20f;
    public float genTemp = 20f;

    public static readonly float[] kpdPerHeight =
{
        0.15F, // 1-й 
        0.14F, // 2-й 
        0.13F, // 3-й 
        0.12F, // 4-й 
        0.11F, // 5-й 
        0.09F, // 6-й 
        0.08F, // 7-й 
        0.07F, // 8-й 
        0.06F, // 9-й 
        0.05F  // 10-й 
    };


    private int maxTemp;
    private float fuelBurnTime;
    private float maxBurnTime;
    public float GenTemp => genTemp;

    /// <summary>
    /// Собственно выходная максимальная мощность
    /// </summary>
    public float Power
    {
        get
        {
            if (kpd > 0)
                return genTemp * kpd / 2.0F;
            else
                return 1f;
        }
    }
    

    private float kpd=0f;

    private ItemSlot FuelSlot => this.inventory[0];
    private ItemStack FuelStack
    {
        get { return this.inventory[0].Itemstack; }
        set
        {
            this.inventory[0].Itemstack = value;
            this.inventory[0].MarkDirty();
        }
    }

    public override InventoryBase Inventory => inventory;

    public string DialogTitle => Lang.Get("termogen");

    public override string InventoryClassName => "termogen";

    public BlockEntityETermoGenerator()
    {
        this.inventory = new InventoryTermoGenerator(null, null);
        this.inventory.SlotModified += OnSlotModified;
    }


    /// <summary>
    /// Инициализация блока
    /// </summary>
    /// <param name="api"></param>
    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);

        if (api.Side == EnumAppSide.Server)
        {
            sapi = api as ICoreServerAPI;
        }
        else
        {
            capi = api as ICoreClientAPI;
        }

        this.inventory.Pos = this.Pos;
        this.inventory.LateInitialize($"{InventoryClassName}-{this.Pos.X}/{this.Pos.Y}/{this.Pos.Z}", api);

        this.RegisterGameTickListener(new Action<float>(OnBurnTick), 1000);

        CanDoBurn();
    }



    public override void OnBlockBroken(IPlayer byPlayer = null)
    {
        base.OnBlockBroken(null);
    }
    

    public void OnSlotModified(int slotId)
    {
        if (slotId == 0)
        {
            if (Inventory[0].Itemstack != null && !Inventory[0].Empty &&
                Inventory[0].Itemstack.Collectible.CombustibleProps != null)
            {
                if (fuelBurnTime == 0)
                    CanDoBurn();
            }
        }

        base.Block = this.Api.World.BlockAccessor.GetBlock(this.Pos);
        this.MarkDirty(this.Api.Side == EnumAppSide.Server, null);
        if (this.Api is ICoreClientAPI && this.clientDialog != null)
        {
            clientDialog.Update(genTemp, fuelBurnTime);
        }

        IWorldChunk chunkatPos = this.Api.World.BlockAccessor.GetChunkAtBlockPos(this.Pos);
        if (chunkatPos == null)
            return;

        chunkatPos.MarkModified();
    }




    public void OnBurnTick(float deltatime)
    {
        Calculate_kpd();

        if (this.Api is ICoreServerAPI)
        {
            if (fuelBurnTime > 0f)
            {
                genTemp = ChangeTemperature(genTemp, maxTemp, deltatime);
                fuelBurnTime -= deltatime; // burn!
                if (fuelBurnTime <= 0f)
                {
                    fuelBurnTime = 0f;
                    maxBurnTime = 0f;
                    maxTemp = 20; // important
                    if (!Inventory[0].Empty)
                        CanDoBurn();
                }
            }
            else
            {
                if (genTemp != 20f)
                    genTemp = ChangeTemperature(genTemp, 20f, deltatime);
                CanDoBurn();
            }

            

            MarkDirty(true, null);
        }

       

        // обновляем диалоговое окно на клиенте
        if (this.Api != null && this.Api.Side == EnumAppSide.Client)
        {
            if (this.clientDialog != null)
                clientDialog.Update(genTemp, fuelBurnTime);

        }
  
    }

    /// <summary>
    /// Расчет КПД генератора
    /// </summary>
    public void Calculate_kpd()
    {
        var accessor = this.Api.World.BlockAccessor;

        kpd = 0f;

        for (int i = 0; i < 10; i++)
        {
            var positions = Pos.UpCopy(i+1);
            var block = accessor.GetBlock(positions);

            if (block is BlockTermoplastini)
                kpd += kpdPerHeight[i];
            else
                break;
        }


    }



    /// <summary>
    /// Проверяет, можно ли сжечь топливо в генераторе
    /// </summary>
    public void CanDoBurn()
    {
        CombustibleProperties fuelProps = FuelSlot.Itemstack?.Collectible.CombustibleProps;
        if (fuelProps == null)
            return;

        if (fuelBurnTime > 0)
            return;

        if (fuelProps.BurnTemperature > 0f && fuelProps.BurnDuration > 0f)
        {
            maxBurnTime = fuelBurnTime = fuelProps.BurnDuration;
            maxTemp = fuelProps.BurnTemperature;
            FuelStack.StackSize--;
            if (FuelStack.StackSize <= 0)
            {
                FuelStack = null;
            }

            FuelSlot.MarkDirty();
            MarkDirty(true);
        }
    }


    /// <summary>
    /// Изменяет температуру в зависимости от времени и разницы температур
    /// </summary>
    /// <param name="fromTemp"></param>
    /// <param name="toTemp"></param>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    public float ChangeTemperature(float fromTemp, float toTemp, float deltaTime)
    {
        float diff = Math.Abs(fromTemp - toTemp);
        deltaTime += deltaTime * (diff / 28f);
        if (diff < deltaTime)
        {
            return toTemp;
        }

        if (fromTemp > toTemp)
        {
            deltaTime = -deltaTime;
        }

        if (Math.Abs(fromTemp - toTemp) < 1f)
        {
            return toTemp;
        }
        return fromTemp + deltaTime;
    }








    /// <summary>
    /// Обработчик нажатия правой кнопкой мыши по блоку, открывает диалоговое окно
    /// </summary>
    /// <param name="byPlayer"></param>
    /// <param name="blockSel"></param>
    /// <returns></returns>
    public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel)
    {
        // меняем состояние дверцы
        if (this.Api != null)
        {
            if (this.Block.Variant["state"] == "open")
            {
                var originalBlock = this.Api.World.BlockAccessor.GetBlock(Pos);
                var newBlockAL = originalBlock.CodeWithVariant("state", "closed");
                var newBlock = this.Api.World.GetBlock(newBlockAL);
                this.Api.World.BlockAccessor.ExchangeBlock(newBlock.Id, Pos);
                MarkDirty();
            }
            else if (this.Block.Variant["state"] == "closed")
            {
                var originalBlock = this.Api.World.BlockAccessor.GetBlock(Pos);
                var newBlockAL = originalBlock.CodeWithVariant("state", "open");
                var newBlock = this.Api.World.GetBlock(newBlockAL);
                this.Api.World.BlockAccessor.ExchangeBlock(newBlock.Id, Pos);
                MarkDirty();
            }
        }

        // открываем диалоговое окно
        if (this.Api.Side == EnumAppSide.Client)
        {
            base.toggleInventoryDialogClient(byPlayer, delegate
            {
                this.clientDialog =
                    new GuiBlockEntityETermoGenerator(DialogTitle, Inventory, this.Pos, this.capi, this);
                clientDialog.Update(genTemp, fuelBurnTime);
                return this.clientDialog;
            });
        }
        return true;
    }


    /// <summary>
    /// При установке блока, устанавливает соединение электричества
    /// </summary>
    /// <param name="byItemStack"></param>
    public override void OnBlockPlaced(ItemStack? byItemStack = null)
    {
        base.OnBlockPlaced(byItemStack);
        var electricity = ElectricalProgressive;
        if (electricity != null)
        {
            electricity.Connection = Facing.DownAll;
        }
    }


    /// <summary>
    /// При удалении блока, закрывает диалоговое окно и отключает электричество
    /// </summary>
    public override void OnBlockRemoved()
    {
        base.OnBlockRemoved();
        this.clientDialog?.TryClose();
        var electricity = ElectricalProgressive;

        if (electricity != null)
        {
            electricity.Connection = Facing.None;
        }
    }



    /// <summary>
    /// Сохраняет атрибуты
    /// </summary>
    /// <param name="tree"></param>
    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        ITreeAttribute invtree = new TreeAttribute();
        this.inventory.ToTreeAttributes(invtree);
        tree["inventory"] = invtree;
        tree.SetFloat("genTemp", genTemp);
        tree.SetInt("maxTemp", maxTemp);
        tree.SetFloat("fuelBurnTime", fuelBurnTime);
        tree.SetBytes("electricalprogressive:facing", SerializerUtil.Serialize(this.facing));
    }


    /// <summary>
    /// Загружает атрибуты 
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="worldForResolving"></param>
    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        this.inventory.FromTreeAttributes(tree.GetTreeAttribute("inventory"));
        if (Api != null)
            Inventory.AfterBlocksLoaded(this.Api.World);
        genTemp = tree.GetFloat("genTemp", 0);
        maxTemp = tree.GetInt("maxTemp", 0);
        fuelBurnTime = tree.GetFloat("fuelBurnTime", 0);

        if (Api != null && Api.Side == EnumAppSide.Client)
        {
            if (this.clientDialog != null)
                clientDialog.Update(genTemp, fuelBurnTime);
            MarkDirty(true, null);
        }

        try
        {
            this.facing = SerializerUtil.Deserialize<Facing>(tree.GetBytes("electricalprogressive:facing"));
        }
        catch (Exception exception)
        {
            this.Api?.Logger.Error(exception.ToString());
        }
    }
    
}