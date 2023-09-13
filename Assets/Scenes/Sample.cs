using System;
using UnityEngine;
using Vuplex.WebView;

public class Sample : MonoBehaviour {
    private static readonly string TAP_BILLBOARD = "tapBillboard";

    public CanvasWebViewPrefab canvasWebViewPrefab;

    private NativeJSBridge bridge;

    async void Start() {
        canvasWebViewPrefab.RemoteDebuggingEnabled = true;
        await canvasWebViewPrefab.WaitUntilInitialized();

        bridge = new NativeJSBridge(canvasWebViewPrefab.WebView, TAP_BILLBOARD);
        
        // 通知
        bridge.Define("clickButton", data => {
            Debug.Log(data);
        });
        // JS 请求
        bridge.Define("getPlatform", () => SystemInfo.operatingSystem);

        // 直接调用，且有返回值
        int result = Convert.ToInt32(await bridge.Call("getValue"));
        Debug.Log($"result: {result}");
    }

    public void OnSubmitButtonClicked() {
        _ = bridge.Call("submit");
    }
}
