﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MidnightLizard.Schemes.Commander.Infrastructure.Authentication;
using MidnightLizard.Schemes.Commander.Infrastructure.ModelBinding;
using MidnightLizard.Schemes.Commander.Infrastructure.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Commander.Infrastructure.ModelBinding
{
    public class RequestModelBinder : IModelBinder
    {
        protected readonly IRequestMetaDeserializer requestSerializer;
        protected readonly RequestVersionAccessor requestVersionAccessor;
        protected readonly RequestBodyAccessor requestBodyAccessor;

        public RequestModelBinder(
            IRequestMetaDeserializer requestSerializer,
            RequestVersionAccessor requestVersionAccessor,
            RequestBodyAccessor requestBodyAccessor)
        {
            this.requestBodyAccessor = requestBodyAccessor;
            this.requestSerializer = requestSerializer;
            this.requestVersionAccessor = requestVersionAccessor;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            try
            {
                var version = this.requestVersionAccessor.GetApiVersion(bindingContext);
                var modelType = bindingContext.ModelType;

                string requestData = "";

                if (bindingContext.BindingSource == BindingSource.Body)
                {
                    requestData = await this.requestBodyAccessor.ReadAsync(bindingContext);
                }
                else
                {
                    requestData = bindingContext.ValueProvider
                        .GetValue(bindingContext.BinderModelName).FirstValue;
                }

                var request = this.requestSerializer.Deserialize(modelType, version, requestData);

                bindingContext.Result = ModelBindingResult.Success(request);
            }
            catch (Exception ex)
            {
                bindingContext.ModelState.AddModelError(bindingContext.FieldName, ex.Message);
            }
        }
    }
}