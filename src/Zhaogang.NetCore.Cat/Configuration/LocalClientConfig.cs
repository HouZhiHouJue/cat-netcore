using Zhaogang.NetCore.Cat.Configuration;
using Zhaogang.NetCore.Cat.Message.Spi;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Zhaogang.NetCore.Cat.Message.Spi.Internals;
using Zhaogang.NetCore.Cat.Util;
using Microsoft.Extensions.Configuration;

namespace Zhaogang.NetCore.Cat.Configuration
{
    // CAT clienet config, which is loaded from a local XML file.
    class LocalClientConfig : AbstractClientConfig
    {
        public LocalClientConfig()
        {
            Init();
            var servers = base.GetCatTcpServers(true);
            if (servers.Count > 0)
            {
                Servers = servers;
            }
        }

        protected override string GetCatRouterServiceURL(bool sync)
        {
            // TODO need to try multiple servers here.
            if (Servers.Count > 0)
            {
                Server server = Servers[0];
                // http://192.168.183.100:8080/cat/s/router
                return "http://" + server.Ip + ":" + server.HttpPort + "/cat/s/router";
            }
            else
                return null;
        }

        private string GetDomainId()
        {
            return this.Domain.Id;
        }

        private void Init()
        {
            var builder = new ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
            var Configuration = builder.Build();
            var systemAlias = Configuration.GetSection("FrameworkSetting:SystemAlias").Value;
            if (string.IsNullOrEmpty(systemAlias))
                throw new ArgumentNullException("FrameworkSetting:SystemAlias Must Be Configured First");
            this.MaxQueueSize = 5000;
            this.MaxQueueByteSize = 32000000;
            this.Domain = new Domain() { Enabled = true, Id = systemAlias };
            bool logEnable = true;
            Logger.Initialize(this.Domain.Id, logEnable);
            // Logger.Info("Use config file({0}).", configFile);

            var serverList= Configuration.GetSection("ZhaoGangMonitor:Severs").Value.Split(new char[] { ','},StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in serverList)
            {
                Servers.Add(new Server(item));
            }
            //NOTE: 只添加Enabled的
            //Servers = new List<Server>();
            //foreach (Server server in servers.Where(server => server.Enabled))
            //{
            //    Servers.Add(server);
            //    Logger.Info("CAT server configured: {0}:{1}", server.Ip, server.Port);
            //}

            //if (!String.IsNullOrWhiteSpace(configFile) && File.Exists(configFile))
            //{
            //    XmlDocument doc = new XmlDocument();

            //    doc.Load(new FileStream(configFile, FileMode.Open));

            //    XmlElement root = doc.DocumentElement;

            //    if (root != null)
            //    {
            //        this.MaxQueueSize = GetMaxQueueSize(root);
            //        this.MaxQueueByteSize = GetMaxQueueByteSize(root);
            //        this.Domain = BuildDomain(root.GetElementsByTagName("domain"));
            //        bool logEnable = BuildLogEnabled(root.GetElementsByTagName("logEnabled"));
            //        Logger.Initialize(this.Domain.Id, logEnable);
            //        Logger.Info("Use config file({0}).", configFile);

            //        IEnumerable<Server> servers = BuildServers(root.GetElementsByTagName("servers"));

            //        //NOTE: 只添加Enabled的
            //        Servers = new List<Server>();
            //        foreach (Server server in servers.Where(server => server.Enabled))
            //        {
            //            Servers.Add(server);
            //            Logger.Info("CAT server configured: {0}:{1}", server.Ip, server.Port);
            //        }
            //    }
            //}
            //else
            //{
            //    Logger.Warn("Config file({0}) not found, using localhost:2280 instead.", configFile);
            //    Domain = BuildDomain(null);
            //    Servers.Add(new Server("localhost", 2280));
            //}
        }

        private int GetMaxQueueSize(XmlElement element)
        {
            try
            {
                var maxQueueSizeStr = element.GetAttribute("max-queue-size");
                if (!String.IsNullOrWhiteSpace(maxQueueSizeStr))
                {
                    var maxQueueSize = int.Parse(maxQueueSizeStr);
                    if (maxQueueSize > 0)
                    {
                        return maxQueueSize;
                    }
                }
            }
            catch (Exception ex)
            { Cat.lastException = ex; }
            return DEFAULT_MAX_QUEUE_SIZE;
        }

        private int GetMaxQueueByteSize(XmlElement element)
        {
            try
            {
                var maxQueueByteSizeStr = element.GetAttribute("max-queue-byte-size");
                if (!String.IsNullOrWhiteSpace(maxQueueByteSizeStr))
                {
                    var maxQueueByteSize = int.Parse(maxQueueByteSizeStr);
                    if (maxQueueByteSize > 0)
                    {
                        return maxQueueByteSize;
                    }
                }
            }
            catch (Exception ex)
            { Cat.lastException = ex; }
            return DEFAULT_MAX_QUEUE_BYTE_SIZE;
        }

        private static Domain BuildDomain(XmlNodeList nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return new Domain();
            }

            XmlElement node = (XmlElement)nodes[0];
            return new Domain
            {
                Id = GetStringProperty(node, "id", CatConstants.UNKNOWN_DOMAIN).Trim(),
                //Ip = GetStringProperty(node, "ip", null),
                Enabled = GetBooleanProperty(node, "enabled", true)
            };
        }

        private static bool BuildLogEnabled(XmlNodeList nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return false;
            }
            XmlElement node = (XmlElement)nodes[0];
            return GetBooleanProperty(node, "enabled", false);
        }

        private static IEnumerable<Server> BuildServers(XmlNodeList nodes)
        {
            List<Server> servers = new List<Server>();

            if (nodes != null && nodes.Count > 0)
            {
                XmlElement first = (XmlElement)nodes[0];
                XmlNodeList serverNodes = first.GetElementsByTagName("server");

                foreach (XmlNode node in serverNodes)
                {
                    XmlElement serverNode = (XmlElement)node;
                    string ip = GetStringProperty(serverNode, "ip", "localhost");
                    int port = GetIntProperty(serverNode, "port", 2280);
                    int httpPort = GetIntProperty(serverNode, "http-port", 8080);
                    Server server = new Server(ip, port, httpPort) { Enabled = GetBooleanProperty(serverNode, "enabled", true) };

                    servers.Add(server);
                }
            }

            if (servers.Count == 0)
            {
                Logger.Warn("No server configured, use localhost:2280 instead.");
                servers.Add(new Server("localhost", 2280));
            }

            return servers;
        }

        private static string GetStringProperty(XmlElement element, string name, string defaultValue)
        {
            if (element != null)
            {
                string value = element.GetAttribute(name);

                if (value.Length > 0)
                {
                    return value;
                }
            }

            return defaultValue;
        }

        private static bool GetBooleanProperty(XmlElement element, string name, bool defaultValue)
        {
            if (element != null)
            {
                string value = element.GetAttribute(name);

                if (value.Length > 0)
                {
                    return "true".Equals(value);
                }
            }

            return defaultValue;
        }

        private static int GetIntProperty(XmlElement element, string name, int defaultValue)
        {
            if (element != null)
            {
                string value = element.GetAttribute(name);

                if (value.Length > 0)
                {
                    int tmpRet;
                    if (int.TryParse(value, out tmpRet))
                        return tmpRet;
                }
            }

            return defaultValue;
        }

        public override string GetConfigHeartbeatMessage()
        {
            return null;
        }
    }
}
