using LTC2.Shared.Models.Domain;

namespace LTC2.Desktopclients.WindowsClient.Models
{
    public class ActivityTypeDescription
    {
        public string Value { get; set; }

        public string Description { get; set; }

        public GenericActivityType ActivityType
        {
            get
            {
                return (GenericActivityType)Enum.Parse(typeof(GenericActivityType), Value);
            }
        }
    }
}
