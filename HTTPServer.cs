using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MerakiLocationVisualizer
{
    public class HTTPServer
    {
        private const int HandlerThread = 2;
        private readonly HttpListener listener;

        public HTTPServer(HttpListener listener, string url)
        {
            this.listener = listener;
            listener.Prefixes.Add(url);
        }

        private void ProcessRequestHandler(Task<HttpListenerContext> result)
        {
            var context = result.Result;
            HttpListenerRequest typeoHTTPRrequest = context.Request;
            HttpListenerResponse serverResponse = context.Response;

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //
            // INSERT YOUR KEY AND SECRET BELOW
            //
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            string validationResponse = "the.string.you.copied.from.the.meraki.portal";
            string MySharedSecret = "the.secret.you.configured.in.the.meraki.portal";

            if (!listener.IsListening)
                return;

            // Start new listener which replace this
            listener.GetContextAsync().ContinueWith(ProcessRequestHandler);

            // Read request
            string request = new StreamReader(context.Request.InputStream).ReadToEnd();

            // the only GET we care about is the Meraki backend verifying this server as
            // a POST target, so we send the expected response below.  Firewall yourself
            // appropriately...
                        if (typeoHTTPRrequest.HttpMethod == "GET")
            {
                Console.WriteLine("SUCCESSFUL GET");
                var responseBytes = System.Text.Encoding.UTF8.GetBytes(validationResponse);
                serverResponse.ContentLength64 = responseBytes.Length;
                var output = serverResponse.OutputStream;
                output.WriteAsync(responseBytes, 0, responseBytes.Length);
                output.Close();
            }
            else if(typeoHTTPRrequest.HttpMethod == "POST")
            {
                Console.WriteLine("SUCCESSFUL POST");

                // the v2 API will return a non-rfc "NaN" string where you've been
                // told to expect a float/double.  The following prevents that from
                // nuking everything.  Pre-req is .NET 5.0

                var jsonOptions = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals 
                    // | JsonNumberHandling.WriteAsString
                };

                DevicesSeen observationReport = new DevicesSeen();
                observationReport = JsonSerializer.Deserialize<DevicesSeen>(request, jsonOptions);

                // is the shared secret a match
                if (observationReport.Secret != MySharedSecret)
                {
                    Console.WriteLine("Shared Secret does not match.  Discarding received data.");
                    return;
                }

                // for debug
                //DumpAsYaml(observationReport);

                // Deal with the POSTed data
                DataParser.ProcessData(observationReport);

            }
        }

        public void Start()
        {
            if (listener.IsListening)
                return;

            listener.Start();

            for (int i = 0; i < HandlerThread; i++)
            {
                listener.GetContextAsync().ContinueWith(ProcessRequestHandler);
            }
        }

        public void Stop()
        {
            if (listener.IsListening)
                listener.Stop();
        }

        // object dumper for debug/dev
        public static void DumpAsYaml(object obj)
        {
            var stringBuilder = new System.Text.StringBuilder();
            var serializer = new YamlDotNet.Serialization.Serializer();
            serializer.Serialize(new System.CodeDom.Compiler.IndentedTextWriter(new StringWriter(stringBuilder)), obj);
            Console.WriteLine(stringBuilder);
        }


    }
}
