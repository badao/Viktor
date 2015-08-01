using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace Viktor
{
    public class Predictions
    {
        //public Predictions (Obj_AI_Base dasher,Dash.DashItem args)
        //{
        //    Dasher = dasher;
        //    Args = args;
        //}
        //public Obj_AI_Base Dasher { get; set; }
        //public Dash.DashItem Args { get; set; }

        //private static List<Predictions> Dashes;
        //static Predictions()
        //{
        //     CustomEvents.Unit.OnDash += OnDash;
        //     foreach (var x in Dashes)
        //     {
        //         if (Utils.TickCount - Game.Ping/2 >= x.Args.EndTick) Dashes.Remove(x);
        //     }
        //}
        //public static void OnDash(Obj_AI_Base sender, Dash.DashItem args)
        //{
        //    if (sender.IsEnemy) Dashes.Add(new Predictions(sender,args));
        //}
        //public static Vector3 GetPrediction(Vector3 from, Obj_AI_Base target, Spells spell, bool DashPredict, bool Immobile)
        //{
        //    if (target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Stun) && Immobile == true && !target.IsDashing())
        //    {
        //        float time = CastSpell.GetImmobileTime(target);
        //        if (time / 1000 + (spell.radius + target.BoundingRadius - 20) / target.MoveSpeed >= spell.delay + Game.Ping / 2 / 1000 + from.Distance(target.Position) / spell.speed)
        //            return target.Position;
        //        else { return GetPrediction(from, target, spell); }
        //    }
        //    else
        //    {
        //        if (DashPredict == false) { return GetPrediction(from, target, spell); }
        //        else if (!target.IsDashing()) { return GetPrediction(from, target, spell); }
        //        else
        //        {
        //            Predictions x = null;
        //            foreach (var y in Dashes) { if (y.Dasher.Name == target.Name)x = y; }
        //            if (x == null) { return GetPrediction(from, target, spell); }
        //            else
        //            {
        //                if (Utils.TickCount + Game.Ping / 2 + spell.delay * 1000 + (from.To2D().Distance(x.Args.EndPos) / spell.speed) * 1000 <= x.Args.EndTick)
        //                {
        //                    Vector3 chuot = x.Args.EndPos.To3D();
        //                    float dis = from.Distance(target.Position);
        //                    float rad = target.BoundingRadius + spell.radius - 20;
        //                    double z = math.t(x.Args.Speed, spell.speed, dis, spell.delay + Game.Ping / 2 / 1000, rad, from, target.Position, chuot);
        //                    if (z != 0) { return target.Position.Extend(chuot, (float)z * x.Args.Speed - rad); }
        //                    else return new Vector3(0, 0, 0);
        //                }
        //                else { return GetPrediction(from, target, spell); }
        //            }
        //        }
        //    }
        //}
        public static Vector3 GetPrediction(Vector3 from, Obj_AI_Base target, Spells spell, bool Immobile)
        {
            if (target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Stun) && Immobile == true && !target.IsDashing())
            {
                float time = CastSpell.GetImmobileTime(target);
                if (time / 1000 + (spell.radius + target.BoundingRadius - 20) / target.MoveSpeed >= spell.delay + Game.Ping / 2 / 1000 + from.Distance(target.Position) / spell.speed)
                    return target.Position;
                else { return GetPrediction(from, target, spell); }
            }
            else
            {
                return GetPrediction(from, target, spell);
            }
        }
        public static Vector3 GetPrediction(Vector3 from, Obj_AI_Base target, Spells spell)
        {
            Vector3 chuot = Prediction.GetPrediction(target, 1).CastPosition;
            float dis = from.Distance(target.Position);
            float rad = target.BoundingRadius + spell.radius - 50;
            double x = math.t(target.MoveSpeed, spell.speed, dis, spell.delay + Game.Ping / 2 / 1000, rad, from, target.Position, chuot);
            if (x != 0 && !target.IsDashing()) { return target.Position.Extend(chuot, (float)x * target.MoveSpeed - rad); }
            else return target.Position;
        }
        public static Vector3 GetPrediction(Obj_AI_Base target, float delay)
        {
            var chuot = Prediction.GetPrediction(target, 1).CastPosition;
            float y = target.MoveSpeed * 100 / 1000;
            bool moving = !(target.Distance(chuot) < y);
            var predictionpos = moving == false ? target.Position : target.Position.Extend(chuot, delay * target.MoveSpeed);
            return predictionpos;
        }
    }
    public class Collisions
    {
        private static int _wallCastT;
        private static Vector2 _yasuoWallCastedPos;

        public CollisionableObjects[] CollisionObjects =
        {
            CollisionableObjects.Minions, CollisionableObjects.YasuoWall
        };
        static Collisions()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsValid && sender.Team != ObjectManager.Player.Team && args.SData.Name == "YasuoWMovingWall")
            {
                _wallCastT = Utils.TickCount;
                _yasuoWallCastedPos = sender.ServerPosition.To2D();
            }
        }

        public static List<Obj_AI_Base> GetCollisions(Vector3 from, Vector3 to, Spells spell)
        {
            var result = new List<Obj_AI_Base>();
            float t = spell.delay + Game.Ping / 2 / 1000 + from.Distance(to) / spell.speed;
            foreach (var obj in ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.Type != GameObjectType.obj_AI_Turret && obj.Type != GameObjectType.obj_Building && obj.IsEnemy || obj.Team == GameObjectTeam.Neutral))
            {
                if (obj.IsMoving)
                {
                    var chuot = Prediction.GetPrediction(obj, 100).CastPosition;
                    Vector3 obj2 = obj.Position.Extend(chuot, t * obj.MoveSpeed);
                    var x = Geometry.Intersection(from.To2D(), to.To2D(), obj.Position.To2D(), obj2.To2D());
                    if (x.Intersects == true) result.Add(obj);
                    else
                    {
                        Vector2 y;
                        double d; math.FindDistanceToSegment(obj2.To2D(), from.To2D(), to.To2D(), out y, out d);
                        if (from.To2D().Distance(y) <= from.To2D().Distance(to.To2D()) && to.To2D().Distance(y) <= from.To2D().Distance(to.To2D()))
                        { if (d <= obj.BoundingRadius + spell.radius)result.Add(obj); }
                    }
                }
                else
                {
                    Vector2 y;
                    double d; math.FindDistanceToSegment(obj.Position.To2D(), from.To2D(), to.To2D(), out y, out d);
                    if (from.To2D().Distance(y) <= from.To2D().Distance(to.To2D()) && to.To2D().Distance(y) <= from.To2D().Distance(to.To2D()))
                    { if (d <= obj.BoundingRadius + spell.radius)result.Add(obj); }
                }

            }
            GameObject wall = null;
            foreach (var gameObject in
                                ObjectManager.Get<GameObject>()
                                    .Where(
                                        gameObject =>
                                            gameObject.IsValid &&
                                            Regex.IsMatch(
                                                gameObject.Name, "_w_windwall_enemy_0.\\.troy", RegexOptions.IgnoreCase)))
            {
                if (Utils.TickCount - _wallCastT > 4000)
                {
                    break;
                }
                wall = gameObject;
                if (wall == null)
                {
                    break;
                }
                var level = wall.Name.Substring(wall.Name.Length - 6, 1);
                var wallWidth = (300 + 50 * Convert.ToInt32(level));

                var wallDirection =
                    (wall.Position.To2D() - _yasuoWallCastedPos).Normalized().Perpendicular();
                var wallStart = wall.Position.To2D() + wallWidth / 2f * wallDirection;
                var wallEnd = wallStart - wallWidth * wallDirection;
                var x = Geometry.Intersection(wallStart, wallEnd, from.To2D(), to.To2D());
                if (x.Intersects == true) result.Add(ObjectManager.Player);
                else
                {
                    Vector2 y;
                    double d; math.FindDistanceToSegment(wallStart, from.To2D(), to.To2D(), out y, out d);
                    Vector2 y1; double d1; math.FindDistanceToSegment(wallEnd, from.To2D(), to.To2D(), out y1, out d1);
                    if (from.To2D().Distance(y) <= from.To2D().Distance(to.To2D()) && to.To2D().Distance(y) <= from.To2D().Distance(to.To2D()))
                    { if (d <= spell.radius)result.Add(ObjectManager.Player); }
                    if (from.To2D().Distance(y1) <= from.To2D().Distance(to.To2D()) && to.To2D().Distance(y1) <= from.To2D().Distance(to.To2D()))
                    { if (d1 <= spell.radius)result.Add(ObjectManager.Player); }

                }

            }
            return result.Distinct().ToList();
        }
        public static List<Obj_AI_Base> GetCollision(Vector3 from, Vector3 to, Spells spell)
        {
            var result = new List<Obj_AI_Base>();
            var list = new List<CollisionableObjects>(); list.Add(CollisionableObjects.Minions); list.Add(CollisionableObjects.YasuoWall);
            float time = spell.delay + Game.Ping / 2 / 1000 + from.Distance(to) / spell.speed;
            foreach (var objectType in list)
            {
                switch (objectType)
                {
                    case CollisionableObjects.Minions:
                        foreach (var minion in
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(
                                    minion =>
                                        minion.IsValidTarget(
                                            Math.Min(spell.range + spell.radius + 100, 2000), true,
                                            from)))
                        {
                            //var chuot = Prediction.GetPrediction(minion, 100).CastPosition;
                            //float y = minion.MoveSpeed * 100 / 1000;
                            //bool moving = !(minion.Distance(chuot) < y);
                            //var minionPrediction = moving == false ? minion.Position : minion.Position.Extend(chuot, time * minion.MoveSpeed);
                            var minionPrediction = Prediction.GetPrediction(minion, time * 1000).UnitPosition;
                            if (
                                minionPrediction.To2D()
                                    .Distance(from.To2D(), to.To2D(), true, true) <=
                                Math.Pow((spell.radius * 2 + 15 + minion.BoundingRadius), 2) ||
                                minion.Position.To2D().Distance(from.To2D(), to.To2D(), true, true) <=
                                Math.Pow((spell.radius * 2 + 15 + minion.BoundingRadius), 2))
                            {
                                result.Add(minion);
                            }
                        }
                        break;
                    //case CollisionableObjects.Heroes:
                    //    foreach (var hero in
                    //        HeroManager.Enemies.FindAll(
                    //            hero =>
                    //                hero.IsValidTarget(
                    //                    Math.Min(input.Range + input.Radius + 100, 2000), true, input.RangeCheckFrom))
                    //        )
                    //    {
                    //        input.Unit = hero;
                    //        var prediction = Prediction.GetPrediction(input, false, false);
                    //        if (
                    //            prediction.UnitPosition.To2D()
                    //                .Distance(input.From.To2D(), position.To2D(), true, true) <=
                    //            Math.Pow((input.Radius + 50 + hero.BoundingRadius), 2))
                    //        {
                    //            result.Add(hero);
                    //        }
                    //    }
                    //    break;

                    //case CollisionableObjects.Walls:
                    //    var step = position.Distance(input.From) / 20;
                    //    for (var i = 0; i < 20; i++)
                    //    {
                    //        var p = input.From.To2D().Extend(position.To2D(), step * i);
                    //        if (NavMesh.GetCollisionFlags(p.X, p.Y).HasFlag(CollisionFlags.Wall))
                    //        {
                    //            result.Add(ObjectManager.Player);
                    //        }
                    //    }
                    //    break;

                    case CollisionableObjects.YasuoWall:

                        if (Utils.TickCount - _wallCastT > 4000)
                        {
                            break;
                        }

                        GameObject wall = null;
                        foreach (var gameObject in
                            ObjectManager.Get<GameObject>()
                                .Where(
                                    gameObject =>
                                        gameObject.IsValid &&
                                        Regex.IsMatch(
                                            gameObject.Name, "_w_windwall_enemy_0.\\.troy", RegexOptions.IgnoreCase))
                            )
                        {
                            wall = gameObject;
                        }
                        if (wall == null)
                        {
                            break;
                        }
                        var level = wall.Name.Substring(wall.Name.Length - 6, 1);
                        var wallWidth = (300 + 50 * Convert.ToInt32(level));

                        var wallDirection =
                            (wall.Position.To2D() - _yasuoWallCastedPos).Normalized().Perpendicular();
                        var wallStart = wall.Position.To2D() + wallWidth / 2f * wallDirection;
                        var wallEnd = wallStart - wallWidth * wallDirection;

                        if (wallStart.Intersection(wallEnd, to.To2D(), from.To2D()).Intersects)
                        {
                            var t = Utils.TickCount +
                                    (wallStart.Intersection(wallEnd, to.To2D(), from.To2D())
                                        .Point.Distance(from) / spell.speed + spell.delay) * 1000;
                            if (t < _wallCastT + 4000)
                            {
                                result.Add(ObjectManager.Player);
                            }
                        }

                        break;
                }
            }


            return result.Distinct().ToList();
        }
    }
    public class math
    {
        public static void FindDistanceToSegment(Vector2 pt, Vector2 p1, Vector2 p2, out Vector2 closest, out double d)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                d = Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Vector2(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Vector2(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new Vector2(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            d = Math.Sqrt(dx * dx + dy * dy);
        }

        public static double ptb2(float a, float b, float c)
        {
            if (a != 0)
            {
                float delta = b * b - 4 * a * c;
                if (delta < 0) return 0;
                else
                {
                    double canDelta = Math.Sqrt(delta);
                    double x1 = (-b + canDelta) / (2 * a); double x2 = (-b - canDelta) / (2 * a);
                    double X = x1 > x2 ? x1 : x2;
                    return X > 0 ? X : 0;
                }
            }
            else { double X = -c / b; return X > 0 ? X : 0; }
        }
        public static float CosB(Vector3 a, Vector3 b, Vector3 c)
        {
            float a1 = c.Distance(b);
            float b1 = a.Distance(c);
            float c1 = b.Distance(a);
            if (a1 == 0 || c1 == 0) { return 360; }
            else { return (a1 * a1 + c1 * c1 - b1 * b1) / (2 * a1 * c1); }
        }

        public static double t(float ms, float flys, float dis, float delay, float rad, Vector3 may, Vector3 no, Vector3 chuot)
        {
            float cosA = CosB(may, no, chuot);
            if (cosA == 360) { return 0; }
            else
            {
                float a = ms * ms - flys * flys; float b = -2 * rad * ms + 2 * flys * flys * delay - 2 * cosA * ms * dis;
                float c = rad * rad + dis * dis - flys * flys * delay * delay + 2 * cosA * dis * rad;
                double X = ptb2(a, b, c); return X;
            }
        }
    }

}
