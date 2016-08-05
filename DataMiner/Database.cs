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

                //var p = context.FlatStatuses.Any(s => s.FlatId == flat.Id && s.Date == newStatus.Date);
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
                var flat = context.Flats.FirstOrDefault(f => f.Id == flatId);
                if (flat == null)
                {
                    return false;
                }

                //return context.FlatStatuses.Where<FlatStatuses>(s => s.FlatId == flatId && s.Date == date).FirstOrDefault<FlatStatuses>() != null;
                var bbb = context.FlatStatuses.Any(s => s.FlatId.Value == flatId);
                var bbb2 = context.FlatStatuses.Any(s => s.Date == date);
                var bbb3 = context.FlatStatuses.Any(s => s.FlatId.Value == flatId && s.Date == date);
                var all = context.FlatStatuses.ToList();
                var b = all[0].FlatId.Value == flatId;
                var exist = all.Any(s => s.FlatId.Value == flatId);
                return exist;
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
