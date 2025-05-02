using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;

namespace func_sf_d365_omnisync_accounts_update
{
    public class UpdateAccountFunction
    {
        private readonly ILogger<UpdateAccountFunction> _logger;

        public UpdateAccountFunction(ILogger<UpdateAccountFunction> logger)
        {
            _logger = logger;
        }

        [Function("UpdateAccount")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("C# HTTP trigger function processed a request.");

                var content = new StreamReader(req.Body).ReadToEndAsync().Result;

                var objects = JsonConvert.DeserializeObject<dynamic>(content);
                foreach (var o in objects)
                {
                    Console.WriteLine($"{o.id.ToString()}");
                }


                dynamic inputAccountValue = JsonConvert.DeserializeObject<dynamic>(content);

                string SecretID = Environment.GetEnvironmentVariable("SECRET_ID");
                string AppID = Environment.GetEnvironmentVariable("APP_ID");
                string InstanceUri = Environment.GetEnvironmentVariable("D365_URL");



                string ConnectionStr = $@"AuthType=ClientSecret;SkipDiscovery=true;url={InstanceUri};
                                      Secret={SecretID};ClientId={AppID};RequireNewInstance=true";


                using (ServiceClient svc = new ServiceClient(ConnectionStr))

                    if (svc.IsReady && inputAccountValue != null)
                    {
                        var account = new Entity("account");

                        account.Attributes["accountnumber"] = inputAccountValue.payload.cgcloud__Account_Number__c;

                        foreach (var changedField in inputAccountValue.changedFields)
                        {
                            if (changedField == "name")
                            {
                                account.Attributes["name"] = inputAccountValue.payload.Name;
                            }
                            else if (changedField == "cgcloud__Account_Email__c")
                            {
                                account.Attributes["emailaddress1"] = inputAccountValue.payload.cgcloud__Account_Email__c;
                            }
                            else if (changedField == "Phone")
                            {
                                account.Attributes["telephone1"] = inputAccountValue.payload.Phone;
                            }
                        }

                        if (!string.IsNullOrEmpty(inputAccountValue.payload.ShippingAddress.Street))
                        {
                            account.Attributes["address1_street"] = inputAccountValue.payload.ShippingAddress.Street;
                        }
                        if (!string.IsNullOrEmpty(inputAccountValue.payload.ShippingAddress.City))
                        {
                            account.Attributes["address1_city"] = inputAccountValue.payload.ShippingAddress.City;
                        }
                        if (!string.IsNullOrEmpty(inputAccountValue.payload.ShippingAddress.PostalCode))
                        {
                            account.Attributes["address1_postalcode"] = inputAccountValue.payload.ShippingAddress.PostalCode;
                        }
                        if (!string.IsNullOrEmpty(inputAccountValue.payload.ShippingAddress.State))
                        {
                            account.Attributes["address1_state"] = inputAccountValue.payload.ShippingAddress.State;
                        }
                        if (!string.IsNullOrEmpty(inputAccountValue.payload.ShippingAddress.Country))
                        {
                            account.Attributes["address1_country"] = inputAccountValue.payload.ShippingAddress.Country;
                        }

                        svc.Create(account);
                    }


                return new OkObjectResult("Success");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}




