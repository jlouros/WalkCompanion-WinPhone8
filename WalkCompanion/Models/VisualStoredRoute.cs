using System;

namespace WalkCompanion.Models
{
    public class VisualStoredRoute
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string StartDate { get; set; }
        
        public string EndDate { get; set; }
        
        public string DistanceTravelled { get; set; }
        
        public string TimeTravelled { get; set; }
    }
}
