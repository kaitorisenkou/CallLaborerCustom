using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using System.Reflection.Emit;
using System.Reflection;
using RimWorld;

namespace CallLaborerCustom {

    [StaticConstructorOnStartup]
    public class CallLaborerCustom {
        static CallLaborerCustom() {
            Log.Message("[CallLaborerCustom] Now active");
            var harmony = new Harmony("kaitorisenkou.CallLaborerCustom");
            harmony.Patch(
                AccessTools.Method(typeof(RoyalTitlePermitWorker_CallLaborers), "CallLaborers", null, null),
                null,
                null,
                new HarmonyMethod(typeof(CallLaborerCustom), nameof(Patch_CallLaborers), null),
                null
                );
            Log.Message("[CallLaborerCustom] Harmony patch complete!");
        }


        static IEnumerable<CodeInstruction> Patch_CallLaborers(IEnumerable<CodeInstruction> instructions) {
            var instructionList = instructions.ToList();
            int patchCount = 0;
            FieldInfo targetInfo = AccessTools.Field(typeof(QuestScriptDefOf), nameof(QuestScriptDefOf.Permit_CallLaborers));
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Ldsfld && (FieldInfo)instructionList[i].operand == targetInfo) {
                    instructionList.RemoveAt(i);
                    instructionList.InsertRange(i, new CodeInstruction[]{
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld,AccessTools.Field(typeof(RoyalTitlePermitWorker), nameof(RoyalTitlePermitWorker.def))),
                        new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(CallLaborerCustom), nameof(CallLaborerCustom.GetQuestScriptDefForLaborers)))
                    });
                    patchCount++;
                    break;
                }
            }
            if (patchCount < 1) {
                Log.Error("[CallLaborerCustom] Patch_CallLaborers seems failed!");
            }
            return instructionList;
        }
        static public QuestScriptDef GetQuestScriptDefForLaborers(RoyalTitlePermitDef def) {
            QuestScriptDef result = null;
            var ext = def.GetModExtension<ModExtension_CallLaborerCustom>();
            if (ext != null)
                result = ext.questScriptDef;
            return result ?? QuestScriptDefOf.Permit_CallLaborers;
        }
    }
}
