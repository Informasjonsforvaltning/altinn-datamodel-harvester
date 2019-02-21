using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using AltinnCore.Common.Factories.ModelFactory;
using Manatee.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Altinn.Service.Controllers
{
    /// <summary>
    /// This controller contains utility operations to convert from xsd to jsonSchema
    /// </summary>
    public class SchemaController : Controller
    {
        /// <summary>
        /// Returns the all production schemas in Altinn as JsonSchemas
        /// </summary>
        /// <returns>The schemas</returns>
        [HttpGet]
        [Route("api/v1/schemas")]
        public async Task<IActionResult> Schemas()
        {
            AltinnServiceRepository repositoryClient = new AltinnServiceRepository();

            Task<List<AltinnResource>> serviceRequestTask = AltinnServiceRepository.ReadAllSchemas();

            await Task.WhenAll(serviceRequestTask);

            if (serviceRequestTask.Result != null)
            {
                Manatee.Json.Serialization.JsonSerializer serializer = new Manatee.Json.Serialization.JsonSerializer();

                JsonValue json = serializer.Serialize(serviceRequestTask.Result);

                return Ok(json.GetIndentedString());
            }

            return NoContent();
        }

        [HttpPost]
        [Route("api/v1/convert")]        
        public async Task<IActionResult> Convert()
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
            };

            if (Request.ContentType.Contains("text/xml"))
            {            
                XmlReader doc = XmlReader.Create(Request.Body, settings);

                // XSD to Json Schema
                XsdToJsonSchema xsdToJsonSchemaConverter = new XsdToJsonSchema(doc, null);

                Manatee.Json.Serialization.JsonSerializer serializer = new Manatee.Json.Serialization.JsonSerializer();

                JsonValue json = serializer.Serialize(xsdToJsonSchemaConverter.AsJsonSchema());

                return Ok(json.GetIndentedString());
            }          
            
            return NotFound("Cannot read body. Needs to be XSD.");            
        }
    }
}
