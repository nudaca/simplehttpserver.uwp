using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WebServer
{
    public sealed class HttpServer : IDisposable
    {
        private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
        {
            #region extension to MIME type list
            {".asf", "video/x-ms-asf"},
            {".asx", "video/x-ms-asf"},
            {".avi", "video/x-msvideo"},
            {".bin", "application/octet-stream"},
            {".cco", "application/x-cocoa"},
            {".crt", "application/x-x509-ca-cert"},
            {".css", "text/css"},
            {".deb", "application/octet-stream"},
            {".der", "application/x-x509-ca-cert"},
            {".dll", "application/octet-stream"},
            {".dmg", "application/octet-stream"},
            {".ear", "application/java-archive"},
            {".eot", "application/octet-stream"},
            {".exe", "application/octet-stream"},
            {".flv", "video/x-flv"},
            {".gif", "image/gif"},
            {".hqx", "application/mac-binhex40"},
            {".htc", "text/x-component"},
            {".htm", "text/html"},
            {".html", "text/html"},
            {".ico", "image/x-icon"},
            {".img", "application/octet-stream"},
            {".iso", "application/octet-stream"},
            {".jar", "application/java-archive"},
            {".jardiff", "application/x-java-archive-diff"},
            {".jng", "image/x-jng"},
            {".jnlp", "application/x-java-jnlp-file"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".js", "application/x-javascript"},
            {".mml", "text/mathml"},
            {".mng", "video/x-mng"},
            {".mov", "video/quicktime"},
            {".mp3", "audio/mpeg"},
            {".mp4", "video/mp4"},
            {".mpeg", "video/mpeg"},
            {".mpg", "video/mpeg"},
            {".msi", "application/octet-stream"},
            {".msm", "application/octet-stream"},
            {".msp", "application/octet-stream"},
            {".pdb", "application/x-pilot"},
            {".pdf", "application/pdf"},
            {".pem", "application/x-x509-ca-cert"},
            {".pl", "application/x-perl"},
            {".pm", "application/x-perl"},
            {".png", "image/png"},
            {".prc", "application/x-pilot"},
            {".ra", "audio/x-realaudio"},
            {".rar", "application/x-rar-compressed"},
            {".rpm", "application/x-redhat-package-manager"},
            {".rss", "text/xml"},
            {".run", "application/x-makeself"},
            {".sea", "application/x-sea"},
            {".shtml", "text/html"},
            {".sit", "application/x-stuffit"},
            {".swf", "application/x-shockwave-flash"},
            {".tcl", "application/x-tcl"},
            {".tk", "application/x-tcl"},
            {".txt", "text/plain"},
            {".war", "application/java-archive"},
            {".wbmp", "image/vnd.wap.wbmp"},
            {".wmv", "video/x-ms-wmv"},
            {".xml", "text/xml"},
            {".xpi", "application/x-xpinstall"},
            {".zip", "application/zip"},
            #endregion
        };

        public StorageFile File { get; set; }
        private const uint BufferSize = 2 << 20;
        private int port = 8080;
        private readonly StreamSocketListener listener;
        private AppServiceConnection appServiceConnection;

        public HttpServer(int serverPort, AppServiceConnection connection) : this(serverPort)
        {
            appServiceConnection = connection;
        }

        public HttpServer(int serverPort)
        {
            listener = new StreamSocketListener();
            port = serverPort;
            listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
        }

        public async void StartServer() => await listener.BindServiceNameAsync(port.ToString());
        public void Dispose() => listener.Dispose();

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            // this works for text only
            string request = string.Empty;
            using (IInputStream input = socket.InputStream)
            {
                byte[] data = new byte[BufferSize];
                IBuffer buffer = data.AsBuffer();
                uint dataRead = BufferSize;
                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    request = Encoding.UTF8.GetString(data, 0, data.Length);
                    dataRead = buffer.Length;
                }
            }
            System.Diagnostics.Debug.WriteLine(request);

            using (IOutputStream output = socket.OutputStream)
            {
                string requestMethod = request.Split('\n')[0];
                string[] requestParts = requestMethod.Split(' ');

                if (requestParts[0] == "GET")
                    await WriteResponseAsync(requestParts[1], output);
                else
                    throw new InvalidDataException("HTTP method not supported: "
                                                   + requestParts[0]);
            }
        }

        private async Task WriteResponseAsync(string request, IOutputStream os)
        {
            using (Stream resp = os.AsStreamForWrite())
            {
                using (Stream sourceStream = await File.OpenStreamForWriteAsync())
                {
                    string mime;
                    string header = string.Format("HTTP/1.1 200 OK\r\n" +
                                      $"Date: {DateTime.Now.ToString("R")}\r\n" +
                                      "Server: MeoGoEmbedded/1.0\r\n" +
                                      "Content-Length: {0}\r\n" +
                                      "Content-Type: {1}\r\n" +
                                      "Connection: close\r\n\r\n",
                                      sourceStream.Length,
                                      _mimeTypeMappings.TryGetValue(File.FileType, out mime) ? mime : "application/octet-stream");
                    byte[] headerArray = Encoding.UTF8.GetBytes(header);
                    await resp.WriteAsync(headerArray, 0, headerArray.Length);
                    var b = new byte[1 << 16]; // 64k
                    int count = 0;
                    while ((count = sourceStream.Read(b, 0, b.Length)) > 0)
                    {
                        await resp.WriteAsync(b, 0, count);
                    }
                    await resp.FlushAsync();
                };
            }
        }
    }
}
