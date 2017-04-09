using GTANetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stuykserver.Classes
{
    public class arpg : Script
    {
        [Command("arpg")]
        public void cmdEnableARPG(Client player)
        {
            API.triggerClientEvent(player, "enableARPG");
        }
    }
}
