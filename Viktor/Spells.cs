using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace Viktor
{
    public class Spells
    {
        public enum HitChances
        {
            Immobile = 6,
            VeryHigh = 5,
            High = 4,
            Medium = 3,
            Low = 2,
            VeryLow = 1,
            Notarget = 0
        }

        public Spells(SpellSlot Spellslot, SkillshotType Skillshottype, float Range, float Delay, float Radius, bool Collision, float Speed = 4000000, float ExtraRange = 0)
        {
            spellslot = Spellslot;
            delay = Delay;
            radius = Radius;
            speed = Speed;
            range = Range;
            extrarange = ExtraRange;
            collision = Collision;
            skillshottype = Skillshottype;
            MinHitChance = HitChances.VeryLow;
        }
        public SkillshotType skillshottype { get; set; }
        public SpellSlot spellslot { get; set; }
        public float delay { get; set; }
        public float radius { get; set; }
        public float speed { get; set; }
        public float range { get; set; }
        public float extrarange { get; set; }
        public bool collision { get; set; }
        public HitChances MinHitChance { get; set; }
        public SpellDataInst Instance
        {
            get { return ObjectManager.Player.Spellbook.GetSpell(spellslot); }
        }
    }
    public static class CastSpell
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static bool Cast(this Spells spell, Vector3 from, Obj_AI_Base ToTarget, Spells.HitChances minhitchance = Spells.HitChances.VeryLow, bool UseExtendRadiusSSCirle = true)
        {
            if (spell.spellslot.IsReady())
            {
                if (GetHiChance(from, ToTarget, spell) >= spell.MinHitChance && GetHiChance(from, ToTarget, spell) >= minhitchance)
                {
                    Vector3 y = new Vector3(0, 0, 0);
                    Vector3 pos = Predictions.GetPrediction(from, ToTarget, spell, true);
                    if (Player.Distance(pos) <= spell.range && pos != y)
                    {
                        if (spell.collision == false)
                            return Player.Spellbook.CastSpell(spell.spellslot, pos);
                        else
                        {
                            List<Obj_AI_Base> list = Collisions.GetCollision(from, pos, spell);

                            if (list.Count == 0)
                            {
                                Vector3 pos1 = Predictions.GetPrediction(from, ToTarget, spell);
                                return Player.Spellbook.CastSpell(spell.spellslot, pos1);
                            }
                            else return false;
                        }
                    }
                    else if (UseExtendRadiusSSCirle == true && spell.skillshottype == SkillshotType.SkillshotCircle)
                    {
                        Spells x = new Spells(spell.spellslot, spell.skillshottype, spell.range + spell.radius - 20, spell.delay, 20, false, spell.speed);
                        Vector3 y1 = new Vector3(0, 0, 0);
                        Vector3 pos1 = Predictions.GetPrediction(from, ToTarget, x, true);
                        if (Player.Distance(pos1) <= x.range && pos1 != y1)
                        {
                            var pos2 = Player.Position.Extend(pos1, spell.range);
                            return Player.Spellbook.CastSpell(spell.spellslot, pos2);
                        }
                        else return false;
                    }
                    else return false;
                }
                else return false;
            }
            else return false;
        }
        public static bool Cast(this Spells spell, Vector3 from, Vector3 rangecheckfrom, Obj_AI_Base ToTarget, Spells.HitChances minhitchance = Spells.HitChances.VeryLow, bool UseExtendRadiusSSCirle = true)
        {
            if (spell.spellslot.IsReady())
            {
                if (GetHiChance(from, ToTarget, spell) >= spell.MinHitChance && GetHiChance(from, ToTarget, spell) >= minhitchance)
                {
                    Vector3 y = new Vector3(0, 0, 0);
                    Vector3 pos = Predictions.GetPrediction(from, ToTarget, spell, true);
                    if (rangecheckfrom.Distance(pos) <= spell.range && pos != y)
                    {
                        if (spell.collision == false)
                            return Player.Spellbook.CastSpell(spell.spellslot, pos);
                        else
                        {
                            List<Obj_AI_Base> list = Collisions.GetCollision(from, pos, spell);

                            if (list.Count == 0)
                            {
                                Vector3 pos1 = Predictions.GetPrediction(from, ToTarget, spell);
                                return Player.Spellbook.CastSpell(spell.spellslot, pos1);
                            }
                            else return false;
                        }
                    }
                    else if (UseExtendRadiusSSCirle == true && spell.skillshottype == SkillshotType.SkillshotCircle)
                    {
                        Spells x = new Spells(spell.spellslot, spell.skillshottype, spell.range + spell.radius - 20, spell.delay, 20, false, spell.speed);
                        Vector3 y1 = new Vector3(0, 0, 0);
                        Vector3 pos1 = Predictions.GetPrediction(from, ToTarget, x, true);
                        if (rangecheckfrom.Distance(pos1) <= x.range && pos1 != y1)
                        {
                            var pos2 = Player.Position.Extend(pos1, spell.range);
                            return Player.Spellbook.CastSpell(spell.spellslot, pos2);
                        }
                        else return false;
                    }
                    else return false;
                }
                else return false;
            }
            else return false;
        }
        public static bool Cast(this Spells spell, bool DragSpell, Vector3 from, Obj_AI_Base ToTarget, Spells.HitChances minhitchance = Spells.HitChances.VeryLow, bool UseExtendRadiusSSCirle = true)
        {
            if (spell.spellslot.IsReady())
            {
                if (GetHiChance(from, ToTarget, spell) >= spell.MinHitChance && GetHiChance(from, ToTarget, spell) >= minhitchance)
                {
                    Vector3 y = new Vector3(0, 0, 0);
                    Vector3 pos = Predictions.GetPrediction(from, ToTarget, spell, true);
                    if (Player.Distance(from) <= spell.range && from.Distance(pos) <= spell.extrarange && pos != y)
                    {
                        if (spell.collision == false)
                            return Player.Spellbook.CastSpell(spell.spellslot, from, pos);
                        else
                        {
                            List<Obj_AI_Base> list = Collisions.GetCollision(from, pos, spell);
                            if (list.Count == 0)
                            {
                                Vector3 pos1 = Predictions.GetPrediction(from, ToTarget, spell);
                                return Player.Spellbook.CastSpell(spell.spellslot, from, pos1);
                            }
                            else return false;
                        }
                    }

                    else return false;
                }
                else return false;
            }
            else return false;
        }
        public static bool Cast(this Spells spell, Obj_AI_Base ToTarget, Spells.HitChances minhitchance = Spells.HitChances.VeryLow)
        {
            return Cast(spell, Player.Position, ToTarget, minhitchance);
        }
        public static bool Cast(this Spells spell, Vector3 From, Vector3 To)
        {
            if (spell.spellslot.IsReady() && Player.Distance(From) <= spell.range)
                return Player.Spellbook.CastSpell(spell.spellslot, From, To);
            else return false;
        }
        public static bool Cast(this Spells spell, Vector3 Position)
        {
            if (spell.spellslot.IsReady() && Player.Distance(Position) <= spell.range)
                return Player.Spellbook.CastSpell(spell.spellslot, Position);
            else return false;
        }
        public static bool Cast(this Spells spell)
        {
            if (spell.spellslot.IsReady())
                return Player.Spellbook.CastSpell(spell.spellslot);
            else return false;
        }

        public static bool IsReady(this Spells spell, int delay = 0)
        {
            return spell.spellslot.IsReady(delay);
        }
        public static Spells.HitChances GetHiChance(Vector3 From, Obj_AI_Base target, Spells spell)
        {
            if (target == null) return Spells.HitChances.Notarget;
            else
            {
                //Game.PrintChat("1");
                if (target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Stun))
                {
                    //Game.PrintChat("2");
                    float time = GetImmobileTime(target) - Utils.TickCount;
                    if (time / 1000 + (spell.radius + target.BoundingRadius - 20) / target.MoveSpeed >= spell.delay + Game.Ping / 2 / 1000 + From.Distance(target.Position) / spell.speed)
                        return Spells.HitChances.Immobile;
                    else return Spells.HitChances.VeryLow;
                }
                else
                {
                    //Game.PrintChat("3");
                    var predict = Prediction.GetPrediction(target, 100).CastPosition;
                    float y = target.MoveSpeed * 100 / 1000;
                    bool moving = !(target.Distance(predict) < y);
                    var chuot = moving == false ? target.Position : target.Position.Extend(predict, y);
                    if (chuot == target.Position) return Spells.HitChances.VeryLow;
                    else
                    {
                        //Game.PrintChat("4");
                        var cosB = math.CosB(From, target.Position, chuot);
                        var gocB = Math.Acos(cosB) * (180 / Math.PI);
                        if (gocB <= 40) { return Spells.HitChances.VeryHigh; }
                        else if (gocB <= 90) return Spells.HitChances.High;
                        else if (gocB <= 165) return Spells.HitChances.Medium;
                        else return Spells.HitChances.Low;
                    }
                }
            }
        }
        public static float GetImmobileTime(Obj_AI_Base target)
        {
            var buffEndTime = target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Type == BuffType.Knockup || buff.Type == BuffType.Snare || buff.Type == BuffType.Stun)
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
            return buffEndTime;
        }
    }
    public class Target
    {
        public delegate void OnChangedDirection(Obj_AI_Hero sender);
        private static List<targets> list = new List<targets>();
        static Target()
        {
            foreach (var target in HeroManager.AllHeroes)
            {
                list.Add(new targets(target, new Vector3(), new Vector3()));
            }
            Game.OnUpdate += Game_OnUpdate;
        }
        public static event OnChangedDirection OnChangeDirection;

        private static void Game_OnUpdate(EventArgs args)
        {
            foreach (var x in list.Where(i => i.Target.IsValidTarget(float.MaxValue, false)))
            {
                x.Direction1 = x.Direction2;
                x.Direction2 = Prediction.GetPrediction(x.Target, 250).UnitPosition;
            }
            if (OnChangeDirection == null) return;
            foreach (var x in list.Where(x => x.Target.IsValidTarget(float.MaxValue, false) && IsChangingDirection(x)))
            {

                OnChangeDirection(x.Target);
            }
        }

        private static bool IsChangingDirection(targets x)
        {
            if (!x.Direction1.IsValid() || !x.Direction2.IsValid()) return false;
            if (x.Direction1.Distance(x.Target.Position) == 0 || x.Direction2.Distance(x.Target.Position) == 0)
            {
                return !(x.Direction1.Distance(x.Target.Position) == 0 && x.Direction2.Distance(x.Target.Position) == 0);
            }
            else
            {
                float cosX = math.CosB(x.Direction1, x.Target.Position, x.Direction2);
                if (cosX <= -0.98) cosX = (float)-0.98;
                if (cosX >= 0.98) cosX = (float)0.98;
                double gocX = Math.Acos(cosX) * (180 / Math.PI);
                return gocX >= 35;
            }
        }

        private class targets
        {
            public targets(Obj_AI_Hero target, Vector3 direction1, Vector3 direction2)
            {
                Target = target;
                Direction1 = direction1;
                Direction2 = direction2;
            }
            public Obj_AI_Hero Target { get; set; }
            public Vector3 Direction1 { get; set; }
            public Vector3 Direction2 { get; set; }
        }
    }
}
