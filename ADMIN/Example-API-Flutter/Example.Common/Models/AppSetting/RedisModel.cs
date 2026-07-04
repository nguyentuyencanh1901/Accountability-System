namespace Example.Common.Models.AppSetting
{
    public class RedisModel
    {
        public int AllowRedisCache { get; set; }
        public string RedisIP { get; set; }
        public string RedisDB { get; set; }
        public string RedisPort { get; set; }
        public bool AllowCountAutoView { get; set; }
        public string CountAutoViewRedisDB { get; set; }
    }
}
