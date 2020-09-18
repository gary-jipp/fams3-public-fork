﻿using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace BcGov.Fams3.Redis.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {

        /// Add StackExchange.Redis with its serialization provider.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="redisConfiguration">The redis configration.</param>
        /// <typeparam name="T">The typof of serializer. <see cref="ISerializer" />.</typeparam>
        private static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, RedisConfiguration redisConfiguration)
            where T : class, ISerializer, new()
        {
            services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<ISerializer, T>();

            services.AddSingleton((provider) =>
            {
                return provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration();
            });

            services.AddSingleton(redisConfiguration);

            return services;
        }

        public static void AddCacheService(this IServiceCollection services, RedisConfiguration redisConfig)
        {


            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = $"{redisConfig.Hosts[0].Host}:{redisConfig.Hosts[0].Port},Password={redisConfig.Password}";

            });
            services.AddSingleton<ICacheService, CacheService>();

            services.AddSingleton(redisConfig);

            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(redisConfig);
        }
    }
}
