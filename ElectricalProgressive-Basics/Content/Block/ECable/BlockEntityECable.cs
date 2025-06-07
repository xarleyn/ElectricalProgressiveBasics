using ElectricalProgressive.Utils;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace ElectricalProgressive.Content.Block.ECable
{
    public class BlockEntityECable : BlockEntityEBase
    {
        private Facing switches = Facing.None;

        public Facing Connection
        {
            get => this.ElectricalProgressive?.Connection ?? Facing.None;
            set
            {
                if (this.ElectricalProgressive != null)
                {
                    this.ElectricalProgressive.Connection = value;
                }
            }
        }

        public Facing Switches
        {
            get => this.switches;
            set => this.ElectricalProgressive!.Interruption &= this.switches = value;
        }

        public const string SwitchesKey = "electricalprogressive:switches";

        public Facing SwitchesState
        {
            get => ~this.ElectricalProgressive!.Interruption;
            set => this.ElectricalProgressive!.Interruption = this.switches & ~value;
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetBytes(SwitchesKey, SerializerUtil.Serialize(this.switches));
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            try
            {
                this.switches = SerializerUtil.Deserialize<Facing>(tree.GetBytes(SwitchesKey));
            }
            catch (Exception exception)
            {
                this.Api?.Logger.Error(exception.ToString());
            }
        }
    }
}
