// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData;
using System.Web.OData.Routing;
using Microsoft.OData.Service.Sample.MediaEntity.Api;
using Microsoft.OData.Service.Sample.MediaEntity.Formatters;
using Microsoft.OData.Service.Sample.MediaEntity.Models;

namespace Microsoft.OData.Service.Sample.MediaEntity.Controllers
{
    /// <summary>
    /// For photo, the metadata information is stored in database.
    /// The real photo file is stored in file system
    /// The method without value will operate with metadata.
    /// The method with value will operate with photo stream.
    ///  </summary>
    public class PhotosController : ODataController
    {
        private MediaEntityApi api;

        private PhotoDataStoreAccess _photoDataStoreAccess;

        private MediaEntityModel DbContext
        {
            get
            {
                return Api.ModelContext;
            }
        }

        private MediaEntityApi Api
        {
            get
            {
                if (api == null)
                {
                    api = new MediaEntityApi();
                }

                return api;
            }
        }

        private PhotoDataStoreAccess PhotoDataStoreAccess
        {
            get
            {
                if (_photoDataStoreAccess == null)
                {
                    _photoDataStoreAccess = new PhotoDataStoreAccess();
                }
                return _photoDataStoreAccess;
            }
        }

        public IHttpActionResult Get()
        {
            return Ok(DbContext.Photos);
        }

        public async Task<IHttpActionResult> Get([FromODataUri] string key)
        {
            return Ok(DbContext.Photos.Where(p =>p.Id == key));
        }

        public async Task<IHttpActionResult> Post()
        {
            if (!Request.Content.Headers.ContentLength.HasValue || Request.Content.Headers.ContentLength.Value <= 0)
            {
                return BadRequest();
            }

            var contentTypeHeader = Request.Content.Headers.ContentType;

            if (contentTypeHeader == null || contentTypeHeader.MediaType == null)
            {
                return BadRequest();
            }

            var contentLength = Request.Content.Headers.ContentLength;

            if (contentLength.Value <= 0)
            {
                return BadRequest();
            }

            var identifier = Guid.NewGuid().ToString("N").ToLowerInvariant();
            var mediaType = contentTypeHeader.MediaType;

            var stream = await Request.Content.ReadAsStreamAsync();

            var photo = new Photo
            {
                Id = identifier,
                Name = identifier,
                Type = mediaType,
                Size = contentLength.Value
            };

            // Store metadata into database
            DbContext.Photos.Add(photo);
            DbContext.SaveChanges();

            // Store photo streaming into file system
            await PhotoDataStoreAccess.CreateAsync(
                identifier,
                stream);

            return Created(photo);
        }

        public async Task<IHttpActionResult> Put([FromODataUri] string key, Photo photo)
        {
            if (photo == null)
            {
                return BadRequest();
            }

            try
            {
                var photoOriginal = DbContext.Photos.Where(p => p.Id == key).FirstOrDefault();
                photoOriginal.Size = photo.Size;
                photoOriginal.Name = photo.Name;
                photoOriginal.Type = photo.Type;

                DbContext.SaveChanges();

                return Updated(photoOriginal);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] string key, Delta<Photo> photoDelta)
        {
            if (photoDelta == null)
            {
                return BadRequest();
            }

            try
            {
                var photoOriginal = DbContext.Photos.Where(p => p.Id == key).FirstOrDefault();
                photoDelta.Patch(photoOriginal);
                photoOriginal.Id = key;
                DbContext.SaveChanges();
                return Updated(photoOriginal);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        public async Task<IHttpActionResult> Delete([FromODataUri] string key)
        {
            try
            {
                var photoOriginal = DbContext.Photos.Where(p => p.Id == key).FirstOrDefault();
                DbContext.Photos.Remove(photoOriginal);
                DbContext.SaveChanges();
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [ODataRoute("Photos({key})/$value")]
        public async Task<IHttpActionResult> GetValue([FromODataUri] string key)
        {

            var photoOriginal = DbContext.Photos.Where(p => p.Id == key).FirstOrDefault();
            var photoStream = await PhotoDataStoreAccess.GetStreamAsync(key);

            if (photoStream == null)
            {
                return NotFound();
            }

            var range = Request.Headers.Range;

            if (range == null)
            {
                // if the range header is present but null, then the header value must be invalid
                if (Request.Headers.Contains("Range"))
                {
                    return StatusCode(HttpStatusCode.RequestedRangeNotSatisfiable);
                }

                // if no range was requested, return the entire stream
                var response = Request.CreateResponse(HttpStatusCode.OK);

                response.Headers.AcceptRanges.Add("bytes");

                response.Content = new StreamContent(photoStream);
                response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(photoOriginal.Type);

                if (photoOriginal.Name != null)
                {
                    var contentDispositionHeader = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = photoOriginal.Name
                    };

                    response.Content.Headers.ContentDisposition = contentDispositionHeader;
                }

                return ResponseMessage(response);
            }
            else
            {
                if (!photoStream.CanSeek)
                {
                    return StatusCode(HttpStatusCode.NotImplemented);
                }

                var response = Request.CreateResponse(HttpStatusCode.PartialContent);
                response.Headers.AcceptRanges.Add("bytes");

                try
                {
                    // return the requested range(s)
                    response.Content = new ByteRangeStreamContent(photoStream, range, photoOriginal.Type);
                }
                catch (InvalidByteRangeException)
                {
                    response.Dispose();
                    throw;
                }

                if (photoOriginal.Name != null)
                {
                    var contentDispositionHeader = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = photoOriginal.Name
                    };

                    response.Content.Headers.ContentDisposition = contentDispositionHeader;
                }

                // change status code if the entire stream was requested
                if (response.Content.Headers.ContentLength.Value == photoStream.Length)
                {
                    response.StatusCode = HttpStatusCode.OK;
                }

                return ResponseMessage(response);
            }
        }

        [HttpPut]
        [ODataRoute("Photos({key})/$value")]
        public async Task<IHttpActionResult> PutValue([FromODataUri] string key)
        {
            try
            {
                var contentTypeHeader = Request.Content.Headers.ContentType;

                if (contentTypeHeader == null || contentTypeHeader.MediaType == null)
                {
                    return BadRequest();
                }

                var contentLength = Request.Content.Headers.ContentLength;

                if (!contentLength.HasValue)
                {
                    return StatusCode(HttpStatusCode.LengthRequired);
                }

                if (contentLength.Value <= 0)
                {
                    return BadRequest();
                }

                var stream = await Request.Content.ReadAsStreamAsync();
                await PhotoDataStoreAccess.UpdateStreamAsync(
                    key,
                    stream);

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [ODataRoute("Photos({key})/$value")]
        // The logic should be same as Delete action
        public IHttpActionResult DeleteValue([FromODataUri] string key)
        {
            try
            {
                var photoOriginal = DbContext.Photos.Where(p => p.Id == key).FirstOrDefault();
                DbContext.Photos.Remove(photoOriginal);
                DbContext.SaveChanges();
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            // Set the provider to create ODataStreamReferenceValue for stream value
            var provider = new DefaultMediaStreamReferenceProvider();
            var key = typeof(IMediaStreamReferenceProvider).FullName;
            Request.Properties[key] = provider;
        }
    }
}