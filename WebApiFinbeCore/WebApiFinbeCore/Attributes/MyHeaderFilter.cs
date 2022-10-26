using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace WebApiFinbeCore.Attributes
{
    public class MyHeaderFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
                operation.parameters = new List<Parameter>();

            operation.parameters.Add(new Parameter
            {
                name = "Authorization",
                @in = "header",
                type = "string",
                required = true,
                description = "Api Key Autorizada"
            });

            var customHeaderAttributes = apiDescription.ActionDescriptor.GetCustomAttributes<SwaggerHeaderAttribute>();

            foreach (var customHeader in customHeaderAttributes)
            {
                var existingParam = operation.parameters.FirstOrDefault(p =>
                p.@in == "header" && p.name == customHeader.HeaderName);
                if (existingParam != null)
                {
                    operation.parameters.Remove(existingParam);
                }

                operation.parameters.Add(new Parameter
                {
                    name = customHeader.HeaderName,
                    @in = "header",
                    type = "string",
                    required = customHeader.IsRequired,
                    description = customHeader.Description,
                    @default = customHeader.DefaultValue
                });
            }
        }
    }
}