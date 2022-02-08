using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

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
                if (!edid.IsNullOrEmpty())
                {
                    if (Regex.IsMatch(edid, "^Clothing.*") || Regex.IsMatch(edid, "^Armor.*") || Regex.IsMatch(edid, "^WeapType.*"))
                    {
                        formList.Items.Add(kywd);
                    }
                }
            });
            state.LoadOrder.PriorityOrder.ObjectEffect().WinningOverrides().ForEach(ench =>
            {
                if (ench.EnchantType != ObjectEffect.EnchantTypeEnum.StaffEnchantment && !ench.WornRestrictions.IsNull)
                {
                    var onch = state.PatchMod.ObjectEffects.GetOrAddAsOverride(ench);
                    onch.WornRestrictions.SetTo(formList);
                }
            });
        }
    }
}
