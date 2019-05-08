using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PG_Test1.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;

namespace PG_Test1
{
    class OCR : IOCR
    {
        // Replace <Subscription Key> with your valid subscription key.
        const string subscriptionKey = "a2597bd5e4664e70b75212c97a052dac";
        const string uriBase = "https://westus.api.cognitive.microsoft.com/vision/v2.0/read/core/asyncBatchAnalyze";
        const int TimeOut = 30;  //Waiting time for Recognizing text 
        public JObject getJSON(string imageFilePath)
        {
            if (File.Exists(imageFilePath))
            {
                // Call the REST API method.
                return MakeOCRRequest(imageFilePath).Result;
            }
            else
            {
                return new JObject(new JProperty("Error", "File not exist"));
            }
        }


        private async Task<JObject> MakeOCRRequest(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. 
                string requestParameters = "mode=Handwritten";

                // Assemble the URI for the REST API method.
                string uri = uriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Two REST API methods are required to extract handwritten text.
                // One method to submit the image for processing, the other method
                // to retrieve the text found in the image.

                // operationLocation stores the URI of the second REST API method,
                // returned by the first REST API method.
                string operationLocation;

                // Read the contents of the specified local image
                // into a byte array.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Asynchronously call the REST API method.
                    response = await client.PostAsync(uri, content);
                }
                // The response header for the Batch Read method contains the URI
                // of the second method, Read Operation Result, which
                // returns the results of the process in the response body.
                // The Batch Read operation does not return anything in the response body.
                if (response.IsSuccessStatusCode)
                    operationLocation =
                        response.Headers.GetValues("Operation-Location").FirstOrDefault();
                else
                {
                    // Display the JSON error data.
                    string errorString = await response.Content.ReadAsStringAsync();
                    return new JObject(new JProperty("Error",JToken.Parse(errorString).ToString()));
                }
                // If the first REST API method completes successfully, the second 
                // REST API method retrieves the text written in the image.
                //
                // Note: The response may not be immediately available. Handwriting
                // recognition is an asynchronous operation that can take a variable
                // amount of time depending on the length of the handwritten text.
                // You may need to wait or retry this operation.
                //
                // This example checks once per second for ten seconds.
                string contentString;
                int i = 0;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    response = await client.GetAsync(operationLocation);
                    contentString = await response.Content.ReadAsStringAsync();
                    ++i;
                }
                while (i < TimeOut && contentString.IndexOf("\"status\":\"Succeeded\"") == -1);

                if (i == TimeOut && contentString.IndexOf("\"status\":\"Succeeded\"") == -1)
                {
                    return new JObject(new JProperty("Error", "Timeout Error"));
                }

                // Display the JSON response.
               return JObject.Parse(contentString);
            }
            catch (Exception e)
            {
                return new JObject(new JProperty("Exception Error",e.ToString()));
            }
        }
        private byte[] GetImageAsByteArray(string imageFilePath)
        {
            // Open a read-only file stream for the specified file.
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the file's contents into a byte array.
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        public string getProperty(JObject jObject)
        {
            string removeProperty = "words";
            string getProperty = "text";
            RemoveJsonProperty(jObject, removeProperty);
            var list = GetJsonProperty(jObject, getProperty);
            string Text = "";
            if (list.Count == 1)
            {
                Text = list[0].ToString();
            }
            else
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (j == list.Count - 1)
                    {
                        Text += list[j].ToString();
                    }
                    else
                    {
                        Text += list[j].ToString() + "\r\n";
                    }
                }
            }
            return Text;
        }

        private List<string> GetJsonProperty(JObject jObject, string property)
        {
            List<JProperty> valueList = jObject.Descendants().OfType<JProperty>().Where(attr => attr.Name.Equals(property)).ToList();
            return valueList.Select(c => (Convert.ToString(c.Value).Replace("{", "").Replace("}", "").Replace("[", "").Replace("]", "").Replace("\"", "").Replace("^%^", ""))).ToList();
        }
        private void RemoveJsonProperty(JObject jObject, string property)
        {
            jObject.Descendants().OfType<JProperty>().Where(attr => attr.Name.StartsWith(property)).ToList().ForEach(attr => attr.Remove());
        }
    }
}
