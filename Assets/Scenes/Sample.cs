using System;
using UnityEngine;
using Vuplex.WebView;

public class Sample : MonoBehaviour {
    private static readonly string TAP_BILLBOARD = "tapBillboard";

    public CanvasWebViewPrefab canvasWebViewPrefab;

    private WebViewBridge bridge;
    private CommonBridgeHandler commonBridgeHandler;
    private BillboardBridgeHandler billboardBridgeHandler;

    async void Start() {
        canvasWebViewPrefab.RemoteDebuggingEnabled = true;
        await canvasWebViewPrefab.WaitUntilInitialized();

        bridge = new WebViewBridge(canvasWebViewPrefab.WebView);
        commonBridgeHandler = new CommonBridgeHandler(bridge);
        billboardBridgeHandler = new BillboardBridgeHandler(bridge);

        int result = await billboardBridgeHandler.GetRedDotsCount();
        Debug.Log($"The count of red dots: {result}");
    }

    public void OnSubmitButtonClicked() {
        billboardBridgeHandler.Submit();
    }
}
