using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView;
using Newtonsoft.Json;
using TMPro;

public class WebViewBridge {
    private readonly static string POST_MESSAGE_FUNC = "postMessage";
    private readonly static string ON_MESSAGE_FUNC = "onMessage";

    private readonly IWebView webView;

    private readonly Dictionary<string, Func<object, Task<object>>> defines;

    private readonly Dictionary<int, TaskCompletionSource<object>> requestTasks;

    private int requestId;

    public WebViewBridge(IWebView webView) {
        this.webView = webView;

        defines = new Dictionary<string, Func<object, Task<object>>>();

        requestTasks = new Dictionary<int, TaskCompletionSource<object>>();

        requestId = 0;

        webView.MessageEmitted += MessageEmitted;

        webView.ExecuteJavaScript($@"
            if (!window.tapBridge) {{
                window.tapBridge = {{}};
            }}
            if (!window.tapBridge.{POST_MESSAGE_FUNC}) {{
                window.vuplex.addEventListener('message', function (event) {{
                    window.tapBridge.{ON_MESSAGE_FUNC}(event.data)
                }});
                window.tapBridge.{POST_MESSAGE_FUNC} = function(message) {{ 
                    window.vuplex.postMessage(message); 
                }}
            }}");
    }

    public void Define(string method, Action action) {
        Define(method, (_) => {
            action.Invoke();
            return Task.FromResult<object>(null);
        });
    }

    public void Define(string method, Action<object> action) {
        Define(method, (data) => {
            action.Invoke(data);
            return Task.FromResult<object>(null);
        });
    }

    public void Define(string method, Func<object> func) {
        Define(method, (data) => {
            return Task.FromResult(func.Invoke());
        });
    }

    public void Define(string method, Func<object, object> func) {
        Define(method, (data) => {
            return Task.FromResult(func.Invoke(data));
        });
    }

    public void Define(string method, Func<object, Task> func) {
        Define(method, async data => {
            await func.Invoke(data);
            return null;
        });
    }

    public void Define(string method, Func<object, Task<object>> callback) {
        defines.Add(method, callback);
    }

    public Task<object> Call(string method, object data = null) {
        WebViewBridgeMessage call = WebViewBridgeMessage.NewCall(++requestId, method, data);

        TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
        requestTasks.Add((int)call.RequestId, tcs);
        Invoke(call);

        return tcs.Task;
    }

    private void Invoke(WebViewBridgeMessage message) {
        JsonSerializerSettings settings = new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore
        };
        string json = JsonConvert.SerializeObject(message, settings);
        Debug.Log($"=> {json}");
        webView.PostMessage(json);
    }

    private void MessageEmitted(object sender, EventArgs<string> args) {
        Debug.Log($"<= {args.Value}");
        WebViewBridgeMessage message = JsonConvert.DeserializeObject<WebViewBridgeMessage>(args.Value);
        if (message.IsResponse) {
            // 应答
            Debug.Log("Response");
            HandleResponse(message);
        } else if (message.IsRequest) {
            // 请求
            Debug.Log("Request");
            HandleRequest(message);
        } else {
            Debug.Log($"Invalid message: {args.Value}");
        }
    }

    private async void HandleRequest(WebViewBridgeMessage message) {
        if (defines.TryGetValue(message.Method, out Func<object, Task<object>> func)) {
            object result = await func(message.Data);
            Invoke(WebViewBridgeMessage.NewResponse((int)message.RequestId, result));
        }
    }

    private void HandleResponse(WebViewBridgeMessage message) {
        if (requestTasks.TryGetValue((int)message.ResponseId, out TaskCompletionSource<object> tcs)) {
            tcs.TrySetResult(message.Data);
        } else {
            Debug.LogError($"Miss response: {message.ResponseId}");
        }
    }
}