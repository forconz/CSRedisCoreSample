using System;
using CSRedis;
using Microsoft.Extensions.DependencyInjection;

namespace CSRedisCoreSample
{
    public static class ServicesExtensions
    {
        /// <summary>
        /// 初始化 RedisHelper，注册MVC分布式缓存
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// <param name="redisConnectionStrings">Redis连接字符串，长度等于1为单机模式，长度大于0为集群模式</param>
        /// <returns></returns>
        public static IServiceCollection AddRedisCache(this IServiceCollection services, params string[] redisConnectionStrings)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (redisConnectionStrings == null || redisConnectionStrings.Length == 0)
                throw new ArgumentNullException(nameof(redisConnectionStrings));

            CSRedisClient redisClient;
            if (redisConnectionStrings.Length == 1)
            {
                // 单机模式
                redisClient = new CSRedisClient(redisConnectionStrings[0]);
            }
            else
            {
                // 集群模式
                redisClient = new CSRedisClient(NodeRule: null, connectionStrings: redisConnectionStrings);
            }

            // 初始化 RedisHelper
            RedisHelper.Initialization(redisClient);

            //// 注册MVC分布式缓存
            //services.AddSingleton<IDistributedCache>(new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));

            return services;
        }
    }
}
