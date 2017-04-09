using GTANetworkServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace d20rp.classes
{
    public class loginhandler : Script
    {
        databasehandler db = new databasehandler();

        public loginhandler()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch(eventName)
            {
                case "tryLogin":
                    tryLogin(player, arguments[0].ToString(), arguments[1].ToString());
                    break;
                case "tryRegister":
                    tryRegister(player, arguments[0].ToString(), arguments[1].ToString());
                    break;
            }
        }

        private void tryLogin(Client player, string username, string password)
        {
            string hash;

            using (MD5 md5Hash = MD5.Create())
            {
                hash = GetMd5Hash(md5Hash, password);
            }

            string[] varNames = { "Username", "Password" };
            string before = "SELECT Id FROM Players WHERE";
            object[] data = { username, hash };
            DataTable result = db.compileSelectQuery(before, varNames, data);

            if (result.Rows.Count != 1)
            {
                API.triggerClientEvent(player, "loginFailed");
                return;
            }

            API.triggerClientEvent(player, "finishLoginCamera");
            API.setEntityDimension(player, 0);
            API.consoleOutput(username + " logged in.");
        }

        private void tryRegister(Client player, string username, string password)
        {
            string hash;

            using (MD5 md5Hash = MD5.Create())
            {
                hash = GetMd5Hash(md5Hash, password);
            }

            string[] varNames = { "Username" };
            string varBefore = "SELECT Username FROM Players WHERE";
            object[] varData = { username };
            DataTable result = db.compileSelectQuery(varBefore, varNames, varData);

            if (result.Rows.Count >= 1)
            {
                API.triggerClientEvent(player, "registerFailed");
                return;
            }

            string[] varRegNames = { "Username", "Password" };
            object[] varRegData = { username, hash };
            db.compileInsertQuery("Players", varRegNames, varRegData);
            API.consoleOutput(username + " registered.");
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}
