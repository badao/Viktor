using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace Viktor
{
    static class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell _q, _w, _e, _r;
        private static Spells E;
        private static Menu _menu;
        private static GameObject ViktorR = null;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Viktor")
                return;

            _q = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W,700);
            _e = new Spell(SpellSlot.E);
            _r = new Spell(SpellSlot.R,700);
            _r.SetSkillshot(0.25f, 1,float.MaxValue,false,SkillshotType.SkillshotCircle);
            _w.SetSkillshot(0.25f, 325, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E = new Spells(SpellSlot.E, SkillshotType.SkillshotLine, 520, (float)0.25, 40, false, 780, 500);
            //R = new Spells(SpellSlot.R, SkillshotType.SkillshotCircle, 700, 0.25f, 325 / 2, false);

            _menu = new Menu(Player.ChampionName, Player.ChampionName, true);
            Menu orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);
            Menu ts = _menu.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            Menu spellMenu = _menu.AddSubMenu(new Menu("Spells", "Spells"));
            Menu Harass = spellMenu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu Combo = spellMenu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu Focus = spellMenu.AddSubMenu(new Menu("Focus Selected", "Focus Selected"));
            Menu KS = spellMenu.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            Harass.AddItem(new MenuItem("Use Q Harass", "Use Q Harass").SetValue(true));
            Harass.AddItem(new MenuItem("Use E Harass", "Use E Harass").SetValue(true));
            Combo.AddItem(new MenuItem("Use Q Combo", "Use Q Combo").SetValue(true));
            Combo.AddItem(new MenuItem("Use E Combo", "Use E Combo").SetValue(true));
            Combo.AddItem(new MenuItem("Use W Combo", "Use W Combo").SetValue(true));
            Combo.AddItem(new MenuItem("Use R Burst Selected", "Use R Combo").SetValue(true));
            Focus.AddItem(new MenuItem("force focus selected", "force focus selected").SetValue(false));
            Focus.AddItem(new MenuItem("if selected in :", "if selected in :").SetValue(new Slider(1000, 1000, 1500)));
            KS.AddItem(new MenuItem("Use Q KillSteal", "Use Q KillSteal").SetValue(true));
            KS.AddItem(new MenuItem("Use E KillSteal", "Use E KillSteal").SetValue(true));
            KS.AddItem(new MenuItem("Use R KillSteal", "Use R KillSteal").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use R Follow", "Use R Follow").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use W GapCloser", "Use W anti gap").SetValue(true));

            _menu.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += Create;
            GameObject.OnDelete += Delete;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.PrintChat("Welcome to ViktorWorld");
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_menu.Item("Use W GapCloser").GetValue<bool>() && _w.IsReady() && gapcloser.Sender.IsValidTarget(_w.Range))
            {
                var pos = gapcloser.End;
                if (Player.Distance(pos) <= _w.Range)
                    _w.Cast(pos);
            }
        }
        private static void Create(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Viktor_Base_R_Droid.troy"))
            {
                ViktorR = sender;
            }
        }
        private static void Delete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Viktor_Base_R_Droid.troy"))
            {
                ViktorR = null;
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Selected()))
            {
                if (_q.IsReady() && Player.Mana >= _q.Instance.ManaCost)
                {
                    _orbwalker.SetAttack(false);
                }
                if (!_q.IsReady() || _q.IsReady() && Player.Mana < _q.Instance.ManaCost)
                {
                    _orbwalker.SetAttack(true);
                }
            }
            else
            {
                _orbwalker.SetAttack(true);
            }

            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (_menu.Item("Use Q Combo").GetValue<bool>())
                {
                    UseQ();
                }
                if (_menu.Item("Use E Combo").GetValue<bool>())
                {
                    UseE();
                }
                if (_menu.Item("Use W Combo").GetValue<bool>())
                {
                    UseW();
                }
                if (_menu.Item("Use R Burst Selected").GetValue<bool>())
                {
                    UseR();
                }
            }
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (_menu.Item("Use Q Harass").GetValue<bool>())
                    UseQ();
                if (_menu.Item("Use E Harass").GetValue<bool>())
                    UseE();
            }
            ViktorRMove();
            killsteal();
        }

        private static void UseW()
        {
            var target = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
            if ( target.IsValidTarget() && !target.IsZombie && _w.IsReady())
            {
                var pos = Prediction.GetPrediction(target, 0.25f).UnitPosition;
                if (Player.Distance(pos) <= _w.Range)
                {
                    _w.Cast(pos);
                }
            }
        }
        private static void ViktorRMove()
        {
            if (_menu.Item("Use R Follow").GetValue<bool>() && ViktorR != null && _r.IsReady())
            {
                var target = ViktorR.Position.GetEnemiesInRange(2000).Where(t => t.IsValidTarget() && !t.IsZombie).OrderByDescending(t => 1 - t.Distance(ViktorR.Position)).FirstOrDefault();
                if (target.Distance(ViktorR.Position) >= 50)
                {
                    Vector3 x = Prediction.GetPrediction(target,0.5f).UnitPosition;
                    _r.Cast(x);
                }
            }
        }
        private static bool Selected()
        {
            if (!_menu.Item("force focus selected").GetValue<bool>())
            {
                return false;
            }
            else
            {
                var target = TargetSelector.GetSelectedTarget();
                float a = _menu.Item("if selected in :").GetValue<Slider>().Value;
                if (target == null || target.IsDead || target.IsZombie)
                {
                    return false;
                }
                return !(Player.Distance(target.Position) > a);
            }
        }

        private static Obj_AI_Base Gettarget(float range)
        {
            return Selected() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);
        }

        private static void UseQ()
        {
            var target = Gettarget(600);
            if (target != null && target.IsValidTarget() && !target.IsZombie && _q.IsReady())
                _q.Cast(target);
        }

        private static void UseR()
        {
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && Player.Distance(target.Position) <= 950 && target.IsValidTarget() && !target.IsZombie && _r.IsReady() && _r.Instance.Name == "ViktorChaosStorm")
                {
                    CastR(target);
                }
            }
            {
                var target = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
                if (target != null && target.IsValidTarget() && !target.IsZombie && _r.IsReady() && _r.Instance.Name == "ViktorChaosStorm" )
                {
                    if (target.Health <= _r.GetDamage(target)*1.7)
                    {
                        _r.Cast(target);
                    }
                    _r.CastIfWillHit(target, 2);
                }
            }
        }

        private static void CastR(Obj_AI_Base target)
        {

            _r.Cast(target);
        }

        private static void UseE()
        {
            var target = Gettarget(1025);
            if (target != null && target.IsValidTarget() && !target.IsZombie && _e.IsReady())
            {

                Vector3 x = Player.Distance(target.Position) >= 525 ? Player.Position.Extend(target.Position, 525) : target.Position;
                E.Cast(true, x, target);
            }
        }
        public static void killsteal()
        {
            if (_q.IsReady() && _menu.Item("Use Q KillSteal").GetValue<bool>() && !Player.IsWindingUp)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget(600)))
                {
                    var dmg = Dame(hero, SpellSlot.Q);
                    if (hero != null && hero.IsValidTarget() && !hero.IsZombie && dmg > hero.Health) { _q.Cast(hero); }
                }
            }
            if (_e.IsReady() && _menu.Item("Use E KillSteal").GetValue<bool>() && !Player.IsWindingUp)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget(1025)))
                {
                    var dmg = Dame(hero, SpellSlot.E);
                    if (hero != null && hero.IsValidTarget() && !hero.IsZombie && dmg > hero.Health)
                    {
                        Vector3 x = Player.Distance(hero.Position) >= 525 ? Player.Position.Extend(hero.Position, 525) : hero.Position;
                        E.Cast(true, x, hero);
                    }
                }
            }

            if (_r.IsReady() && _menu.Item("Use R KillSteal").GetValue<bool>() && !Player.IsWindingUp)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget(860)))
                {
                    var dmgR = Dame(hero, SpellSlot.R);
                    var dmgE = Dame(hero, SpellSlot.E);
                    var dmgQ = Dame(hero, SpellSlot.Q);
                    if (hero != null && hero.IsValidTarget() && !hero.IsZombie)
                    {
                        if (dmgE > hero.Health && dmgR > hero.Health)
                        {
                            if (!E.IsReady())
                                _r.Cast(hero);
                        }
                        else if (dmgQ > hero.Health && dmgR > hero.Health && Player.Distance(hero.Position) <= 600)
                        {
                            if (!_q.IsReady() && !E.IsReady())
                                _r.Cast(hero);
                        }
                        else if (dmgR > hero.Health) { _r.Cast(hero); }
                    }
                }
            }

        }
        public static double Dame(Obj_AI_Base target, SpellSlot x)
        {
            if (target != null) { return Player.GetSpellDamage(target, x); } else return 0;
        }

    }
}
