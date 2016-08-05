using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMiner
{
    public static class Database
    {
        public static void Save(FlatStatus status)
        {
            using (var context = new MonitorEntities())
            {
                var flat = context.Flats.Where(f => f.Id == status.Flat.Id).FirstOrDefault<Flats>();
                if (flat == null)
                {
                    flat = AddNewFlat(context, status);
                }

                var newStatus = new FlatStatuses()
                {
                    Available = status.Available,
                    Date = status.Date,
                    Price = (short?)status.Price.Get(),
                    FlatId = flat.Id,
                    Flats = flat
                };
                context.FlatStatuses.Add(newStatus);

                //Perform Update operation
                /*Student studentToUpdate = studentList.Where(s => s.StudentName == "student1").FirstOrDefault<Student>();
                studentToUpdate.StudentName = "Edited student1";

                //Perform delete operation
                context.Students.Remove(studentList.ElementAt<Student>(0));

                //Execute Inser, Update & Delete queries in the database*/
                context.SaveChanges();
            }
        }

        public static Flats AddNewFlat(MonitorEntities context, FlatStatus status)
        {
            var newFlat = new Flats()
            {
                Id = status.Flat.Id,
                Latitude = (float?)status.Flat.Latitude.Get(),
                Longitude = (float?)status.Flat.Longitude.Get(),
                Country = status.Flat.Country,
                City = status.Flat.City,
                Description = status.Flat.Description,
                Rating = (float?)status.Flat.Rating.Get()
            };
            context.Flats.Add(newFlat);

            return newFlat;
        }

        public static bool IsFlatProcessed(long flatId, DateTime date)
        {
            using (var context = new MonitorEntities())
            {
                return context.FlatStatuses.Any(s => s.FlatId.Value == flatId && s.Date == date); 
            }
        }

        public static T? Get<T>(this Nullable<T> t) where T : struct
        {
            if (t.HasValue)
            {
                return t.Value;
            }

            return null;
        }
    }
}
