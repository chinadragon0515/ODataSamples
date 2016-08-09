// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Web.OData.Builder;

namespace Microsoft.OData.Service.Sample.MediaEntity.Models
{    
    // Mark the entity to have stream
    // The photo stream is stored in file system, it could be stored in database as byte array too.
    // This implies a no-named property for stream and conversion url to access is ~/entityset(key)/$value
    [MediaType]
    public class Photo
    {
        public string Id { get; set; }

        public long Size { get; set; }

        public string Name { set; get; }

        public string Type { set; get; }

        // Will not be a database column but an Edm model property
        // The stream data is stored in filesystem but not database.
        [NotMapped]
        public Stream Smallpreview { get; set; }
    }
}