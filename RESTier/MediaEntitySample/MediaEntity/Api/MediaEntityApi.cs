// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.OData.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.Service.Sample.MediaEntity.Models;
using Microsoft.Restier.Core;
using Microsoft.Restier.Core.Model;
using Microsoft.Restier.Providers.EntityFramework;

namespace Microsoft.OData.Service.Sample.MediaEntity.Api
{
    public class MediaEntityApi : EntityFrameworkApi<MediaEntityModel>
    {
        public MediaEntityModel ModelContext
        {
            get { return DbContext; }
        }

        protected override IServiceCollection ConfigureApi(IServiceCollection services)
        {
            return base.ConfigureApi(services)
                .AddService<IModelBuilder, MediaEntityModelExtender>();
        }

        private class MediaEntityModelExtender : IModelBuilder
        {
            public Task<IEdmModel> GetModelAsync(ModelContext context, CancellationToken cancellationToken)
            {
                var builder = new ODataConventionModelBuilder();

                builder.EntitySet<Person>("People");
                builder.EntitySet<Photo>("Photos");
                var entityConfiguration = builder.StructuralTypes.First(t => t.ClrType == typeof(Photo));
                entityConfiguration.AddProperty(typeof(Photo).GetProperty("Smallpreview"));
                return Task.FromResult(builder.GetEdmModel());
            }
        }
    }
}