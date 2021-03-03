using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KCAeroPlugin.Helper;
using KCAeroPlugin.Models;
using Microsoft.Xrm.Sdk.Query;

namespace KCAeroPlugin
{
  public class DistanceCalculator : IPlugin
  {
    //calculates the great circle distance in nautical miles between points
    //Distance 1:  Trip1 to Trip2 
    //Distance 2:  Trip2 to Trip3
    //Distance 3:  Trip3 to Trip4

    //use the plugin registration tool to register to CRM instance and create the Step 
    //to attach to it PreOperation

    private const int TRIP1 = 277670000;
    private const int TRIP2 = 277670001;
    private const int TRIP3 = 277670002;
    private const int TRIP4 = 277670003;
    public void Execute(IServiceProvider serviceProvider)
    {
      ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
      try
      {
        IPluginExecutionContext context =
        (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

        //verify that this the correct event
        if (context.MessageName.ToUpper() != "UPDATE")
        {
          return;
        }

        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
        {
          Entity entity = (Entity)context.InputParameters["Target"];

          //verify that this is the target entity
          if (entity.LogicalName.ToUpper() != "OPPORTUNITY") { return; }

          //the entity contains fields that have been updated
          //iterate through list to see if any airport fields have changed
          List<string> fields = new List<string>();
          fields.Add("dev_departairport");
          fields.Add("dev_arrive1");
          fields.Add("dev_arrive2");
          fields.Add("dev_arrive3");
          fields.Add("dev_arrive4");
          bool airportChange = false;
          foreach (KeyValuePair<string, object> item in entity.Attributes)
          {
            if (fields.Contains(item.Key))
            {
              airportChange = true;
              break;
            }
          }
          //this keep the plugin code from recalculating data
          if (!airportChange) { return; }

          //organizational service reference for web service calls
          IOrganizationServiceFactory serviceFactory =
          (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

          IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

          TripInfo tripInfo = GetTripInfo(entity.Id, service);

          string distance1 = "";
          if (tripInfo.TripLegs >= 1)
          {
            double distance = GetDistance(tripInfo.DepartAirport, tripInfo.Arrival1, service);
            distance1 = Math.Round(distance, 1).ToString() + " NM";
          }

          string distance2 = "";
          if (tripInfo.TripLegs >= 2)
          {
            double distance = GetDistance(tripInfo.Arrival1, tripInfo.Arrival2, service);
            distance2 = Math.Round(distance, 1).ToString() + " NM";
          }
          string distance3 = "";
          if (tripInfo.TripLegs >= 3)
          {
            double distance = GetDistance(tripInfo.Arrival2, tripInfo.Arrival3, service);
            distance3 = Math.Round(distance, 1).ToString() + " NM";
          }
          string distance4 = "";
          if (tripInfo.TripLegs == 4)
          {
            double distance = GetDistance(tripInfo.Arrival3, tripInfo.Arrival4, service);
            distance4 = Math.Round(distance, 1).ToString() + " NM";
          }

          Entity updateOpportunity = new Entity("opportunity");
          updateOpportunity.Id = entity.Id;
          updateOpportunity.Attributes["dev_distance1"] = distance1;
          updateOpportunity.Attributes["dev_distance2"] = distance2;
          updateOpportunity.Attributes["dev_distance3"] = distance3;
          updateOpportunity.Attributes["dev_distance4"] = distance4;
          service.Update(updateOpportunity);



        }

      }
      catch (Exception ex)
      {
        tracingService.Trace("Plugin: {0}, Error: {1}", "DistanceCalulator", ex.Message);
      }

    }

    private double GetDistance(Airport depart, Airport arrive, IOrganizationService service)
    {
      try
      {
        if (depart.AirportName.Length == 0 || arrive.AirportName.Length == 0) { return 0f; }
        ColumnSet columnSet = new ColumnSet("dev_latitude", "dev_longitude");
        //dev_airport
        Entity lookupDepart = service.Retrieve("dev_airport", depart.RecordID, columnSet);
        Entity lookupArrival = service.Retrieve("dev_airport", arrive.RecordID, columnSet);


        double lat1 = Convert.ToDouble(lookupDepart["dev_latitude"]);
        double long1 = Convert.ToDouble(lookupDepart["dev_longitude"]);
        Coordinates coordinates1 = new Coordinates(lat1, long1);
        double lat2 = Convert.ToDouble(lookupArrival["dev_latitude"]);
        double long2 = Convert.ToDouble(lookupArrival["dev_longitude"]);
        Coordinates coordinates2 = new Coordinates(lat2, long2);

        double distance = KCGeoCoordinates.DistanceTo(coordinates1, coordinates2);
        return distance;
      }
      catch
      {

        return 0f;
      }
    }
    private TripInfo GetTripInfo(Guid opportunityID, IOrganizationService service)
    {
      TripInfo tripInfo = new TripInfo();
      try
      {

        ColumnSet columnSet = new ColumnSet("dev_tripstops", "dev_departairport", "dev_arrive1", "dev_arrive2", "dev_arrive3", "dev_arrive4");
        Entity results = service.Retrieve("opportunity", opportunityID, columnSet);
        var tripCount = results.Attributes["dev_tripstops"];

        //the airport fields are lookups so we need to make sure they are in the column set
        if (results.Attributes.Contains("dev_departairport"))
        {
          EntityReference departure = (EntityReference)results.Attributes["dev_departairport"];
          Airport airport0 = new Airport(departure.Name, departure.Id);
          tripInfo.DepartAirport = airport0;

        }
        if (results.Attributes.Contains("dev_arrive1"))
        {
          EntityReference arrival1 = (EntityReference)results.Attributes["dev_arrive1"];
          Airport airport1 = new Airport(arrival1.Name, arrival1.Id);
          tripInfo.Arrival1 = airport1;

        }
        if (results.Attributes.Contains("dev_arrive2"))
        {
          EntityReference arrival2 = (EntityReference)results.Attributes["dev_arrive2"];
          Airport aiport2 = new Airport(arrival2.Name, arrival2.Id);
          tripInfo.Arrival2 = aiport2;

        }
        if (results.Attributes.Contains("dev_arrive3"))
        {
          EntityReference arrival3 = (EntityReference)results.Attributes["dev_arrive3"];
          Airport airport3 = new Airport(arrival3.Name, arrival3.Id);
          tripInfo.Arrival3 = airport3;
        }


        if (results.Attributes.Contains("dev_arrive4"))
        {
          EntityReference arrival4 = (EntityReference)results.Attributes["dev_arrive4"];
          Airport airport4 = new Airport(arrival4.Name, arrival4.Id);

        }



        int tripLegs = 0;
        if (tripCount != null)
        {
          OptionSetValue selectedTrips = (OptionSetValue)tripCount;

          int tripOptionValue = selectedTrips.Value;
          switch (tripOptionValue)
          {
            case TRIP1:

              tripLegs = 1;
              break;
            case TRIP2:

              tripLegs = 2;
              break;
            case TRIP3:
              tripLegs = 3;
              break;
            case TRIP4:
              tripLegs = 4;
              break;

          }
          tripInfo.TripLegs = tripLegs;
        }
        return tripInfo;
      }
      catch (Exception ex)
      {
        return tripInfo;
      }
    }
  }
}
