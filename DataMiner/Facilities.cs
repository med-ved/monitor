//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataMiner
{
    using System;
    using System.Collections.Generic;
    
    public partial class Facilities
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Facilities()
        {
            this.Flats = new HashSet<Flats>();
        }
    
        public int Id { get; set; }
        public Nullable<bool> Kitchen { get; set; }
        public Nullable<bool> Internet { get; set; }
        public Nullable<bool> TV { get; set; }
        public Nullable<bool> ToiletAccessories { get; set; }
        public Nullable<bool> Heating { get; set; }
        public Nullable<bool> AirConditioner { get; set; }
        public Nullable<bool> WashingMashine { get; set; }
        public Nullable<bool> Dryer { get; set; }
        public Nullable<bool> FreeParking { get; set; }
        public Nullable<bool> WirelessInternet { get; set; }
        public Nullable<bool> CableTv { get; set; }
        public Nullable<bool> Breakfast { get; set; }
        public Nullable<bool> PetsAllowed { get; set; }
        public Nullable<bool> FamilyFriendly { get; set; }
        public Nullable<bool> EventsFriendly { get; set; }
        public Nullable<bool> SmokingAllowed { get; set; }
        public Nullable<bool> PeopleWithLimitedAbilities { get; set; }
        public Nullable<bool> Fireplace { get; set; }
        public Nullable<bool> Intercom { get; set; }
        public Nullable<bool> Porter { get; set; }
        public Nullable<bool> SwimingPoll { get; set; }
        public Nullable<bool> Jacuzzi { get; set; }
        public Nullable<bool> Gym { get; set; }
        public Nullable<bool> Shoulder { get; set; }
        public Nullable<bool> Iron { get; set; }
        public Nullable<bool> Hairdryer { get; set; }
        public Nullable<bool> NotebookWorkingPlace { get; set; }
        public Nullable<bool> Lift { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Flats> Flats { get; set; }
    }
}
