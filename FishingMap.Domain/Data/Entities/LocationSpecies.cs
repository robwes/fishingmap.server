using System;
using System.Collections.Generic;
using System.Text;

namespace FishingMap.Domain.Data.Entities
{
    public class LocationSpecies
    {
        public int LocationId { get; set; }
        public Location Location { get; set; }

        public int SpeciesId { get; set; }
        public Species Species { get; set; }
    }
}
