using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Spatial
{
	public class GeoHelper
	{
		public const int SridGoogleMaps = 4326;
		public const int SridCustomMap = 3857;



		public static DbGeography FromLatLng(double lat, double lng)
		{
			// http://codepaste.net/73hssg
			return DbGeography.PointFromText("POINT(" + lng.ToString() + " " + lat.ToString() + ")", SridGoogleMaps);
		}
	}
}
