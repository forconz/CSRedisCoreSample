using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CSRedisCoreSample.Models;

namespace CSRedisCoreSample.Controllers
{
    public class CacheModel
    {
        /// <summary>
        /// 数据来源
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
    }

    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            CacheModel model = new CacheModel();

            var key = $"namespace:class:method"; // 根据需求定义key
            var value =  await RedisClient.GetAsync(key);
            model.From = "来自缓存";
            if (value == null)
            {
                value = "从数据库中读取值";
                model.From = "来自数据库";

                // 缓存有限期，单位：分钟。
                int expiration = 100;
                await RedisClient.SetAsync(key, value, expiration);
            }
            model.Name = value;

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
