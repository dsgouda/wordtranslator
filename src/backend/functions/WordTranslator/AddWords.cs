using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WordTranslatorInc
{
    using Modules;

    public static class AddWords
    {
        [FunctionName("AddWords")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string word1 = req.Query["word1"];
            string word2 = req.Query["word2"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            word1 = word1 ?? data?.word1;

            Exception exceptionThrown = null;
            int result = -1;

            try
            {
                result = WordToIntTranslator.AddWords(word1, word2);
            }
            catch (Exception e)
            {
                exceptionThrown = e;
            }

            if (exceptionThrown != null)
            {
                return new BadRequestObjectResult(exceptionThrown);
            }

            return new OkObjectResult(result);
        }
    }
}
