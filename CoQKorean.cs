using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Linq;
using XRL;
using XRL.World;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HarmonyLib;

namespace CoQKorean
{
    [HasModSensitiveStaticCache]
    public static class CoQKorean
    {
        private static Dictionary<char, exTextureInfo> koreanCharInfos = new Dictionary<char, exTextureInfo>();
        private static readonly string modPath = Path.GetFullPath(Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "..", "..", "Mods", "CoQKorean"));

        [ModSensitiveCacheInit]
        public static void Init()
        {
            var bundle = AssetBundle.LoadFromFile(Path.Combine(modPath, "coqkorean"));
            if (bundle != null)
            {
                var font = bundle.LoadAsset<TMP_FontAsset>("Assets/coqkorean/D2Coding SDF.asset");
                TMP_Settings.fallbackFontAssets.Add(font);
                Debug.Log($"CoQKorean :: FallbackFont added {font.name}");
            }

            var charSet = File.ReadAllText(Path.Combine(modPath, "Fonts", "charset.txt"));
            for (int i = 0; i < charSet.Length; i++)
            {
                if (!koreanCharInfos.ContainsKey(charSet[i]))
                {
                    var raw = File.ReadAllBytes(Path.Combine(modPath, "Fonts", $"{i}.png"));
                    var texture = new Texture2D(1, 1);
                    texture.LoadImage(raw);

                    koreanCharInfos.Add(charSet[i], exTextureInfo.Create(texture));
                }
            }

            Debug.Log("CoQKorean :: Done!");
        }

        [HarmonyPatch(typeof(GameManager))]
        class GameManagerPatch
        {
            [HarmonyTranspiler]
            [HarmonyPatch("OnUpdate")]
            static IEnumerable<CodeInstruction> OnUpdateTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var codes = new List<CodeInstruction>(instructions);
                var charInfos = AccessTools.Field(typeof(GameManager), nameof(GameManager.CharInfos));
                var getCharInfo = SymbolExtensions.GetMethodInfo((char c) => GetCharInfo(c));
                var isKoreanInfo = SymbolExtensions.GetMethodInfo((char c) => IsKorean(c));
                var label = il.DefineLabel();
                int insertionIndex = -1;
                var insertionInstructions = new List<CodeInstruction>();
                var insertionLdloc = new CodeInstruction(OpCodes.Ldloc_S, null);

                for (int i = 0; i < codes.Count - 2; i++)
                {
                    if (codes[i].LoadsField(charInfos))
                    {
                        Debug.Log($"CoQKorean :: CharInfos loads");
                        codes[i + 2].opcode = OpCodes.Call;
                        codes[i + 2].operand = getCharInfo;
                    }
                    else if (codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Ldc_I4_0)
                    {
                        Debug.Log($"CoQKorean :: insertionIndex found");
                        insertionIndex = i;
                        insertionLdloc.operand = codes[i].operand;
                    }
                    else if (codes[i].opcode == OpCodes.Ldc_I4_S && (SByte)codes[i].operand == 0x20 && codes[i + 1].opcode == OpCodes.Stloc_S)
                    {
                        Debug.Log($"CoQKorean :: Label found");
                        codes[i + 2].labels.Add(label);
                    }
                }

                insertionInstructions.Add(new CodeInstruction(insertionLdloc));
                insertionInstructions.Add(new CodeInstruction(OpCodes.Call, isKoreanInfo));
                insertionInstructions.Add(new CodeInstruction(OpCodes.Brtrue_S, label));

                if (insertionIndex != -1)
                {
                    Debug.Log($"CoQKorean :: insertionInstructions insert");
                    codes.InsertRange(insertionIndex, insertionInstructions);
                }

                for (int i = insertionIndex; i < insertionIndex + 11; i++)
                    Debug.Log(codes[i].opcode);

                return codes.AsEnumerable();
            }

            static exTextureInfo GetCharInfo(char c)
            {
                if (c < 256)
                    return GameManager.Instance.CharInfos[c];
                else
                    return koreanCharInfos[c];
            }

