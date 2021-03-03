using System;

namespace KCAeroPlugin.Models
{
  public class Airport
  {
    public string AirportName { get; set; } = "";
    public Guid RecordID { get; set; }

    public Airport()
    {

    }
    public Airport(string airportName, Guid recordID)
    {
      AirportName = airportName;
      RecordID = recordID;
    }
  }
}
