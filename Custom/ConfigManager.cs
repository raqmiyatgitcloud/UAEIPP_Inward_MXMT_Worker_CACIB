namespace Raqmiyat.Framework.Custom
{
    static class ConfigManager
    {
        public static IConfiguration AppSetting { get; }
        static ConfigManager()
        {
            AppSetting = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        }
    }
}
