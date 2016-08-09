// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity;

namespace Microsoft.OData.Service.Sample.MediaEntity.Models
{
    public class MediaEntityModel : DbContext
    {
        static MediaEntityModel()
        {
            Database.SetInitializer(new MediaEntityDatabaseInitializer());
        }

        public MediaEntityModel()
            : base("name=MediaEntityModel")
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Photo> Photos { get; set; }

        private static MediaEntityModel instance;
        public static MediaEntityModel Instance
        {
            get
            {
                if (instance == null)
                {
                    ResetDataSource();
                }
                return instance;
            }
        }

        public static void ResetDataSource()
        {
            instance = new MediaEntityModel();

            // Discard all local changes, and reload data from DB, them remove all
            foreach (var x in instance.People)
            {
                // Discard local changes for the person..
                instance.Entry(x).State = EntityState.Detached;
            }

            instance.People.RemoveRange(instance.People);

            // This is to set the People Id from 0
            instance.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('People', RESEED, 0)");

            instance.SaveChanges();

            #region People

            #region Friends russellwhyte & scottketchum & ronaldmundy
            var person1 = new Person
            {
                PersonId = 1,
                UserName = "russellwhyte"
            };

            var person2 = new Person
            {
                PersonId = 2,
                UserName = "scottketchum"
            };

            var person3 = new Person
            {
                PersonId = 3,
                UserName = "ronaldmundy",
            };

            var person4 = new Person
            {
                PersonId = 4,
                UserName = "javieralfred",
            };

            #endregion

            instance.People.AddRange(new List<Person>
            {
                person1,
                person2,
                person3,
                person4,
            });

            #endregion

            instance.SaveChanges();
        }
    }

    class MediaEntityDatabaseInitializer : DropCreateDatabaseAlways<MediaEntityModel>
    {
        protected override void Seed(MediaEntityModel context)
        {
            MediaEntityModel.ResetDataSource();
        }
    }
}
