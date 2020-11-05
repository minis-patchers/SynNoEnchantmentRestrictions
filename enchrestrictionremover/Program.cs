using System;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;

namespace enchrestrictionremover
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return SynthesisPipeline.Instance.Patch<ISkyrimMod, ISkyrimModGetter>(
                args: args,
                patcher: RunPatch,
                new UserPreferences() {
                    ActionsForEmptyArgs = new RunDefaultPatcher
                    {
                        IdentifyingModKey = "EnchRestricts.esp",
                        TargetRelease = GameRelease.SkyrimSE
                    }
                });
        }

        public static void RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var formList = state.PatchMod.FormLists.AddNew("NER");
            foreach(var kywd in state.LoadOrder.PriorityOrder.OnlyEnabled().Keyword().WinningOverrides()) {
                var edid = kywd.EditorID;
                if((edid?.Contains("Clothing") ?? false) || ((edid?.Contains("Armor") ?? false) && (!edid?.Contains("ArmorMaterial") ?? false)) || (edid?.Contains("WeapType") ?? false)) {
                    formList.Items.Add(kywd.FormKey);
                }
            }
            foreach(var ench in state.LoadOrder.PriorityOrder.OnlyEnabled().ObjectEffect().WinningOverrides()) {
                if(ench.EnchantType != ObjectEffect.EnchantTypeEnum.StaffEnchantment) {
                    var onch = ench.DeepCopy();
                    onch.WornRestrictions = formList.FormKey;
                    state.PatchMod.ObjectEffects.Add(onch);
                }
            }
        }
    }
}
