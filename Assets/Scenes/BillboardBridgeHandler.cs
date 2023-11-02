using System;
using System.Threading.Tasks;

/// <summary>
/// 模拟业务模块
/// </summary>
public class BillboardBridgeHandler {
    private readonly WebViewBridge bridge;

    public BillboardBridgeHandler(WebViewBridge bridge) {
        this.bridge = bridge;
        // 也可以定义业务模块的响应

    }

    public async Task<int> GetRedDotsCount() {
        object result = await bridge.Call("getRedDotsCount");
        return Convert.ToInt32(result);
    }

    public void Submit() {
      bridge.Call("submit");
    }
}