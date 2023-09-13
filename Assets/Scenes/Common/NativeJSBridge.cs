using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView;
using Newtonsoft.Json;
using TMPro;

public class NativeJSBridge {
    private readonly string native2JSFunc;
    private readonly IWebView webView;

    private readonly Dictionary<string, Func<object, Task<object>>> defines;

    private readonly Dictionary<int, TaskCompletionSource<object>> requestTasks;

    private int requestId;

    public NativeJSBridge(IWebView webView, string native2JSFunc, string js2NativeFunc) {
        this.webView = webView;
        this.native2JSFunc = native2JSFunc;

        defines = new Dictionary<string, Func<object, Task<object>>>();

        requestTasks = new Dictionary<int, TaskCompletionSource<object>>();

        requestId = 0;

        webView.MessageEmitted += MessageEmitted;

        webView.ExecuteJavaScript($@"window.{js2NativeFunc} = function(message) {{ 
            window.vuplex.postMessage(message); 
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

    public async Task<object> Call(string method, object data = null) {
        NativeJSMessage call = NativeJSMessage.NewCall(++requestId, method, data);

        TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
        requestTasks.Add((int)call.RequestId, tcs);
        await Invoke(call);

        return await tcs.Task;
    }

    private Task Invoke(NativeJSMessage message) {
        JsonSerializerSettings settings = new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore
        };
        string json = JsonConvert.SerializeObject(message, settings);
        Debug.Log($"=> {json}");
        string callString = $"window.{native2JSFunc}('{json}')";
        return webView.ExecuteJavaScript(callString);
    }

    private async void MessageEmitted(object sender, EventArgs<string> args) {
        Debug.Log($"<= {args.Value}");
        NativeJSMessage message = JsonConvert.DeserializeObject<NativeJSMessage>(args.Value);
        if (message.IsResponse) {
            // 应答
            Debug.Log("Response");
            if (requestTasks.TryGetValue((int)message.ResponseId, out TaskCompletionSource<object> tcs)) {
                tcs.TrySetResult(message.Data);
            } else {
                Debug.LogError($"Miss response: {message.ResponseId}");
            }
        } else if (message.IsRequest) {
            // 请求
            Debug.Log("Request");
            if (defines.TryGetValue(message.Method, out Func<object, Task<object>> func)) {
                object result = await func(message.Data);
                await Invoke(NativeJSMessage.NewResponse((int)message.RequestId, result));
            }
        } else {
            Debug.Log($"Invalid message: {args.Value}");
        }
    }
}