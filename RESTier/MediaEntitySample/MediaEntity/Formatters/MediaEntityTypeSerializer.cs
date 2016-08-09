// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Web.OData;
using System.Web.OData.Formatter.Serialization;
using Microsoft.OData.Core;

namespace Microsoft.OData.Service.Sample.MediaEntity.Formatters
{
    public class MediaEntityTypeSerializer : ODataEntityTypeSerializer
    {
        public MediaEntityTypeSerializer(ODataSerializerProvider serializerProvider)
            : base(serializerProvider)
        {
        }

        public override ODataEntry CreateEntry(
            SelectExpandNode selectExpandNode,
            EntityInstanceContext entityInstanceContext)
        {
            if (selectExpandNode == null)
            {
                throw new ArgumentNullException("selectExpandNode");
            }

            if (entityInstanceContext == null)
            {
                throw new ArgumentNullException("entityInstanceContext");
            }

            var entry = base.CreateEntry(selectExpandNode, entityInstanceContext);


            // if the model doesn't have a stream, then this isn't a media link entry
            if (!entityInstanceContext.EntityType.HasStream)
            {
                return entry;
            }

            // must have an entity
            if (entityInstanceContext.EntityInstance == null)
            {
                return entry;
            }

            // get the metadata provider associated with the current request
            IMediaStreamReferenceProvider provider = null;

            var key = typeof(IMediaStreamReferenceProvider).FullName;
            object value;
            if (entityInstanceContext.Request.Properties.TryGetValue(key, out value))
            {
                provider = (IMediaStreamReferenceProvider)value;
            }

            // need metadata to construct the media link entry
            if (provider == null)
            {
                return entry;
            }

            var baseIdUri = entityInstanceContext.Url.Request.RequestUri;
            if (entry.Id != null)
            {
                baseIdUri = entry.Id;
            }

            // attach the media link entry
            var context = entityInstanceContext.SerializerContext;
            var mediaResource = provider.GetMediaStreamReference(entityInstanceContext, baseIdUri);
            entry.MediaResource = mediaResource;

            // Set value for Stream type property....
            foreach (var property in entry.Properties)
            {
                if (property.Name == "Smallpreview")
                {
                    property.Value = new ODataStreamReferenceValue
                        {
                            // TODO, should be a real URL
                            EditLink = new Uri(baseIdUri + "/Smallpreview"),
                        };
                }
            }

            return entry;
        }
    }
}