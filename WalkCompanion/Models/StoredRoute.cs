using Microsoft.Phone.Maps.Controls;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Runtime.Serialization;

namespace WalkCompanion.Models
{
    [DataContract]
    public class StoredRoute
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<GeoCoordinate> StoredCoordinates { get; set; }
        [DataMember]
        public DateTime StartDate { get; set; }
        [DataMember]
        public DateTime EndDate { get; set; }
        [DataMember]
        public double DistanceTravelled { get; set; }
        [DataMember]
        public TimeSpan TimeTravelled { get; set; }
        
        public GeoCoordinateCollection ToGeoCoordinateCollection()
        {
            GeoCoordinateCollection result = new GeoCoordinateCollection();

            foreach (GeoCoordinate coordinate in StoredCoordinates)
            {
                result.Add(coordinate);
            }

            return result;
        }

    }
}
