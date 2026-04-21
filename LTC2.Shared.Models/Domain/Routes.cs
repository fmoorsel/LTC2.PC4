

using System.Collections.Generic;

namespace LTC2.Shared.Models.Domain
{
    public class Routes
    {
        public bool IsPlannerRoute { get; set; }

        public string PlannerRouteId { get; set; }

        public LimitInfo LimitInfo { get; set; }

        public List<Route> RouteCollection { get; set; } = new List<Route>();
    }
}
