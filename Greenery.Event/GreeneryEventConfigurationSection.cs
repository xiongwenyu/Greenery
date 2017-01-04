using System.Configuration;

namespace Greenery.Event
{
    public class GreeneryEventConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// 获取配置实例
        /// </summary>
        /// <returns></returns>
        public static GreeneryEventConfigurationSection GetConfig()
        {
            GreeneryEventConfigurationSection section = (GreeneryEventConfigurationSection)ConfigurationManager.GetSection("Greenery");
            return section;
        }
        /// <summary>
        /// 消息中间服务端口
        /// </summary>
        [ConfigurationProperty("MQMIP", DefaultValue = "127.0.0.1", IsRequired = false)]
        public string MQMIP
        {
            get
            {
                return (string)base["MQMIP"];
            }
            set
            {
                base["MQMIP"] = value;
            }
        }
        /// <summary>
        /// 消息中间服务端口
        /// </summary>
        [ConfigurationProperty("MQMPort", DefaultValue = 3033, IsRequired = false)]
        public int MQMPort
        {
            get
            {
                return (int)base["MQMPort"];
            }
            set
            {
                base["MQMPort"] = value;
            }
        }

        [ConfigurationProperty("MQMPassword", DefaultValue = "admin", IsRequired = false)]
        public string MQMPassword
        {
            get
            {
                return (string)base["MQMPassword"];
            }
            set
            {
                base["MQMPassword"] = value;
            }
        }

        [ConfigurationProperty("WebSiteHost", DefaultValue = "http://localhost/", IsRequired = false)]
        public string WebSiteHost
        {
            get
            {
                return (string)base["WebSiteHost"];
            }
            set
            {
                base["WebSiteHost"] = value;
            }
        }
    }
}
