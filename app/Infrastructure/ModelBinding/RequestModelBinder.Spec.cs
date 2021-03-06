﻿using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MidnightLizard.Schemes.Commander.Infrastructure.ModelBinding;
using MidnightLizard.Schemes.Commander.Requests.PublishScheme;
using MidnightLizard.Schemes.Commander.Infrastructure.Serialization;
using MidnightLizard.Testing.Utilities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MidnightLizard.Schemes.Commander.Infrastructure.Authentication;

namespace MidnightLizard.Schemes.Commander.Infrastructure.ModelBinding
{
    public class RequestModelBinderSpec
    {
        private readonly IRequestMetaDeserializer requestMetaDeserializer;
        private readonly RequestSchemaVersionAccessor versionAccessor;
        private readonly RequestModelBinder modelBinder;
        private readonly ModelBindingContext context;
        private readonly SchemaVersion testSchemaVersion = new SchemaVersion("1.3.0");

        public RequestModelBinderSpec()
        {
            this.requestMetaDeserializer = Substitute.For<IRequestMetaDeserializer>();
            this.versionAccessor = Substitute.For<RequestSchemaVersionAccessor>();
            this.modelBinder = new RequestModelBinder(
                this.requestMetaDeserializer,
                this.versionAccessor);
            this.context = Substitute.For<ModelBindingContext>();

            this.context.ModelType.Returns(typeof(PublishSchemeRequest));
            this.context.ModelState = new ModelStateDictionary();
            this.context.BindingSource = BindingSource.Body;

            this.versionAccessor.GetSchemaVersion(this.context).Returns(this.testSchemaVersion);
        }

        [It(nameof(RequestModelBinder.BindModelAsync))]
        public async Task Should_read_ModelType()
        {
            await this.modelBinder.BindModelAsync(this.context);

            var test = this.context.Received(1).ModelType;
        }

        [It(nameof(RequestModelBinder.BindModelAsync))]
        public async Task Should_read_ApiVersion()
        {
            await this.modelBinder.BindModelAsync(this.context);

            this.versionAccessor.Received(1).GetSchemaVersion(this.context);
        }

        [It(nameof(RequestModelBinder.BindModelAsync))]
        public async Task Should_call_RequestSerializer()
        {
            await this.modelBinder.BindModelAsync(this.context);

            this.requestMetaDeserializer.Received(1)
                .Deserialize(typeof(PublishSchemeRequest), this.testSchemaVersion, this.context);
        }

        [It(nameof(RequestModelBinder.BindModelAsync))]
        public async Task Should_set_ModelBindingContext__Result_to_Success()
        {
            await this.modelBinder.BindModelAsync(this.context);

            this.context.Result.IsModelSet.Should().BeTrue();
        }

        [It(nameof(RequestModelBinder.BindModelAsync))]
        public async Task Should_set_ModelBindingContext__Result_to_Fail_when_Exception()
        {
            this.requestMetaDeserializer
                .Deserialize(typeof(PublishSchemeRequest), this.testSchemaVersion, this.context)
                .Returns(x => throw new Exception("test"));

            await this.modelBinder.BindModelAsync(this.context);

            this.context.Result.IsModelSet.Should().BeFalse();
        }

        [It(nameof(RequestModelBinder.BindModelAsync))]
        public async Task Should_add_Error_to_ModelState_when_Exception_raised()
        {
            var testErrorMessage = "test error message";
            this.requestMetaDeserializer
                .Deserialize(typeof(PublishSchemeRequest), this.testSchemaVersion, this.context)
                .Returns(x => throw new Exception(testErrorMessage));

            await this.modelBinder.BindModelAsync(this.context);

            this.context.ModelState.Should().HaveCount(1);
            var firstState = this.context.ModelState.First();
            firstState.Value.Errors.Should().HaveCount(1);
            firstState.Value.Errors[0].ErrorMessage.Should().Be(testErrorMessage);
        }
    }
}
