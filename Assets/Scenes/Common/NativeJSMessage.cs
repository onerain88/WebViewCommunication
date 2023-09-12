﻿using System;
using Newtonsoft.Json;

public class NativeJSMessage {
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
    public static NativeJSMessage NewCall(int reqId, string method, object data = null) {
        NativeJSMessage message = new NativeJSMessage {
            RequestId = reqId,
            Method = method,
        };
        if (data != null) {
            message.Data = data;
        }
        return message;
    }

    public static NativeJSMessage NewNotification(string method, object data = null) {
        NativeJSMessage message = new NativeJSMessage {
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
    public static NativeJSMessage NewResponse(int reqId, object data = null) {
        NativeJSMessage message = new NativeJSMessage {
            ResponseId = reqId,
        };
        if (data != null) {
            message.Data = data;
        }
        return message;
    }

    /// <summary>
    /// 是否是通知
    /// </summary>
    [JsonIgnore]
    public bool IsNotification => RequestId == null && ResponseId == null;
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