﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Validation;
using System.Web.Http.Validation.Providers;
using System.Web.Http.ValueProviders.Providers;

namespace DT.Core.Web.Common.Api.WebApi.Formatter
{
    /// <summary>
    /// Represents the <see cref="MediaTypeFormatter"/> class to handle multipart/form-data. 
    /// </summary>
    public class FormMultipartEncodedMediaTypeFormatter : MediaTypeFormatter
    {
        private const string SupportedMediaType = "multipart/form-data";

        /// <summary>
        /// Initializes a new instance of the <see cref="FormMultipartEncodedMediaTypeFormatter"/> class.
        /// </summary>
        public FormMultipartEncodedMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(SupportedMediaType));
        }

        public override bool CanReadType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return false;
        }

        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (readStream == null) throw new ArgumentNullException(nameof(readStream));

            try
            {
                // load multipart data into memory 
                MultipartMemoryStreamProvider multipartProvider = await content.ReadAsMultipartAsync();
                // fill parts into a ditionary for later binding to model
                IDictionary<string, object> modelDictionary = await ToModelDictionaryAsync(multipartProvider);
                // bind data to model 
                return BindToModel(modelDictionary, type, formatterLogger);
            }
            catch (Exception e)
            {
                if (formatterLogger == null)
                {
                    throw;
                }
                formatterLogger.LogError(string.Empty, e);
                return GetDefaultValueForType(type);
            }
        }

        private async Task<IDictionary<string, object>> ToModelDictionaryAsync(MultipartMemoryStreamProvider multipartProvider)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            // iterate all parts 
            foreach (HttpContent part in multipartProvider.Contents)
            {
                // unescape the name 
                string name = part.Headers.ContentDisposition.Name.Trim('"');

                // if we have a filename, we treat the part as file upload,
                // otherwise as simple string, model binder will convert strings to other types. 
                if (!string.IsNullOrEmpty(part.Headers.ContentDisposition.FileName))
                {
                    // set null if no content was submitted to have support for [Required]
                    if (part.Headers.ContentLength.GetValueOrDefault() > 0)
                    {
                        if (dictionary.ContainsKey(name))
                        {
                            if (dictionary[name].GetType() != typeof(ArrayList))
                            {
                                ArrayList list = new ArrayList();
                                list.Add(dictionary[name]);
                                dictionary[name] = list;
                            }
                            ((ArrayList)dictionary[name]).Add(new HttpPostedFileMultipart(
                                        part.Headers.ContentDisposition.FileName.Trim('"'),
                                        part.Headers.ContentType.MediaType,
                                        await part.ReadAsByteArrayAsync()));
                        }
                        else
                        {
                            dictionary[name] = new HttpPostedFileMultipart(
                                part.Headers.ContentDisposition.FileName.Trim('"'),
                                part.Headers.ContentType.MediaType,
                                await part.ReadAsByteArrayAsync()
                            );
                        }
                    }
                    else
                    {
                        dictionary[name] = null;
                    }
                }
                else
                {
                    dictionary[name] = await part.ReadAsStringAsync();
                }
            }

            return dictionary;
        }


        private object BindToModel(IDictionary<string, object> data, Type type, IFormatterLogger formatterLogger)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (type == null) throw new ArgumentNullException(nameof(type));

            HttpConfiguration config = GlobalConfiguration.Configuration;

            // if there is a requiredMemberSelector set, use this one by replacing the validator provider
            bool validateRequiredMembers = RequiredMemberSelector != null && formatterLogger != null;
            if (validateRequiredMembers)
            {
                config.Services.Replace(typeof(ModelValidatorProvider), new RequiredMemberModelValidatorProvider(RequiredMemberSelector));
            }

            // create a action context for model binding
            HttpActionContext actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext
                {
                    Configuration = config,
                    ControllerDescriptor = new HttpControllerDescriptor
                    {
                        Configuration = config
                    }
                }
            };

            // create model binder context 
            NameValuePairsValueProvider valueProvider = new NameValuePairsValueProvider(data, CultureInfo.InvariantCulture);
            ModelMetadataProvider metadataProvider = actionContext.ControllerContext.Configuration.Services.GetModelMetadataProvider();
            ModelMetadata metadata = metadataProvider.GetMetadataForType(null, type);
            ModelBindingContext modelBindingContext = new ModelBindingContext
            {
                ModelName = string.Empty,
                FallbackToEmptyPrefix = false,
                ModelMetadata = metadata,
                ModelState = actionContext.ModelState,
                ValueProvider = valueProvider
            };

            // bind model 
            CompositeModelBinderProvider modelBinderProvider = new CompositeModelBinderProvider(config.Services.GetModelBinderProviders());
            IModelBinder binder = modelBinderProvider.GetBinder(config, type);
            bool haveResult = binder.BindModel(actionContext, modelBindingContext);

            // log validation errors 
            if (formatterLogger != null)
            {
                foreach (KeyValuePair<string, ModelState> modelStatePair in actionContext.ModelState)
                {
                    foreach (ModelError modelError in modelStatePair.Value.Errors)
                    {
                        if (modelError.Exception != null)
                        {
                            formatterLogger.LogError(modelStatePair.Key, modelError.Exception);
                        }
                        else
                        {
                            formatterLogger.LogError(modelStatePair.Key, modelError.ErrorMessage);
                        }
                    }
                }
            }

            return haveResult ? modelBindingContext.Model : GetDefaultValueForType(type);
        }
    }
}
