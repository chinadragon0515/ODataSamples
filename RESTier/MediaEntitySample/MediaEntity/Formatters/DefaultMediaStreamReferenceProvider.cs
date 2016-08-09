// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Web.OData;
using Microsoft.OData.Core;

namespace Microsoft.OData.Service.Sample.MediaEntity.Formatters
{
    public class DefaultMediaStreamReferenceProvider : IMediaStreamReferenceProvider
    {
        public ODataStreamReferenceValue GetMediaStreamReference(
            EntityInstanceContext entity,
            Uri baseUri)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            if (!entity.EntityType.HasStream)
            {
                return null;
            }

            return new ODataStreamReferenceValue
            {
                // TODO, should set real value based on passed in entity
                ContentType = "",
                ETag = "ShouldBeReplaceWithRealEtag",
                ReadLink = new Uri(baseUri + "/$value")
            };
        }
    }
}