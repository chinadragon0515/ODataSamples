// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Web.OData;
using Microsoft.OData.Core;

namespace Microsoft.OData.Service.Sample.MediaEntity.Formatters
{
    public interface IMediaStreamReferenceProvider
    {
        ODataStreamReferenceValue GetMediaStreamReference(EntityInstanceContext entity, Uri baseUri);
    }
}