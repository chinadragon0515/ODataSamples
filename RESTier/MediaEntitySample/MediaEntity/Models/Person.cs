// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.Service.Sample.MediaEntity.Models
{
    public class Person
    {
        public long PersonId { get; set; }

        public string UserName { get; set; }

        public Photo Photo { get; set; }
    }
}
