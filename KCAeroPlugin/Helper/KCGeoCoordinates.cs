using KCAeroPlugin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCAeroPlugin.Helper
{
  public static class KCGeoCoordinates
  {
    private const double NMILES = 59.997756F;
    public static double DistanceTo(this Coordinates baseCoordinates, Coordinates targetCoordinates)
    {
      //returns greeat circle distance in nautical miles

      var baseRad = Math.PI * baseCoordinates.Latitude / 180;
      var targetRad = Math.PI * targetCoordinates.Latitude / 180;
      var theta = baseCoordinates.Longitude - targetCoordinates.Longitude;
      var thetaRad = Math.PI * theta / 180;

      double dist =
          Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
          Math.Cos(targetRad) * Math.Cos(thetaRad);
      dist = Math.Acos(dist);

      dist = (dist * 180) / Math.PI;
      dist = dist * NMILES;

      return dist;
    }
  }
}
