// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.OData.Service.Sample.MediaEntity.Models
{
    public class Person
    {
        public long PersonId { get; set; }

        public string UserName { get; set; }

        public Photo Photo { get; set; }

        // A non-media entity type could have stream properties, and its value should be set to ODataStreamReferenceValue
        // when requesting Person
        public Stream StreamMedia { get; set; }
    }
}
