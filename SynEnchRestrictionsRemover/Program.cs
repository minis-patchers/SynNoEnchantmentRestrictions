using System.Threading.Tasks;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace SynEnchRestrictionsRemover
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "EnchRestricts.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var formList = state.PatchMod.FormLists.AddNew("NER");
            state.LoadOrder.PriorityOrder.Keyword().WinningOverrides().ForEach(kywd =>
            {
                var edid = kywd.EditorID;
                if ((edid?.Contains("Clothing") ?? false) || ((edid?.Contains("Armor") ?? false) && (!edid?.Contains("ArmorMaterial") ?? false)) || (edid?.Contains("WeapType") ?? false))
                {
                    formList.Items.Add(kywd.FormKey);
                }
            });
            state.LoadOrder.PriorityOrder.ObjectEffect().WinningOverrides().ForEach(ench =>
            {
                if (ench.EnchantType != ObjectEffect.EnchantTypeEnum.StaffEnchantment)
                {
                    var onch = state.PatchMod.ObjectEffects.GetOrAddAsOverride(ench);
                    onch.WornRestrictions.SetTo(formList);
                }
            });
        }
    }
}
