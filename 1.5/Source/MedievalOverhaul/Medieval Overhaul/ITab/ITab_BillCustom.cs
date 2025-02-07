using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MedievalOverhaul
{
    public class ITab_BillCustom : ITab
    {
        private float viewHeight = 1000f;

        private Vector2 scrollPosition;

        private Bill mouseoverBill;

        private static readonly Vector2 WinSize = new Vector2(420f, 480f);

        [TweakValue("Interface", 0f, 128f)]
        private static float PasteX = 48f;

        [TweakValue("Interface", 0f, 128f)]
        private static float PasteY = 3f;

        [TweakValue("Interface", 0f, 32f)]
        private static float PasteSize = 24f;

        protected Building_WorkTableCustom SelTable => (Building_WorkTableCustom)base.SelThing;

        public ITab_BillCustom()
        {
            size = WinSize;
            labelKey = "TabBills";
            tutorTag = "Bills";
        }

        public override void FillTab()
        {
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BillsTab, KnowledgeAmount.FrameDisplayed);
            Rect rect2 = new Rect(WinSize.x - PasteX, PasteY, PasteSize, PasteSize);
            if (BillUtility.Clipboard != null)
            {
                if (!SelTable.def.AllRecipes.Contains(BillUtility.Clipboard.recipe) || !BillUtility.Clipboard.recipe.AvailableNow || !BillUtility.Clipboard.recipe.AvailableOnNow(SelTable))
                {
                    GUI.color = Color.gray;
                    Widgets.DrawTextureFitted(rect2, TexButton.Paste, 1f);
                    GUI.color = Color.white;
                    if (Mouse.IsOver(rect2))
                    {
                        TooltipHandler.TipRegion(rect2, "ClipboardBillNotAvailableHere".Translate() + ": " + BillUtility.Clipboard.LabelCap);
                    }
                }
                else if (SelTable.billStack.Count >= 15)
                {
                    GUI.color = Color.gray;
                    Widgets.DrawTextureFitted(rect2, TexButton.Paste, 1f);
                    GUI.color = Color.white;
                    if (Mouse.IsOver(rect2))
                    {
                        TooltipHandler.TipRegion(rect2, "PasteBillTip".Translate() + " (" + "PasteBillTip_LimitReached".Translate() + "): " + BillUtility.Clipboard.LabelCap);
                    }
                }
                else
                {
                    if (Widgets.ButtonImageFitted(rect2, TexButton.Paste, Color.white))
                    {
                        Bill bill = BillUtility.Clipboard.Clone();
                        bill.InitializeAfterClone();
                        SelTable.billStack.AddBill(bill);
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                    if (Mouse.IsOver(rect2))
                    {
                        TooltipHandler.TipRegion(rect2, "PasteBillTip".Translate() + ": " + BillUtility.Clipboard.LabelCap);
                    }
                }
            }
            Rect rect3 = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
            mouseoverBill = SelTable.billStack.DoListing(rect3, OptionsMaker, ref scrollPosition, ref viewHeight);
            List<FloatMenuOption> OptionsMaker()
            {
                List<FloatMenuOption> opts = new List<FloatMenuOption>();
                for (int i = 0; i < SelTable.def.AllRecipes.Count; i++)
                {
                    RecipeDef recipe;
                    if (SelTable.def.AllRecipes[i].AvailableNow && SelTable.def.AllRecipes[i].AvailableOnNow(SelTable))
                    {
                        recipe = SelTable.def.AllRecipes[i];
                        Add(null);
                        foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
                        {
                            foreach (Precept_Building cachedPossibleBuilding in allIdeo.cachedPossibleBuildings)
                            {
                                if (cachedPossibleBuilding.ThingDef == recipe.ProducedThingDef)
                                {
                                    Add(cachedPossibleBuilding);
                                }
                            }
                        }
                    }
                    void Add(Precept_ThingStyle precept)
                    {
                        string label = ((precept != null) ? "RecipeMake".Translate(precept.LabelCap).CapitalizeFirst() : recipe.LabelCap);
                        opts.Add(new FloatMenuOption(label, delegate
                        {
                            if (ModsConfig.BiotechActive && recipe.mechanitorOnlyRecipe && !SelTable.Map.mapPawns.FreeColonists.Any(MechanitorUtility.IsMechanitor))
                            {
                                Find.WindowStack.Add(new Dialog_MessageBox("RecipeRequiresMechanitor".Translate(recipe.LabelCap)));
                            }
                            else if (!SelTable.Map.mapPawns.FreeColonists.Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
                            {
                                Bill.CreateNoPawnsWithSkillDialog(recipe);
                            }
                            Bill bill2 = recipe.MakeNewBill(precept);
                            SelTable.billStack.AddBill(bill2);
                            if (recipe.conceptLearned != null)
                            {
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
                            }
                            if (TutorSystem.TutorialMode)
                            {
                                TutorSystem.Notify_Event("AddBill-" + recipe.LabelCap.Resolve());
                            }
                        }, itemIcon: recipe.UIIcon, shownItemForIcon: recipe.UIIconThing, thingStyle: null, forceBasicStyle: false, priority: MenuOptionPriority.Default, mouseoverGuiAction: delegate (Rect rect)
                        {
                            BillUtility.DoBillInfoWindow(i, label, rect, recipe);
                        }, revalidateClickTarget: null, extraPartWidth: 29f, extraPartOnGUI: (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, recipe, precept), revalidateWorldClickTarget: null, playSelectionSound: true, orderInPriority: -recipe.displayPriority));
                    }
                }
                if (!opts.Any())
                {
                    opts.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
                }
                return opts;
            }
        }

        public override void TabUpdate()
        {
            if (mouseoverBill != null)
            {
                mouseoverBill.TryDrawIngredientSearchRadiusOnMap(SelTable.Position);
                mouseoverBill = null;
            }
        }
    }

}