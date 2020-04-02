using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MicrowaveMonitor.Gui
{
    public partial class ElevationWindow : Window
    {
        private static readonly long EarthRadius = 6371000;

        public ElevationWindow()
        {
            InitializeComponent();
            error.Visibility = Visibility.Visible;
        }

        public ElevationWindow(string latitudeA, string longitudeA, string latitudeB, string longitudeB)
        {
            InitializeComponent();

            MockLocation siteA = new MockLocation(Double.Parse(latitudeA, CultureInfo.InvariantCulture), Double.Parse(longitudeA, CultureInfo.InvariantCulture));
            MockLocation siteB = new MockLocation(Double.Parse(latitudeB, CultureInfo.InvariantCulture), Double.Parse(longitudeB, CultureInfo.InvariantCulture));

            double dist = GetDistance(siteA, siteB);
            double azimuth = CalculateBearing(siteA, siteB);
            distance.Content = String.Format("{0:0.##} m", dist);

            int interval = Convert.ToInt32(Math.Round(dist / 20, MidpointRounding.AwayFromZero));
            List<MockLocation> coords = GetLocations(interval, dist, azimuth, siteA, siteB);
            string[] coordstrings = new string[coords.Count];

            for (int i = 0; i < coords.Count; i++)
            {
                coordstrings[i] = coords[i].ToString();
            }

            Console.WriteLine(coords.Count);
            Console.WriteLine(string.Join(",", coordstrings));

            _ = ImgDownloadAsync(new Uri(String.Format("http://open.mapquestapi.com/elevation/v1/chart?key={0}&shapeFormat=raw&width=585&height=265&latLngCollection={1}", ConfigurationManager.AppSettings.Get("ElevationApiKey"), string.Join(",", coordstrings))));
        }

        private async Task ImgDownloadAsync(Uri address)
        {
            var bytes = await new WebClient().DownloadDataTaskAsync(address);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = new MemoryStream(bytes);
            image.EndInit();
            image.Freeze();
            graph.Source = image;
        }

        internal double GetDistance(MockLocation start, MockLocation end)
        {
            double dLat = ToRadians(end.lat - start.lat);
            double dLon = ToRadians(end.lng - start.lng);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(ToRadians(start.lat)) * Math.Cos(ToRadians(end.lat)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            double d = EarthRadius * c;

            return d;
        }

        private double CalculateBearing(MockLocation start, MockLocation end)
        {
            double startLat = ToRadians(start.lat);
            double startLong = ToRadians(start.lng);
            double endLat = ToRadians(end.lat);
            double endLong = ToRadians(end.lng);
            double dLong = endLong - startLong;
            double dPhi = Math.Log(Math.Tan((endLat / 2.0) + (Math.PI / 4.0)) / Math.Tan((startLat / 2.0) + (Math.PI / 4.0)));
            if (Math.Abs(dLong) > Math.PI)
            {
                if (dLong > 0.0)
                {
                    dLong = -(2.0 * Math.PI - dLong);
                }
                else
                {
                    dLong = (2.0 * Math.PI + dLong);
                }
            }
            double bearing = (ToDegrees(Math.Atan2(dLong, dPhi)) + 360.0) % 360.0;
            return bearing;
        }

        private MockLocation GetDestinationLatLng(double lat, double lng, double azimuth, double distance)
        {
            double radiusKm = EarthRadius / 1000; //Radius of the Earth in km
            double brng = ToRadians(azimuth); //Bearing is degrees converted to radians.
            double d = distance / 1000; //Distance m converted to km
            double lat1 = ToRadians(lat); //Current dd lat point converted to radians
            double lon1 = ToRadians(lng); //Current dd long point converted to radians
            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d / radiusKm) + Math.Cos(lat1) * Math.Sin(d / radiusKm) * Math.Cos(brng));
            double lon2 = lon1 + Math.Atan2(Math.Sin(brng) * Math.Sin(d / radiusKm) * Math.Cos(lat1), Math.Cos(d / radiusKm) - Math.Sin(lat1) * Math.Sin(lat2));
            lat2 = ToDegrees(lat2);
            lon2 = ToDegrees(lon2);
            return new MockLocation(lat2, lon2);
        }

        private List<MockLocation> GetLocations(int interval, double distance, double azimuth, MockLocation start, MockLocation end)
        {
            List<MockLocation> coords = new List<MockLocation>();
            MockLocation mock = new MockLocation(start.lat, start.lng);
            coords.Add(mock);
            for (int coveredDist = interval; coveredDist < distance; coveredDist += interval)
            {
                MockLocation coord = GetDestinationLatLng(start.lat, start.lng, azimuth, coveredDist);
                coords.Add(coord);
            }
            coords.Add(new MockLocation(end.lat, end.lng));
            return coords;
        }

        private double ToRadians(double value)
        {
            return (Math.PI / 180) * value;
        }

        private double ToDegrees(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            return (degrees);
        }
    }

    internal class MockLocation
    {
        public double lat;
        public double lng;

        public MockLocation(double lat, double lng)
        {
            this.lat = lat;
            this.lng = lng;
        }

        public override string ToString()
        {
            return lat.ToString(CultureInfo.InvariantCulture) + "," + lng.ToString(CultureInfo.InvariantCulture);
        }
    }
}
