

namespace KCAeroPlugin.Models
{
 public class TripInfo
  {
    public int TripLegs { get; set; } = 0;
    public Airport DepartAirport { get; set; } = new Airport();
    public Airport Arrival1 { get; set; } = new Airport();
    public Airport Arrival2 { get; set; } = new Airport();
    public Airport Arrival3 { get; set; } = new Airport();
    public Airport Arrival4 { get; set; } = new Airport();
  }
}
