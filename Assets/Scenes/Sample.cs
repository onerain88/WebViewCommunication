using System;
using UnityEngine;
using Vuplex.WebView;

public class Sample : MonoBehaviour {
    private static readonly string TAP_BILLBOARD_NATIVE_2_JS = "tapBillboardNative2JS";
    private static readonly string TAP_BILLBOARD_JS_2_NATIVE = "tapBillboardJS2Native";

    public CanvasWebViewPrefab canvasWebViewPrefab;

    private NativeJSBridge bridge;

    async void Start() {
        await canvasWebViewPrefab.WaitUntilInitialized();

        bridge = new NativeJSBridge(canvasWebViewPrefab.WebView, TAP_BILLBOARD_NATIVE_2_JS, TAP_BILLBOARD_JS_2_NATIVE);
        
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
        bridge.Notify("submit");
    }
}
