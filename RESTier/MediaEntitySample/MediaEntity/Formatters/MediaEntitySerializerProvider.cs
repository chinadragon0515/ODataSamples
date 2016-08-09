// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.OData.Formatter.Serialization;
using Microsoft.OData.Edm;

namespace Microsoft.OData.Service.Sample.MediaEntity.Formatters
{
    public class MediaEntitySerializerProvider : DefaultODataSerializerProvider
    {
        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType.IsEntity())
            {
                return new MediaEntityTypeSerializer(this);
            }

            return base.GetEdmTypeSerializer(edmType);
        }
    }
}