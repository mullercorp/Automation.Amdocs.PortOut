using System;
using System.Linq;
using System.Net;
using Automation.Amdocs.Shared.Uhelp;
using AutomationFramework;
using AutomationFramework.DAO;
using Newtonsoft.Json;
using RestSharp;

namespace PortOutApiTool
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Start");

            Console.WriteLine("BaseUrl?");
            var baseUrl = Console.ReadLine();
            
            Console.WriteLine("Call variant?");
            var variant = Console.ReadLine();
            
            
            Console.WriteLine("Msisdn?");
            var mobileNr = Console.ReadLine();

            
            
            var apiCred = CredentialsDAO.GetAppCredentials(0, "Uhelp_RestApi");
            Console.WriteLine($"Application: {apiCred.Application}");
            //var baseUrl = AssetsDAO.GetAssetByName("Uhelp_RestApi_PortingDataOldest101Url");
            Console.WriteLine($"baseUrl: {baseUrl}");
            
                //var portingDataOldest101 = new UhelpBase().GePortingDataOldest101ViaRestApi(baseUrl, mobileNr, apiCred.Password, "prd");
                            var portingDataOldest101 = new PortingDataOldest101();

            IRestResponse response = null;
            try
            {
                Console.WriteLine("Trying to fetch the data...");
                
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                var client = new RestClient(baseUrl);
                RestRequest request = new RestRequest("{MobileNumber}/"+ variant, Method.GET);
                
                request.AddHeader("ApiKey", apiCred.Password);
                request.AddHeader("Env", "prd");

                request.AddParameter("MobileNumber", mobileNr, ParameterType.QueryString);
                //Logger.AddProcessLog($"request: {request}");
                
                Console.WriteLine(DateTime.Now);
                
                response = client.Execute(request);
                //Logger.AddProcessLog($"response.ResponseStatus: {response.ResponseStatus.ToString()}");
                //Logger.AddProcessLog($"response.Request {response.Request}");
                //Logger.AddProcessLog($"response.Content {response.Content}");

                Console.WriteLine(DateTime.Now);
                
                Console.WriteLine($"response.Content: {response.Content}");
                
                portingDataOldest101 = JsonConvert.DeserializeObject<PortingDataOldest101>(response.Content);
                Console.WriteLine($"MobileNumber: {portingDataOldest101.MobileNumber}");
                Console.WriteLine($"Sender: {portingDataOldest101.Sender}");
                Console.WriteLine($"Oldest101Date: {portingDataOldest101.Oldest101Date}");
                if (portingDataOldest101.ErrorMessages.Any())
                {
                    Console.WriteLine($"ErrorMessages.Code: {portingDataOldest101.ErrorMessages[0].Code}");
                    Console.WriteLine($"ErrorMessages.Message: {portingDataOldest101.ErrorMessages[0].Message}");
                }

                Console.WriteLine($"Success: {portingDataOldest101.Success.ToString()}");
                Console.WriteLine($"TrxId: {portingDataOldest101.TrxId}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if (response != null) Console.WriteLine("response.Content: " + response.Content);
            }
                
            Console.WriteLine(portingDataOldest101.Success);
            Console.WriteLine(portingDataOldest101.Oldest101Date);
            
            Console.WriteLine("Finished");
        }
    }
}