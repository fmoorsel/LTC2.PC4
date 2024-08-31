using System.Collections.Generic;

namespace LTC2.Shared.Models.Mapdefinitions
{
    public class NeighbourDefinition
    {
        public PlaceDefination Place { get; set; }


        public List<PlaceDefination> Neighbours { get; set; }
    }
}
