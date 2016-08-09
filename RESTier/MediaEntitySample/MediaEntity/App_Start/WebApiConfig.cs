// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;
using Microsoft.OData.Service.Sample.MediaEntity.Api;
using Microsoft.OData.Service.Sample.MediaEntity.Formatters;
using Microsoft.Restier.Publishers.OData.Batch;
using Microsoft.Restier.Publishers.OData.Routing;

namespace Microsoft.OData.Service.Sample.MediaEntity
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            RegisterMediaEntity(config, GlobalConfiguration.DefaultServer);
            config.Formatters.InsertRange(0, ODataMediaTypeFormatters.Create(new MediaEntitySerializerProvider(), new DefaultODataDeserializerProvider()));
        }

        public static async void RegisterMediaEntity(
            HttpConfiguration config, HttpServer server)
        {
            await config.MapRestierRoute<MediaEntityApi>(
                "MediaEntityApi", "api/mediaentity",
                new RestierBatchHandler(server));
        }
    }
}
