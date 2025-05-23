using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LoveYuri.Utils;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace LoveYuri.Core.Service {
    /// <summary>
    /// 收到消息时的回调委托
    /// </summary>
    /// <param name="message">接收到的消息内容</param>
    public delegate void UdpMessageEventHandler(string message);

    /// <summary>
    /// Udp基础服务
    /// </summary>
    public class UdpService : IDisposable {
        /// <summary>
        /// 远程ip
        /// </summary>
        private readonly IPEndPoint remoteEndPoint;

        /// <summary>
        /// udp客户端
        /// </summary>
        private UdpClient udpClient;

        /// <summary>
        /// 标记是否已释放资源
        /// </summary>
        private volatile bool disposed;

        /// <summary>
        /// 是否正在监听
        /// </summary>
        private volatile bool isListening;

        /// <summary>
        /// 监听取消token
        /// </summary>
        private CancellationTokenSource receiveCts;

        /// <summary>
        /// 是否启用自动重连
        /// </summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>
        /// 收到消息事件回调
        /// </summary>
        public event UdpMessageEventHandler HasReceiveMsg;

        /// <summary>
        /// 发送消息回调
        /// </summary>
        public event UdpMessageEventHandler HasSendMsg;

        /// <summary>
        /// service的唯一key
        /// </summary>
        public string Key => $"{remoteEndPoint.Address}:{remoteEndPoint.Port}";

        /// <summary>
        /// 目标端口
        /// </summary>
        public int Port => remoteEndPoint.Port;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => !disposed && udpClient != null;

        /// <summary>
        /// 初始化UDP服务并指定远程端点
        /// </summary>
        /// <param name="remoteIp">远程IP地址</param>
        /// <param name="remotePort">远程端口</param>
        public UdpService(string remoteIp, int remotePort) {
            InitializeUdpClient();
            Log.Info($"连接至 {remoteIp}:{remotePort} 的UDP服务已在 {udpClient.Client.LocalEndPoint} 启动");
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~UdpService() => Dispose(false);

        /// <summary>
        /// 初始化UDP客户端
        /// </summary>
        private void InitializeUdpClient() {
            try {
                udpClient = new UdpClient(0);
            } catch (Exception ex) {
                Log.Error($"初始化UDP客户端失败: {ex.Message}");
                throw;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <returns>是否发送成功</returns>
        public bool Send(string message) {
            if (disposed || udpClient == null) {
                Log.Error($"尝试在已释放的UDP服务上发送消息: {Key}");
                return false;
            }

            try {
                byte[] data = Encoding.ASCII.GetBytes(message);
                int res = udpClient.Send(data, data.Length, remoteEndPoint);

                if (res != data.Length) {
                    return false;
                }
                HasSendMsg?.Invoke(message);
                return true;
            } catch (Exception ex) {
                Log.Error($"发送消息时发生错误({Port}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 开始持续监听消息,直到服务停止
        /// 收到消息会调用 _receiveMsgDelegate 事件
        /// </summary>
        public void StartListening() {
            // 如果已释放，不要开始监听
            if (isListening || disposed) return;

            isListening = true;
            receiveCts = new CancellationTokenSource();

            Task.Run(async () => {
                try {
                    Log.Info($"开始监听: {Key}");
                    // 检查是否已释放
                    while (!receiveCts.Token.IsCancellationRequested && !disposed) {
                        if (!IsConnected) break;
                        var result = await udpClient.ReceiveAsync();
                        if (!IsConnected) break;

                        // 处理接收到的消息-捕获异常，防止监听中断
                        string message = Encoding.ASCII.GetString(result.Buffer);
                        try {
                            HasReceiveMsg?.Invoke(message);
                        } catch (Exception exception) {
                            Log.Error($"处理数据({message})回调发生异常: {exception.Message}");
                        }
                    }
                } catch (OperationCanceledException) {
                    // 取消操作，正常退出
                    Log.Info($"{Key}取消监听, 正在退出.....");
                } catch (ObjectDisposedException) {
                } catch (Exception ex) {
                    Log.Error($"监听过程中发生错误({Port}): {ex.Message}");

                    if (AutoReconnect) {
                        _ = Task.Run(() => ReConnect());
                    }
                } finally {
                    isListening = false;
                }
            }, receiveCts.Token);
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        public void StopListening() {
            try {
                receiveCts?.Cancel(true);

                if (udpClient != null) {
                    udpClient.Close();
                    udpClient = null;
                }

                Log.Info($"正在停止监听: {Key}");
            } catch (Exception ex) {
                Log.Error($"停止监听时发生错误({Port}): {ex.Message}");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否为显式释放</param>
        protected virtual void Dispose(bool disposing) {
            if (disposed) return;

            if (disposing) {
                StopListening();

                receiveCts?.Dispose();

                if (udpClient != null) {
                    udpClient.Dispose();
                    udpClient = null;
                }

                HasReceiveMsg = null;
                HasSendMsg = null;
            }

            disposed = true;
        }

        /// <summary>
        /// 重新连接UDP服务
        /// </summary>
        /// <param name="needReconnect">是否需要重连 默认会重连</param>
        /// <returns>重连是否成功</returns>
        public async Task ReConnect(bool needReconnect = true) {
            if (disposed) {
                Log.Error($"尝试在已释放的UDP服务上重新连接: {Key}");
                return;
            }

            // 关闭监听
            StopListening();

            // 等待服务关闭
            await Task.Delay(300);

            // 创建新的UDP客户端
            InitializeUdpClient();

            // 如果之前在监听，则重新开始监听
            if (needReconnect) {
                StartListening();
            }

            Log.Info($"{Key} 已重新在 {udpClient?.Client.LocalEndPoint} 启动!");
        }
    }
}
