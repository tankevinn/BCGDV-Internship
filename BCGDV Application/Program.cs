using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace BCGDV_Application
{
    class Program
    {
        static void Main(string[] args)
        {
            generateKey(Constant.baseUrl);
            ConsoleKeyInfo command = Console.ReadKey();
            if (command.KeyChar == 'x')
            {
                System.Environment.Exit(1);
            }
        }

        private async static void generateKey(string url)
        {
            string result = "";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        using (HttpContent content = response.Content)
                        {
                            string myContent = await content.ReadAsStringAsync();
                            string key = (string)JObject.Parse(myContent)["key"];

                            if (!string.IsNullOrEmpty(key))
                            {
                                Console.WriteLine(key);
                                string postUrl = Constant.postUrl;
                                postUrl = postUrl + key;
                                result = postForm(postUrl);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    Console.WriteLine(result);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                Console.WriteLine(string.Format("Error has occurred: {0}", exception));
            }
        }

        private static string postForm(string url)
        {
            string result = "";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string name = askInput(Constant.askName);
                string email = askInput(Constant.askEmail);
                string json = string.Format("{{ \"name\": \"{0}\", \"email\": \"{1}\" }}", name, email);
                Debug.Write(json);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            try
            {
                using (var response = httpWebRequest.GetResponse() as HttpWebResponse)
                {
                    if (httpWebRequest.HaveResponse && response != null)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)e.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            string error = reader.ReadToEnd();
                            result = error;
                        }
                    }
                }
            }
            
            return result;
        }

        private static string askInput(string question)
        {
            string input = "";
            do
            {
                Console.WriteLine(question);
                input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine(Constant.emptyInput);
                }
            } while (string.IsNullOrEmpty(input));

            return input;
        }
    }
}
