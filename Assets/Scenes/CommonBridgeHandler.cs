using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuplex.WebView;

/// <summary>
/// 公共模块，可能被复用
/// </summary>
public class CommonBridgeHandler {
    private readonly WebViewBridge bridge;

    public CommonBridgeHandler(WebViewBridge bridge) {
        this.bridge = bridge;
        bridge.Define("getPlatform", () => SystemInfo.operatingSystem);
    }
}
