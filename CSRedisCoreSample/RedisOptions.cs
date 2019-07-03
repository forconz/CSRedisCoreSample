namespace CSRedisCoreSample
{
    /// <summary>
    /// Redis配置
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// 是否启用Redis
        /// </summary>
        public string Enabled { get; set; }

        /// <summary>
        /// Redis连接字符串，长度等于1为单机模式，长度大于0为集群模式
        /// </summary>
        public string[] ConnectionStrings { get; set; }
    }
}
