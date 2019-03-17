using System;
using System.Threading.Tasks;
using NetFwTypeLib;

namespace Filter
{
    public static class FirewallHandler
    {
        public static Object FirewallBlockLocker = new Object();

        public static void BlockIP(string ip, string reason = "")
        {
            lock (FirewallBlockLocker)
            {
                if (FilterMain.RULE != 3) return;
                if (ip == FilterMain.gateway_local) return;

                try
                {
                    new Task(() =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(reason))
                            {
                                reason = "Blocked for exploiting/flooding KRYLFILTER";
                            }
                            const string guidFWPolicy2 = "{E2B3C97F-6AE1-41AC-817A-F6F92166D7DD}";
                            const string guidRWRule = "{2C5BC43E-3369-4C33-AB0C-BE9469677AF4}";
                            Type typeFWPolicy2 = Type.GetTypeFromCLSID(new Guid(guidFWPolicy2));
                            Type typeFWRule = Type.GetTypeFromCLSID(new Guid(guidRWRule));
                            INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(typeFWPolicy2);
                            INetFwRule netFwRule = (INetFwRule)Activator.CreateInstance(typeFWRule);
                            netFwRule.Name = $"KRYLFILTER {ip}:{reason}";
                            netFwRule.Description = $"{reason}";
                            netFwRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                            netFwRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                            netFwRule.Enabled = true;
                            netFwRule.InterfaceTypes = "All";
                            netFwRule.RemoteAddresses = ip;
                            INetFwPolicy2 netFwPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                            netFwPolicy.Rules.Add(netFwRule);
                        }
                        catch
                        {
                            //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Restart the filter in Administrator mode to use Firewall block feature.");
                            Logger.WriteLine(Logger.LogLevel.Debug, "Restart the filter in Administrator mode to use Firewall block feature.");
                        }
                    }).Start();
                }
                catch (Exception ex)
                {
                    //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] {ex.ToString()}");
                    Logger.WriteLine(Logger.LogLevel.Debug, ex.ToString());
                }
            }
        }
    }
}