            static bool IsKorean(char c)
            {
                return (c >= 0xAC00 && c <= 0xD7A3) || (c >= 0x3131 && c <= 0x318E);
            }
        }

        [HarmonyPatch(typeof(XRL.World.Calendar))]
        class CalendarPatch
        {
            [HarmonyTranspiler]
            [HarmonyPatch("getMonth")]
            static IEnumerable<CodeInstruction> getMonthTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr)
                    {
                        switch (codes[i].operand as string)
                        {
                            case "Nivvun Ut":
                                codes[i].operand = "니분 우트";
                                break;

                            case "Iyur Ut":
                                codes[i].operand = "이유르 우트";
                                break;

                            case "Simmun Ut":
                                codes[i].operand = "시문 우트";
                                break;

                            case "Tuum Ut":
                                codes[i].operand = "툼 우트";
                                break;

                            case "Ubu Ut":
                                codes[i].operand = "우부 우트";
                                break;

                            case "Uulu Ut":
                                codes[i].operand = "울루 우트";
                                break;

                            case "Ut yara Ux":
                                codes[i].operand = "우트 야라 욱스";
                                break;

                            case "Tishru i Ux":
                                codes[i].operand = "티쉬루 이 욱스";
                                break;

                            case "Tishru ii Ux":
                                codes[i].operand = "티쉬루 이이 욱스";
                                break;

                            case "Kisu Ux":
                                codes[i].operand = "키수 욱스";
                                break;

                            case "Tebet Ux":
                                codes[i].operand = "테벳 욱스";
                                break;

                            case "Shwut Ux":
                                codes[i].operand = "쉬웟 욱스";
                                break;

                            case "Uru Ux":
                                codes[i].operand = "우루 욱스";
                                break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }

            [HarmonyTranspiler]
            [HarmonyPatch("getDay")]
            static IEnumerable<CodeInstruction> getDayTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr)
                    {
                        codes[i].operand = (codes[i].operand as string).Replace("st", "일")
                                                                       .Replace("nd", "일")
                                                                       .Replace("rd", "일")
                                                                       .Replace("th", "일")
                                                                       .Replace("Ides", "가운데 날");
                    }
                }

                return codes.AsEnumerable();
            }

            [HarmonyTranspiler]
            [HarmonyPatch("getTime")]
            [HarmonyPatch(new Type[] { typeof(int) })]
            static IEnumerable<CodeInstruction> getTimeTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr)
                    {
                        switch (codes[i].operand as string)
                        {
                            case "Beetle Moon Zenith":
                                codes[i].operand = "정점의 딱정벌레 달";
                                break;

                            case "Waning Beetle Moon":
                                codes[i].operand = "이지러지는 딱정벌레 달";
                                break;

                            case "The Shallows":
                                codes[i].operand = "옅은시";
                                break;

                            case "Harvest Dawn":
                                codes[i].operand = "수확의 새벽";
                                break;

                            case "Waxing Salt Sun":
                                codes[i].operand = "차오르는 소금 해";
                                break;

                            case "High Salt Sun":
                                codes[i].operand = "높다란 소금 해";
                                break;

                            case "Waning Salt Sun":
                                codes[i].operand = "이지러지는 소금 해";
                                break;

                            case "Hindsun":
                                codes[i].operand = "무른 해";
                                break;

                            case "Jeweled Dusk":
                                codes[i].operand = "보석 황혼";
                                break;

                            case "Waxing Beetle Moon":
                                codes[i].operand = "차오르는 딱정벌레 달";
                                break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Qud.UI.PlayerStatusBar))]
        class PlayerStatusBarPatch
        {
            [HarmonyTranspiler]
            [HarmonyPatch("BeginEndTurn")]
            static IEnumerable<CodeInstruction> BeginEndTurnTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var dayInfo = SymbolExtensions.GetMethodInfo(() => Calendar.getDay(-1));
                var monthInfo = SymbolExtensions.GetMethodInfo(() => Calendar.getMonth(-1));
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call)
                    {
                        if (codes[i].OperandIs(dayInfo))
                            codes[i].operand = monthInfo;
                        else if (codes[i].OperandIs(monthInfo))
                            codes[i].operand = dayInfo;
                    }
                    else if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand as string == " of ")
                        codes[i].operand = "의 ";
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(XRL.World.Parts.OpeningStory))]
        class OpeningStoryPatch
        {
            [HarmonyTranspiler]
            [HarmonyPatch("HandleEvent")]
            [HarmonyPatch(new Type[] { typeof(XRL.World.BeforeTakeActionEvent) })]
            static IEnumerable<CodeInstruction> HandleEventTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr)
                    {
                        switch (codes[i].operand as string)
                        {
                            case "\n\n<Press space, then press F1 for help.>":
                                codes[i].operand = "\n\n<스페이스를 누르세요. 도움말을 여시려면 이후 F1을 누르세요.>";
                                break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(XRL.World.ConversationChoice))]
        class ConversationChoicePatch
        {
            [HarmonyTranspiler]
            [HarmonyPatch("GetDisplayText")]
            static IEnumerable<CodeInstruction> GetDisplayTextTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr)
                    {
                        switch (codes[i].operand as string)
                        {
                            case " {{W|[Complete Quest Step]}}":
                                codes[i].operand = " {{W|[퀘스트 단계 완료]}}";
                                break;

                            case " {{W|[Accept Quest]}}":
                                codes[i].operand = " {{W|[퀘스트 수락]}}";
                                break;

                            case " {{W|[Accept Quest - level-based reward]}}":
                                codes[i].operand = " {{W|[퀘스트 수락 - 레벨 비례 보상]}}";
                                break;

                            case " {{K|[End]}}":
                                codes[i].operand = " {{K|[종료]}}";
                                break;

                            case " {{R|[Fight]}}":
                                codes[i].operand = " {{R|[전투]}}";
                                break;

                            case " {{g|[select limb to infect]}}":
                                codes[i].operand = " {{g|[감염시킬 사지 선택]}}";
                                break;

                            case " {{g|[Give Artifact]}}":
                                codes[i].operand = " {{g|[아티팩트 주기]}}";
                                break;

                            case " {{g|[Give Book]}}":
                                codes[i].operand = " {{g|[책 주기]}}";
                                break;

                            case " {{M|[lesser victory]}}":
                                codes[i].operand = " {{M|[lesser victory]}}";
                                break;

                            case " {{g|[Share a secret from Resheph's life]}}":
                                codes[i].operand = " {{g|[레셰프의 삶의 비밀 공유하기]}}";
                                break;

                            case " {{g|[Begin Dance]}}":
                                codes[i].operand = " {{g|[춤추기]}}";
                                break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(XRL.World.Conversations.Parts.QuestHandler))]
        class QuestHandlerPatch
        {
            [HarmonyTranspiler]
            [HarmonyPatch("HandleEvent")]
            [HarmonyPatch(new Type[] { typeof(XRL.World.Conversations.GetChoiceTagEvent) })]
            static IEnumerable<CodeInstruction> HandleEventTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr)
                    {
                        switch (codes[i].operand as string)
                        {
                            case "{{W|[Accept Quest - level-based reward]}}":
                                codes[i].operand = "{{W|[퀘스트 수락 - 레벨 비례 보상]}}";
                                break;

                            case "{{W|[Accept Quest]}}":
                                codes[i].operand = "{{W|[퀘스트 수락]}}";
                                break;

                            case "{{W|[Complete Quest Step]}}":
                                codes[i].operand = "{{W|[퀘스트 단계 완료]}}";
                                break;

                            case "{{W|[Complete Quest - level-based reward]}}":
                                codes[i].operand = "{{W|[퀘스트 완료 - 레벨 비례 보상]}}";
                                break;

                            case "{{W|[Complete Quest]}}":
                                codes[i].operand = "{{W|[퀘스트 완료]}}";
                                break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(XRL.XRLGame))]
        class XRLGamePatch
        {
            [HarmonyTranspiler]
            [HarmonyPatch("StartQuest")]
            [HarmonyPatch(new Type[] { typeof(XRL.World.Quest) })]
            static IEnumerable<CodeInstruction> StartQuest1Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr)
                    {
                        if (codes[i].operand as string == "You have received a new quest, ")
                            codes[i].operand = "새로운 퀘스트를 받았습니다 - ";
                    }
                }

                return codes.AsEnumerable();
            }

            [HarmonyTranspiler]
            [HarmonyPatch("StartQuest")]
            [HarmonyPatch(new Type[] { typeof(string) })]
            static IEnumerable<CodeInstruction> StartQuest2Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr)
                    {
                        if (codes[i].operand as string == "You have received a new quest, ")
                            codes[i].operand = "새로운 퀘스트를 받았습니다 - ";
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(XRL.UI.JournalScreen))]
        class JournalScreenPatch
        {
            [HarmonyTranspiler]
            [HarmonyPatch("Show")]
            static IEnumerable<CodeInstruction> ShowTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr)
                    {
                        if (codes[i].operand as string == "[ {{W|Journal}} ]")
                            codes[i].operand = "[ {{W|일지}} ]";
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}