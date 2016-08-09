// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.OData.Core;

namespace Microsoft.OData.Service.Sample.MediaEntity.Models
{
    public class PhotoDataStoreAccess
    {
        private static readonly DirectoryInfo _appDataDirectory = new DirectoryInfo(AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
        private static readonly DirectoryInfo _filesDirectory = new DirectoryInfo(Path.Combine(_appDataDirectory.FullName, "Files"));

        public PhotoDataStoreAccess()
        {
            if (!FilesDirectory.Exists)
            {
                FilesDirectory.Create();
            }
        }

        internal static DirectoryInfo FilesDirectory
        {
            get { return _filesDirectory; }
        }

        public async Task CreateAsync(
            string identifier,
            Stream stream)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            var binaryFile = new FileInfo(Path.Combine(_filesDirectory.FullName, identifier));

            try
            {
                using (var fileStream = new FileStream(binaryFile.FullName, FileMode.CreateNew, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
            catch (Exception exception)
            {
                throw new ODataException();
            }
        }

        public async Task<Stream> GetStreamAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("fullName");
            }

            var binaryFile = GetFileInfo(identifier);

            try
            {
                return binaryFile.OpenRead();
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception exception)
            {
                throw new ODataException();
            }
        }
        
        public async Task UpdateStreamAsync(
            string identifier,
            Stream stream)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var binaryFile = GetFileInfo(identifier);

            if (!binaryFile.Exists)
            {
                throw new ODataException();
            }

            try
            {
                using (var fileStream = new FileStream(binaryFile.FullName, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
            catch (Exception exception)
            {
                throw new ODataException();
            }
        }

        internal static FileInfo GetFileInfo(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            return new FileInfo(Path.Combine(_filesDirectory.FullName, identifier));
        }
    }
}