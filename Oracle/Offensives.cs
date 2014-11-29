﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using OC = Oracle.Program;

namespace Oracle
{
    internal static class Offensives
    {
        private static int casttime;
        private static Menu mainmenu, menuconfig;
        private static Obj_AI_Hero currenttarget;
        private static SpellSlot manamuneslot;
        private static readonly Obj_AI_Hero me = ObjectManager.Player;

        public static void Initialize(Menu root)
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            mainmenu = new Menu("Offensives", "omenu");
            menuconfig = new Menu("Offensive Config", "oconfig");

            foreach (Obj_AI_Hero x in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                menuconfig.AddItem(new MenuItem("ouseon" + x.SkinName, "Use for " + x.SkinName)).SetValue(true);
            mainmenu.AddSubMenu(menuconfig);


            if (Game.MapId == GameMapId.CrystalScar || Game.MapId == GameMapId.HowlingAbyss)
                CreateMenuItem("Entropy", "Entropy", 90, 30);

            if (Game.MapId == GameMapId.HowlingAbyss)
                CreateMenuItem("Guardians Horn", "Guardians", 90, 30);

            if (Game.MapId == GameMapId.CrystalScar || Game.MapId == GameMapId.TwistedTreeline)            
                CreateMenuItem("Blackfire Torch", "Torch", 100, 30);

            CreateMenuItem("Muramana", "Muramana", 90, 30, true);
            CreateMenuItem("Tiamat/Hydra", "Hydra", 90, 30);
            CreateMenuItem("Deathfire Grasp", "DFG", 100, 30);
            CreateMenuItem("Youmuu's Ghostblade", "Youmuus", 90, 30);
            CreateMenuItem("Bilgewater's Cutlass", "Cutlass", 90, 30);
            CreateMenuItem("Hextech Gunblade", "Hextech", 90, 30);
            CreateMenuItem("Blade of the Ruined King", "Botrk", 70, 70);
            CreateMenuItem("Frost Queen's Claim", "Frostclaim", 90, 30);
            CreateMenuItem("Sword of Divine", "Divine", 90, 30);
            
            root.AddSubMenu(mainmenu);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            currenttarget = SimpleTs.GetTarget(900f, SimpleTs.DamageType.Physical);
            if (currenttarget != null)
            {
                if (OC.Origin.Item("ComboKey").GetValue<KeyBind>().Active)
                {
                    if (Game.MapId == GameMapId.HowlingAbyss)
                        UseItem("Guardians", 2051, 450f);

                    if (Game.MapId == GameMapId.CrystalScar || Game.MapId == GameMapId.HowlingAbyss)
                        UseItem("Entropy", 3184, 450f, true);

                    if (Game.MapId == GameMapId.CrystalScar || Game.MapId == GameMapId.TwistedTreeline)
                        UseItem("Torch", 3188, 750f, true);

                    UseItem("Frostclaim", 3092, 850f, true);
                    UseItem("Youmuus", 3142, 650f);
                    UseItem("Hydra", 3077, 250f);
                    UseItem("Hydra", 3074, 250f);
                    UseItem("Hextech", 3146, 700f, true);
                    UseItem("Cutlass", 3144, 450f, true);
                    UseItem("Botrk", 3153, 450f, true);
                    UseItem("Divine", 3131, 650f);
                    UseItem("DFG", 3128, 750f, true);
                }
            }

            manamuneslot = me.GetSpellSlot("Muramana");
            if (mainmenu.Item("useMuramana").GetValue<bool>())
            {
                if (manamuneslot != SpellSlot.Unknown)
                {
                    if (me.HasBuff("Muramana") && casttime + 400 < Environment.TickCount)
                        me.Spellbook.CastSpell(manamuneslot);
                }
            }
        }

