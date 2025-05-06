using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Global

namespace LoveYuri.Core.Di {
    /// <summary>
    /// 全局di服务
    /// </summary>
    public static class DiService {
        private static IHost _host;
        
        /// <summary>
        /// 全局服务提供
        /// </summary>
        private static IServiceProvider ServiceProvider => _host?.Services;

        /// <summary>
        /// 获取某个类型的依赖服务，如果不存在则报错
        /// </summary>
        public static T GetRequiredService<T>() {
            if (ServiceProvider == null) {
                throw new InvalidOperationException("DI服务尚未初始化");
            }
            
            return ServiceProvider.GetRequiredService<T>();
        }
        
        /// <summary>
        /// 获取某个类型的依赖服务，如果不存在则返回null
        /// </summary>
        public static T GetService<T>() where T : class {
            return ServiceProvider?.GetService<T>();
        }

        /// <summary>
        /// 检查DI服务是否已初始化
        /// </summary>
        public static bool IsInitialized => _host != null;

        /// <summary>
        /// 注册DiService
        /// </summary>
        /// <param name="application">待注册的application</param>
        /// <param name="register">服务注册函数</param>
        public static void RegisterDiService(this Application application, Action<IServiceCollection> register) {
            _host = Host
                .CreateDefaultBuilder()
                .ConfigureLogging(logging => logging.ClearProviders()) // 移除所有日志提供程序
                .ConfigureServices((_, service) => {
                    // 注册服务
                    register.Invoke(service);
                }).Build();
            
            // 启动时启动服务
            application.Startup += (_, __) => _host.Start();
            
            // 关闭时停止服务
            application.Exit += (_, __) => _host.Dispose();
        }
    }
}