using System.Text;

namespace RayBlast;

public class RayBlastHttp : IDisposable {
    public static readonly Dictionary<int, HttpClient> HTTP_CLIENTS = new() {
        {
            30, new HttpClient {
                Timeout = TimeSpan.FromSeconds(30)
            }
        }
    };

    private HttpRequestMessage? requestMessage;
    private Task<HttpResponseMessage>? responseTask;
    private Dictionary<string, string>? responseHeaders;
    private int timeout = 30;
    private HttpClient httpClient;
    private CancellationTokenSource responseCancellationToken = new();

    protected RayBlastHttp() {
        httpClient = HTTP_CLIENTS[timeout];
    }

    public int Timeout {
        get => timeout;
        set {
            timeout = value;
            if(!HTTP_CLIENTS.ContainsKey(value)) {
                HTTP_CLIENTS[value] = new HttpClient {
                    Timeout = TimeSpan.FromSeconds(value)
                };
            }
            httpClient = HTTP_CLIENTS[value];
        }
    }

    public string URL { get; private set; } = "";
    public virtual bool IsDone => responseTask?.IsCompleted ?? false;
    private HttpResponseMessage? SuccessfulResponse =>
        responseTask?.IsCompletedSuccessfully ?? throw new InvalidOperationException("Request not sent") ? responseTask.Result : null;
    //TODO_AFTER: Get progress from HttpClient
    public virtual float UploadProgress => IsDone ? 1f : 0f;
    public virtual float DownloadProgress => IsDone ? 1f : 0f;
    public virtual ulong UploadedBytes => IsDone ? 1U : 0U;
    public virtual ulong DownloadedByteCount => IsDone ? 1U : 0U;
    public virtual string DownloadedText => SuccessfulResponse?.Content.ReadAsStringAsync().Result ?? "";
    public virtual byte[] DownloadedBytes => SuccessfulResponse?.Content.ReadAsByteArrayAsync().Result ?? [];
    public long ResponseCode {
        get {
            Task<HttpResponseMessage>? task = responseTask;
            if(task == null)
                throw new InvalidOperationException("Request not sent");
            if(!IsDone)
                return 0;
            HttpResponseMessage? successfulResponse = SuccessfulResponse;
            if(successfulResponse != null)
                return (long)successfulResponse.StatusCode;
            try {
                return (long)task.Result.StatusCode;
            }
            catch {
                return -1;
            }
        }
    }
    public virtual Result State =>
        ResponseCode switch {
            0 => Result.InProgress,
            < 200 => Result.ConnectionError,
            < 300 => Result.Success,
            _ => Result.ProtocolError
        };
    public virtual string? Error {
        get {
            Task<HttpResponseMessage> task = responseTask ?? throw new InvalidOperationException("Request not sent");
            try {
                if(task.IsCompletedSuccessfully)
                    return task.Result.IsSuccessStatusCode ? null : task.Result.Content.ReadAsStringAsync().Result;
                if(IsDone) {
                    HttpResponseMessage responseMessage = task.Result;
                    return responseMessage.IsSuccessStatusCode ? null : responseMessage.Content.ReadAsStringAsync().Result;
                }
                return null;
            }
            catch(Exception e) {
                return e.Message;
            }
        }
    }
    public Dictionary<string, string>? ResponseHeaders {
        get {
            if(responseTask == null)
                throw new InvalidOperationException("Request not sent");
            if(responseHeaders == null) {
                HttpResponseMessage? successfulResponse = SuccessfulResponse;
                if(successfulResponse != null) {
                    responseHeaders = new Dictionary<string, string>();
                    foreach(KeyValuePair<string, IEnumerable<string>> kvp in successfulResponse.Headers) {
                        responseHeaders[kvp.Key] = kvp.Value.Join(", ");
                    }
                    foreach(KeyValuePair<string, IEnumerable<string>> kvp in successfulResponse.Content.Headers) {
                        responseHeaders[kvp.Key] = kvp.Value.Join(", ");
                    }
                }
            }
            return responseHeaders;
        }
    }
    public virtual bool WasSent => responseTask != null;

    public void SetRequestHeader(string header, string value) {
        if(requestMessage == null)
            throw new InvalidOperationException("Request not set up");
        requestMessage.Headers.Add(header, value);
    }

    public virtual void SendRequest() {
        if(requestMessage == null)
            throw new InvalidOperationException("Request not set up");
        if(responseTask != null)
            throw new InvalidOperationException("Request already sent");
        //TODO: Use HttpCompletionOption.ResponseHeadersRead and read the content as it downloads
        responseTask = httpClient.SendAsync(requestMessage, responseCancellationToken.Token);
    }

    public static RayBlastHttp CreateGet(string url) {
        return new RayBlastHttp {
            URL = url, requestMessage = new HttpRequestMessage(HttpMethod.Get, url)
        };
    }

    public static RayBlastHttp CreatePostJson(string url, string jsonBody) {
        return new RayBlastHttp {
            URL = url,
            requestMessage = new HttpRequestMessage(HttpMethod.Post, url) {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
                Headers = {
                    {
                        "Accept", "application/json"
                    }
                }
            }
        };
    }

    public virtual void Abort() {
        if(responseTask != null)
            responseCancellationToken.Cancel();
    }

    public enum Result {
        InProgress,
        Success,
        /// <summary>
        ///   <para>Failed to communicate with the server. For example, the request couldn't connect or it could not establish a secure channel.</para>
        /// </summary>
        ConnectionError,
        /// <summary>
        ///   <para>The server returned an error response. The request succeeded in communicating with the server, but received an error as defined by the connection protocol.</para>
        /// </summary>
        ProtocolError,
        /// <summary>
        ///   <para>Error processing data. The request succeeded in communicating with the server, but encountered an error when processing the received data. For example, the data was corrupted or not in the correct format.</para>
        /// </summary>
        DataProcessingError
    }

    protected virtual void Dispose(bool disposing) {
        if(disposing) {
            if(responseTask != null) {
                responseCancellationToken.Cancel();
                try {
                    responseTask.Wait();
                }
                catch {
                    // ignored
                }
            }
            requestMessage?.Dispose();
            responseTask?.Dispose();
            responseCancellationToken.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