        private static void UseItem(string name, int itemId, float range, bool targeted = false)
        {
            var damage = 0f;
            if (!Items.HasItem(itemId) || !Items.CanUseItem(itemId))
                return;

            if (mainmenu.Item("use" + name).GetValue<bool>())
            {
                if (itemId == 3128 || itemId == 3188)
                    damage = OC.DamageCheck(me, currenttarget);

                if (currenttarget.Distance(me.Position) <= range)
                {
                    var eHealthPercent = (int) ((currenttarget.Health/currenttarget.MaxHealth)*100);
                    var aHealthPercent = (int) ((me.Health/currenttarget.MaxHealth)*100);

                    if (eHealthPercent <= mainmenu.Item("use" + name + "Pct").GetValue<Slider>().Value &&
                        mainmenu.Item("ouseon" + currenttarget.SkinName).GetValue<bool>())
                    {
                        if (targeted && itemId == 3092)
                        {
                            var pi = new PredictionInput
                            {
                                Aoe = true,
                                Collision = false,
                                Delay = 0.0f,
                                From = me.Position,
                                Radius = 250f,
                                Range = 850f,
                                Speed = 1500f,
                                Unit = currenttarget,
                                Type = SkillshotType.SkillshotCircle
                            };

                            var po = Prediction.GetPrediction(pi);
                            if (po.Hitchance >= HitChance.Medium)
                                Items.UseItem(itemId, po.CastPosition);
                        }

                        else if (targeted)
                        {
                            if ((itemId == 3128 || itemId == 3188) && damage < currenttarget.Health)
                                return;

                            Items.UseItem(itemId, currenttarget);
                        }

                        else
                        {
                            Items.UseItem(itemId);
                        }
                    }

                    else if (aHealthPercent <= mainmenu.Item("use" + name + "Me").GetValue<Slider>().Value &&
                             mainmenu.Item("ouseon" + currenttarget.SkinName).GetValue<bool>())
                    {
                        if (targeted)
                            Items.UseItem(itemId, currenttarget);
                        else
                            Items.UseItem(itemId);
                    }
                }
            }
        }

        private static void CreateMenuItem(string displayname, string name, int evalue, int avalue, bool usemana = false)
        {
            var menuName = new Menu(displayname, name.ToLower());
            menuName.AddItem(new MenuItem("use" + name, "Use " + name)).SetValue(true);
            menuName.AddItem(new MenuItem("use" + name + "Pct", "Use on enemy HP %")).SetValue(new Slider(evalue));
            if (!usemana)
                menuName.AddItem(new MenuItem("use" + name + "Me", "Use on my HP %")).SetValue(new Slider(avalue));
            if (usemana)
                menuName.AddItem(new MenuItem("use" + name + "Mana", "Minimum mana % to use")).SetValue(new Slider(35));
            mainmenu.AddSubMenu(menuName);
        }

        private static void Obj_AI_Base_OnPlayAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (!mainmenu.Item("useMuramana").GetValue<bool>())
                return;

            if (manamuneslot != SpellSlot.Unknown)
            {
                if (currenttarget == null)
                    return;

                if (OC.Origin.Item("ComboKey").GetValue<KeyBind>().Active)
                {
                    var manaPercent = (int) ((me.Mana/me.MaxMana)*100);
                    if (args.Animation.Contains("Attack"))
                    {
                        if (me.Spellbook.CanUseSpell(manamuneslot) != SpellState.Unknown)
                        {
                            if (!me.HasBuff("Muramana") && mainmenu.Item("ouseon" + currenttarget.SkinName).GetValue<bool>())
                                if (manaPercent > mainmenu.Item("useMuramanaMana").GetValue<Slider>().Value)
                                    me.Spellbook.CastSpell(manamuneslot);
                        }
                    }
                }
            }
        }

        private static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe)
                return;

            if (mainmenu.Item("useMuramana").GetValue<bool>())
            {
                if (manamuneslot != SpellSlot.Unknown)
                {
                    Utility.DelayAction.Add(1000, delegate
                    {
                        if (me.HasBuff("Muramana"))
                            me.Spellbook.CastSpell(manamuneslot);
                    });
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (manamuneslot == SpellSlot.Unknown)
                return;
            if (!mainmenu.Item("useMuramana").GetValue<bool>())
                return;

            if (sender.IsMe && OC.Origin.Item("ComboKey").GetValue<KeyBind>().Active)
            {
                var manaPercent = (int)((me.Mana / me.MaxMana) * 100);
                if (me.Spellbook.CanUseSpell(manamuneslot) != SpellState.Ready)
                    return;

                var myslot = me.GetSpellSlot(args.SData.Name);
                foreach (var spell in OracleLib.Database.Where(x => x.Name == sender.SkinName))
                {
                    if (myslot == spell.Slot && spell.OnHit && me.HasBuff("Muramana"))
                    {
                        if (manaPercent <= mainmenu.Item("useMuramanaMana").GetValue<Slider>().Value)
                            return;

                        me.Spellbook.CastSpell(manamuneslot);
                        casttime = Environment.TickCount;                      
                    }
                }
            }
        }
    }
}