using LTC2.Shared.Models.Mapdefinitions;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace LTC2.Shared.Repositories.Interfaces
{
    public interface IPlacesRepository
    {
        public List<PlaceDefination> GetPlaces();

        public List<NeighbourDefinition> GetNeighbourDefinitions();

        public Polygon GetHitAreaForPlace(PlaceDefination place, List<PlaceDefination> places, List<NeighbourDefinition> neighbourDefinitions);

        public string GetHitAreaForPlaceAsWkt(PlaceDefination place, List<PlaceDefination> places, List<NeighbourDefinition> neighbourDefinitions);

        public void FillHitAreaFieldsForPlace(PlaceDefination place, List<PlaceDefination> places, List<NeighbourDefinition> neighbourDefinitions);

        public List<PlaceDefination> GetPlacesFromCache();

        public void CachePlaces(List<PlaceDefination> places);

    }
}
