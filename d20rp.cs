using GTANetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace d20rp
{
    public class d20rp : Script
    {
        string createQuery = @"CREATE TABLE IF NOT EXISTS
                             [Players] (
                             [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                             [Username] NVARCHAR(2048) NULL,
                             [Password] NVARCHAR(2048) NULL)";
        
        public d20rp()
        {
            API.onResourceStart += API_onResourceStart;
        }

        private void API_onResourceStart()
        {
            
        }
    }
}
