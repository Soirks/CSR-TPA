using CSR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace tpa
{
    public class tpa
    {
        public static void tpaa(MCCSAPI api)
        {
            api.setCommandDescribe("homegui", "打开homegui");
            api.setCommandDescribe("tpato", "传送到一个玩家");
            api.setCommandDescribe("tpac", "同游传送请求");
            api.setCommandDescribe("tpde", "拒绝传送请求");
            api.setCommandDescribe("tpapb", "改变tpa屏蔽状态");
            api.setCommandDescribe("tpagui", "打开tpagui");
            api.setCommandDescribe("homeadd", "添加一个私人传送点");
            api.setCommandDescribe("homedel", "删除一个私人传送点");
            api.setCommandDescribe("homego", "前往一个私人传送点");
            api.setCommandDescribe("back", "返回上一个死亡点");
            Dictionary<string, string> uuid = new Dictionary<string, string>();//uuid
            Dictionary<string, string> tpa_pb = new Dictionary<string, string>();//tpa屏蔽状态
            Dictionary<string, string> tpa_dx = new Dictionary<string, string>();//tpa对象
            Dictionary<string, string> tpa_ys = new Dictionary<string, string>();//tpa延时用
            Dictionary<string, string> tpa_gui = new Dictionary<string, string>();//记录guiid
            Dictionary<string, string> guils = new Dictionary<string, string>();//gui类型
            Dictionary<string, string> back_x = new Dictionary<string, string>();//backx
            Dictionary<string, string> back_y = new Dictionary<string, string>();//backy
            Dictionary<string, string> back_z = new Dictionary<string, string>();//backz
            Dictionary<string, int> back_did = new Dictionary<string, int>();//back维度
            ArrayList onlineplayer = new ArrayList();//在线玩家
            int tpa_yx = 30000;
            string __back = "true";
            string home_max = "5";
            if (File.Exists("./config/tpa.txt"))
            {
                try
                {
                    string[] config = File.ReadAllLines("./config/tpa.txt",System.Text.Encoding.Default);
                    tpa_yx = int.Parse(config[0].Substring(12));
                    __back = config[1].Substring(12);
                    home_max = config[2].Substring(14);
                    Console.WriteLine("[TPA]配置文件读取成功！");
                }
                catch { Console.WriteLine("[TPA]配置文件读取失败！"); }
              
            }
            else
            {
                Directory.CreateDirectory("config/");
                File.AppendAllText("./config/tpa.txt", "玩家tpa请求有效时间:30000\n是否开启/back功能:true\n玩家设置home的最大数量:5",System.Text.Encoding.Default);
                Console.WriteLine("[TPA]未检查到配置文件！将自动创建！");
            }
            api.addAfterActListener(EventKey.onLoadName, x =>
            {
                var a = BaseEvent.getFrom(x) as LoadNameEvent;
                    uuid.Add(a.playername, a.uuid);
                    guils.Add(a.playername, string.Empty);
                    onlineplayer.Add(a.playername);
                    if (tpa_pb.ContainsKey(a.playername) == false)
                    {
                        tpa_pb.Add(a.playername, "no");
                    }
                try
                {
                    tpa_dx.Add(a.playername, "cxk");
                    tpa_gui.Add(a.playername, "a");
                    tpa_ys.Add(a.playername, "0");
                    back_x.Add(a.playername, string.Empty);
                    back_y.Add(a.playername, string.Empty);
                    back_z.Add(a.playername, string.Empty);
                    back_did.Add(a.playername, 0);
                }
                catch { Console.WriteLine("warn!!!!"); }
                return true;
            });
            api.addBeforeActListener(EventKey.onInputCommand, x =>
            {
                bool re = true;
                var a = BaseEvent.getFrom(x) as InputCommandEvent;
                if (a.cmd.StartsWith("/tpato"))
                {
                    string tpatoplayername = string.Empty;
                    re = false;
                    try
                    {
                        if (tpa_ys[a.playername] == "0")
                        {
                            tpatoplayername = a.cmd.Substring(7);
                            if (api.getOnLinePlayers().IndexOf(tpatoplayername) != -1 || tpatoplayername.Length > 5)
                            {
                                if (tpa_pb[tpatoplayername] == "no")
                                {
                                    api.runcmd("tellraw \"" + tpatoplayername + "\" {\"rawtext\":[{\"text\":\"玩家" + a.playername + "向您发送了一个传送请求,/tpac接受，/tpde拒绝\"}]}");
                                    tpa_ys[a.playername] = "1";
                                    tpa_dx[tpatoplayername] = a.playername;
                                    tpa_gui[tpatoplayername] =  api.sendModalForm(uuid[tpatoplayername], "TPA请求", "玩家" + a.playername + "向您发送了一个传送请求", "同意", "拒绝").ToString();
                                    Task taskkk = Task.Run(async () =>
                                    {
                                        await Task.Delay(tpa_yx);
                                        if (tpa_ys[a.playername] == "1")
                                        {
                                            tpa_ys[a.playername] = "0";
                                            api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"tpa请求超时\"}]}");
                                            tpa_dx[tpatoplayername] = "cxk";
                                        }
                                    });
                                }
                                else
                                {
                                    api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"对方屏蔽了tpa请求\"}]}");
                                }
                            }
                            else
                            {
                                api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"tpa请求发送失败，请检查您输入的指令\"}]}");
                            }
                        }
                        else
                        {
                            api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"您有另一个tpa正在进行中！\"}]}");
                        }
                    }

                    catch
                    {
                        api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"tpa请求发送失败，请检查您输入的指令\"}]}");
                        Console.WriteLine("warn！");
                    }
                }
                if (a.cmd.StartsWith("/tpapb"))
                {
                    re = false;
                    if (tpa_pb[a.playername] == "yes")
                    {
                        tpa_pb[a.playername] = "no";
                    }
                    else
                    {
                        tpa_pb[a.playername] = "yes";
                    }
                }
                if (a.cmd.StartsWith("/tpac"))
                {
                    re = false;
                    if (tpa_dx[a.playername] != "cxk")
                    {
                        api.runcmd("tp \""+a.playername+ "\" "+tpa_dx[a.playername]);
                        tpa_ys[tpa_dx[a.playername]] = "0";
                        tpa_dx[a.playername] = "cxk";
                    }
                    else
                    {
                        api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"没有人向你发送传送请求！\"}]}");
                    }
                }
                if (a.cmd.StartsWith("/tpde"))
                {
                    re = false;
                    if (tpa_dx[a.playername] != "cxk")
                    {
                        api.runcmd("tellraw \"" + tpa_dx[a.playername] + "\" {\"rawtext\":[{\"text\":\"对方拒绝了您的传送请求\"}]}");
                        tpa_ys[tpa_dx[a.playername]] = "0";
                        tpa_dx[a.playername] = "cxk";
                    }
                    else
                    {
                        api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"没有人向你发送传送请求！\"}]}");
                    }
                }
                if (a.cmd.StartsWith("/tpagui"))
                {
                    re = false;
                    string online = "[\"";
                    foreach (string p in onlineplayer)
                    {
                        online = online + "\",\"" + p;
                    }
                    online = online + "\"]";                          
                    online = "[" + online.Substring(4);
                    api.sendCustomForm(uuid[a.playername], "{\"content\":[{\"type\":\"label\",\"text\":\"这个一个TPAGUI喵\"},{\"default\":0,\"options\":" + online + ",\"type\":\"dropdown\",\"text\":\"请选择一个玩家\"}], \"type\":\"custom_form\",\"title\":\"TPAGUI\"}").ToString();
                    guils[a.playername] = "fz";
                }
                if (__back == "true")
                {
                    if (a.cmd.StartsWith("/back") && a.cmd.EndsWith("/back"))
                    {
                        re = false;
                        if (back_x[a.playername] != string.Empty)
                        {
                            api.teleport(uuid[a.playername], Convert.ToSingle( back_x[a.playername]), Convert.ToSingle(back_y[a.playername]), Convert.ToSingle(back_z[a.playername]),back_did[a.playername]);
                            api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"命令已执行\"}]}");
                            back_x[a.playername] = string.Empty;
                            back_y[a.playername] = string.Empty;
                            back_z[a.playername] = string.Empty;
                        }
                        else
                        {
                            api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"未找到死亡点!\"}]}");
                        }
                    }
                }
                if (a.cmd.StartsWith("/homeadd "))
                {
                    re = false;
                    if (File.Exists("./data/tpa/" + a.playername + ".txt"))
                    {
                        if (File.ReadAllLines("./data/tpa/" + a.playername + ".txt").Length < int.Parse(home_max))
                        {
                            File.AppendAllText("./data/tpa/" + a.playername + ".txt", a.cmd.Substring(9) + "-" + a.XYZ.x + " " + a.XYZ.y + " " + a.XYZ.z + "\n");
                        }
                        else
                        {
                            api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"您设置的home数量已达到上限!\"}]}");
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory("./data/tpa");
                        File.AppendAllText("./data/tpa/" + a.playername + ".txt", a.cmd.Substring(9) + "-" + a.XYZ.x + " " + a.XYZ.y + " " + a.XYZ.z + "\n");
                    }
                }
                if (a.cmd == "/homegui")
                {
                    re = false;
                    if (File.Exists("./data/tpa/" + a.playername + ".txt"))
                    {
                        string[] lines = File.ReadAllLines("./data/tpa/" + a.playername + ".txt");
                        if (lines.Length != 0)
                        {
                            string homes = "[";
                            foreach(string line in lines)
                            {
                                Console.WriteLine(line);
                                homes = homes +"\""+ line.Substring(0,line.IndexOf("-")) + "\""  + ",";
                            }
                            homes = homes.Substring(0, homes.Length-1) + "]";
                            api.sendCustomForm(uuid[a.playername], "{\"content\":[{\"type\":\"label\",\"text\":\"这个一个Thomegui\"},{\"default\":0,\"options\":" + homes + ",\"type\":\"dropdown\",\"text\":\"请选择一个家\"}], \"type\":\"custom_form\",\"title\":\"HOMEGUI\"}").ToString();
                            guils[a.playername] = "homegui";
                        }
                        else { api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"您还没有任何家!\"}]}"); }
                    }
                    else
                    {
                        api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"您还没有任何家!\"}]}");
                    }
                }
                if (a.cmd.StartsWith("/homego "))
                {
                    re = false;
                    string tz = a.cmd.Substring(8);
                    if (File.Exists("./data/tpa/" + a.playername + ".txt"))
                    {
                        byte bbb = 1;
                        foreach (string line in File.ReadAllLines("./data/tpa/" + a.playername + ".txt"))
                        {
                            if (line.StartsWith(tz))
                            {
                                bbb = 0;
                                api.runcmd("tp \"" + a.playername + "\" " + line.Substring(tz.Length + 1));
                                break;
                            }
                        }
                        if (bbb == 1)
                        {
                            api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"未找到该名称的home点!\"}]}");
                        }
                    }
                }
                if (a.cmd.StartsWith("/homedel "))
                {
                    re = false;
                    if (File.Exists("./data/tpa/" + a.playername + ".txt"))
                    {
                        string[] lines = File.ReadAllLines("./data/tpa/" + a.playername + ".txt", System.Text.Encoding.Default);
                        if (lines.Length != 0)
                        {
                            ArrayList ol = new ArrayList();
                            foreach(string line in lines)
                            {
                                ol.Add(line);
                                if (line.StartsWith(a.cmd.Substring(9)))
                                {
                                    ol.Remove(line);
                                }
                            }
                            if (ol.Count == lines.Length)
                            {
                                api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"未找到该名字的home点\"}]}");
                            }
                            else
                            {
                                File.Delete("./data/tpa/" + a.playername + ".txt");
                                File.AppendAllLines("./data/tpa/" + a.playername + ".txt", (string[])ol.ToArray(typeof(string)));
                                api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"该home点已删除！\"}]}");
                            }
                        }
                    }
                    else
                    {
                        api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"您还没有任何home点\"}]}");
                    }
                }
                return re;
            });
            api.addAfterActListener(EventKey.onMobDie, x =>
            {
                var a = BaseEvent.getFrom(x) as MobDieEvent;
                if (a.mobtype == "entity.player.name")
                {
                    try
                    {
                        back_x[a.playername] = a.XYZ.x.ToString();
                        back_y[a.playername] = a.XYZ.y.ToString();
                        back_z[a.playername] = a.XYZ.z.ToString();
                        back_did[a.playername] = a.dimensionid;
                    }
                    catch { }
                }
                return true;
            });
            api.addAfterActListener(EventKey.onPlayerLeft, x =>
            {
                var a = BaseEvent.getFrom(x) as PlayerLeftEvent;
                uuid.Remove(a.playername);
                guils.Remove(a.playername);
                onlineplayer.Remove(a.playername);
                tpa_dx.Remove(a.playername);
                tpa_gui.Remove(a.playername);
                tpa_ys.Remove(a.playername);
                back_x.Remove(a.playername);
                back_y.Remove(a.playername);
                back_z.Remove(a.playername);
                return true;
            });
            api.addAfterActListener(EventKey.onFormSelect, x =>
            {
            var a = BaseEvent.getFrom(x) as FormSelectEvent;
                if (guils[a.playername] == "homegui")
                {
                    try
                    {
                        int ssss = Convert.ToInt32(a.selected.Substring(6, a.selected.Length - 7));
                        string[] lines = File.ReadAllLines("./data/tpa/" + a.playername + ".txt");
                        guils[a.playername] = string.Empty;
                        api.runcmd("tp " + a.playername + " " + lines[ssss].Substring(lines[ssss].IndexOf("-") + 1));
                    }
                    catch { }
                }
                if (guils[a.playername] == "fz")
                {
                    try
                    {
                        if (tpa_ys[a.playername] == "0")
                        {
                            String tpatoplayername;
                            tpatoplayername = onlineplayer[int.Parse(a.selected.Substring(6, 1))].ToString();
                            if (api.getOnLinePlayers().IndexOf(tpatoplayername) != -1 && tpatoplayername.Length > 5)
                            {
                                if (tpa_pb[tpatoplayername] == "no")
                                {
                                    api.runcmd("tellraw \"" + tpatoplayername + "\" {\"rawtext\":[{\"text\":\"玩家" + a.playername + "向您发送了一个传送请求,/tpac接受，/tpde拒绝\"}]}");
                                    tpa_ys[a.playername] = "1";
                                    tpa_dx[tpatoplayername] = a.playername;
                                    tpa_gui[tpatoplayername] = api.sendModalForm(uuid[tpatoplayername], "TPA请求", "玩家" + a.playername + "向您发送了一个传送请求", "同意", "拒绝").ToString();
                                    guils[tpatoplayername] = "jd";
                                    guils[a.playername] = string.Empty;
                                    Task taskkk = Task.Run(async () =>
                                    {
                                        await Task.Delay(tpa_yx);
                                        if (tpa_ys[a.playername] == "1")
                                        {
                                            tpa_ys[a.playername] = "0";
                                            api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"tpa请求超时\"}]}");
                                            tpa_dx[tpatoplayername] = "cxk";
                                        }
                                    });
                                }
                                else
                                {
                                    api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"对方屏蔽了tpa请求\"}]}");
                                }
                            }
                            else
                            {
                                api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"tpa请求发送失败，请检查您输入的指令\"}]}");
                            }
                        }
                    }
                    catch { }
                }
                if (guils[a.playername] == "jd")
                {
                    if (a.selected == "true")
                    {
                        if (tpa_dx[a.playername] != "cxk")
                        {
                            if (tpa_dx[a.playername] != "cxk")
                            {
                                api.runcmd("tp \"" + a.playername + "\" " + tpa_dx[a.playername]);
                                tpa_ys[tpa_dx[a.playername]] = "0";
                                tpa_dx[a.playername] = "cxk";
                            }
                        }
                    }
                    if (a.selected == "false")
                    {
                        api.runcmd("tellraw \"" + tpa_dx[a.playername] + "\" {\"rawtext\":[{\"text\":\"对方拒绝了您的传送请求\"}]}");
                        tpa_ys[tpa_dx[a.playername]] = "0";
                        tpa_dx[a.playername] = "cxk";
                    }
                    guils[a.playername] = string.Empty;
                }
                return true;
            });
        }
    }
}
/*namespace aaaa
{
    public class test
    {
        public static void testt(MCCSAPI api)
        {
            Dictionary<string, string> uuid = new Dictionary<string, string>();
            api.addAfterActListener(EventKey.onLoadName, x =>
            {
                var a = BaseEvent.getFrom(x) as LoadNameEvent;
                uuid.Add(a.playername, a.uuid);
                return true;
            });
            api.addBeforeActListener(EventKey.onInputCommand, x =>
            {
                var a = BaseEvent.getFrom(x) as InputCommandEvent;
                api.sendCustomForm(uuid[a.playername], "{\"content\":[{\"type\":\"label\",\"text\":\"这个一个TPAGUI喵\"},{\"default\":0,\"options\":[],\"type\":\"dropdown\",\"text\":\"请选择一个玩家\"}], \"type\":\"custom_form\",\"title\":\"TPAGUI\"}").ToString();
                return true;
            });
            api.addAfterActListener(EventKey.onFormSelect, x =>
            {
                var a = BaseEvent.getFrom(x) as FormSelectEvent;
                Console.WriteLine(a.selected);
                return true;
            });
        }
    }
}*/
