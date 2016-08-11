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
    public static class Flee
    {
        public static void BadaoActivate ()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Program._menu.Item("Flee Key").GetValue<KeyBind>().Active)
                return;
            Orbwalking.Orbwalk(null, Game.CursorPos, 90, 50);
            Program.UseW();
            if (Program._q.IsReady() && (ItemData.The_Hex_Core_mk_2.GetItem().IsOwned() || ItemData.Perfect_Hex_Core.GetItem().IsOwned()))
            {
                Program.UseQ();
                var minion = MinionManager.GetMinions(Program._q.Range).FirstOrDefault();
                var minion2 = MinionManager.GetMinions(Program._q.Range, MinionTypes.All, MinionTeam.Neutral).FirstOrDefault();
                if (minion != null)
                    Program._q.Cast(minion);
                if (minion2 != null)
                    Program._q.Cast(minion2);
            }
        }
    }
}
