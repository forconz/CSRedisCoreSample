# CSRedisCoreSample

# Windows下安装Redis服务
安装Redis，首先要获取安装包，浏览器打开链接https://github.com/microsoftarchive/redis/releases
如果看不到下载链接，点击一下 Assets ，下载最新版的msi文件即可，这个msi文件是Windows的Redis安装包。
我下载的是https://github.com/microsoftarchive/redis/releases/download/win-3.2.100/Redis-x64-3.2.100.msi

双击刚下载好的msi格式的安装包（Redis-x64-3.2.100.msi）开始安装。
不熟悉的朋友，可以打开链接，一步步操作：https://www.cnblogs.com/jaign/articles/7920588.html

# 安装 NuGet 程序包 CSRedisCore

安装 AnotherRedisDesktopManager，查看redis服务器的数据
从 https://github.com/qishibo/AnotherRedisDesktopManager/releases 下载 Another.Redis.Desktop.Manager.1.2.3.exe

在 appsettings.json 中配置 Redis连接字符串
"Redis": {
    "Enabled": true,
    "ConnectionStrings": [
      "127.0.0.1:6379,password=foobared123456,defaultDatabase=0,poolsize=50,ssl=false,writeBuffer=10240,prefix=key",
      "127.0.0.1:6379,password=foobared123456,defaultDatabase=1,poolsize=50,ssl=false,writeBuffer=10240,prefix=bot"
    ]
  },

# 增加Redis配置类 RedisOptions
`    /// <summary>`
    `/// Redis配置`
    `/// </summary>`
    `public class RedisOptions`
    `{`
        `/// <summary>`
        `/// 是否启用Redis`
        `/// </summary>`
        `public string Enabled { get; set; }`

        `/// <summary>`
        `/// Redis连接字符串，长度等于1为单机模式，长度大于0为集群模式`
        `/// </summary>`
        `public string[] ConnectionStrings { get; set; }`
    `}`

# 增加ServicesExtensions
`    public static class ServicesExtensions`
    `{`
        `/// <summary>`
        `/// 初始化 RedisHelper，注册MVC分布式缓存`
        `/// </summary>`
        `/// <param name="services">Specifies the contract for a collection of service descriptors.</param>`
        `/// <param name="redisConnectionStrings">Redis连接字符串，长度等于1为单机模式，长度大于0为集群模式</param>`
        `/// <returns></returns>`
        `public static IServiceCollection AddRedisCache(this IServiceCollection services, params string[] redisConnectionStrings)`
        `{`
            `if (services == null) throw new ArgumentNullException(nameof(services));`
            `if (redisConnectionStrings == null || redisConnectionStrings.Length == 0)`
                `throw new ArgumentNullException(nameof(redisConnectionStrings));`

            `CSRedisClient redisClient;`
            `if (redisConnectionStrings.Length == 1)`
            `{`
                `// 单机模式`
                `redisClient = new CSRedisClient(redisConnectionStrings[0]);`
            `}`
            `else`
            `{`
                `// 集群模式`
                `redisClient = new CSRedisClient(NodeRule: null, connectionStrings: redisConnectionStrings);`
            `}`

            `// 初始化 RedisHelper`
            `RedisHelper.Initialization(redisClient);`

            `//// 注册MVC分布式缓存`
            `//services.AddSingleton<IDistributedCache>(new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));`

            `return services;`
        `}`
    `}`

# 在Startup.cs添加代码
读取 appsettings.json 的配置，初始化 RedisHelper
`var redisOptions = Configuration.GetSection("Redis").Get<RedisOptions>();`
`// redisOptions.Enabled`
`services.AddRedisCache(redisOptions.ConnectionStrings);`

# 增加RedisClient
`/// <summary>`
    `/// Redis 客户端`
    `/// </summary>`
    `public class RedisClient`
    `{`
        `public static string Get(string key)`
        `{`
            `if (RedisHelper.Exists(key))`
            `{`
                `return RedisHelper.Get(key);`
            `}`

            `return null;`
        `}`

        `public static async Task<string> GetAsync(string key)`
        `{`
            `if (await RedisHelper.ExistsAsync(key))`
            `{`
                `var content = await RedisHelper.GetAsync(key);`
                `return content;`
            `}`

            `return null;`
        `}`

        `public static T Get<T>(string key)`
        `{`
            `var value = Get(key);`
            `if (!string.IsNullOrEmpty(value))`
                `return JsonConvert.DeserializeObject<T>(value);`
            `return default(T);`
        `}`

        `public static async Task<T> GetAsync<T>(string key)`
        `{`
            `var value = await GetAsync(key);`
            `if (!string.IsNullOrEmpty(value))`
            `{`
                `return JsonConvert.DeserializeObject<T>(value);`
            `}`

            `return default(T);`
        `}`

        `public static void Set(string key, object data, int expiredSeconds)`
        `{`
            `RedisHelper.Set(key, JsonConvert.SerializeObject(data), expiredSeconds);`
        `}`

        `public static async Task<bool> SetAsync(string key, object data, int expiredSeconds)`
        `{`
            `return await RedisHelper.SetAsync(key, JsonConvert.SerializeObject(data), expiredSeconds);`
        `}`
    `}`
    
添加Model
` public class CacheModel`
    `{`
        `/// <summary>`
        `/// 数据来源`
        `/// </summary>`
        `public string From { get; set; }`

        `/// <summary>`
        `/// `
        `/// </summary>`
        `public string Name { get; set; }`
    `}`

修改视图代码
\src\CSRedisCoreSample\Views\Home\Index.cshtml
<h1 class="display-4">Welcome **@Model?.Name** (**@Model?.From**)</h1>

修改控制器代码
\src\CSRedisCoreSample\Controllers\HomeController.cs
`        public async Task<IActionResult> Index()`
        `{`
            `CacheModel model = new CacheModel();`

            `var key = $"namespace:class:method"; // 根据需求定义key`
            `var value =  await RedisClient.GetAsync(key);`
            `model.From = "来自缓存";`
            `if (value == null)`
            `{`
                `value = "从数据库中读取值";`
                `model.From = "来自数据库";`

                `// 缓存有限期，单位：分钟。`
                `int expiration = 100;`
                `await RedisClient.SetAsync(key, value, expiration);`
            `}`
            `model.Name = value;`

            `return View(model);`
        `}`
        
按F5，运行
在控制器代码 if (value == null) 这里设置一个断点
第一次是读取 数据库中的代码 （这里是伪代码，未做数据库读取操作）
刷新当前页面，再次看断点，是读取缓存的值



            
            

