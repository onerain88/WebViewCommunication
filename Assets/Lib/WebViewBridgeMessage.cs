using System;
using Newtonsoft.Json;

public class WebViewBridgeMessage {
    /// <summary>
    /// 调用方法名
    /// </summary>
    [JsonProperty("method")]
    public string Method { get; set; }

    /// <summary>
    /// 传递参数
    /// </summary>
    [JsonProperty("data")]
    public object Data { get; set; }

    /// <summary>
    /// 请求 id
    /// </summary>
    [JsonProperty("requestId")]
    public int? RequestId { get; set; }

    /// <summary>
    /// 应答 id
    /// </summary>
    [JsonProperty("responseId")]
    public int? ResponseId { get; set; }

    /// <summary>
    /// 创建调用消息
    /// </summary>
    /// <param name="method"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static WebViewBridgeMessage NewCall(int reqId, string method, object data = null) {
        WebViewBridgeMessage message = new WebViewBridgeMessage {
            RequestId = reqId,
            Method = method,
        };
        if (data != null) {
            message.Data = data;
        }
        return message;
    }

    /// <summary>
    /// 创建响应消息
    /// </summary>
    /// <param name="reqId"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static WebViewBridgeMessage NewResponse(int reqId, object data = null) {
        WebViewBridgeMessage message = new WebViewBridgeMessage {
            ResponseId = reqId,
        };
        if (data != null) {
            message.Data = data;
        }
        return message;
    }

    /// <summary>
    /// 是否是请求
    /// </summary>
    [JsonIgnore]
    public bool IsRequest => RequestId != null;
    /// <summary>
    /// 是否是应答
    /// </summary>
    [JsonIgnore]
    public bool IsResponse => ResponseId != null;
}
