using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using CefSharp;
using CefSharp.Wpf;

namespace MicrowaveMonitor.Gui
{
    public partial class MapWindow : Window
    {
        private string latitudeA;
        private string longitudeA;
        private string latitudeB;
        private string longitudeB;
        private bool dual = false;

        public MapWindow(string latitude, string longitude)
        {
            InitializeComponent();
            //Cef.Initialize(new CefSettings());
            
            latitudeA = latitude;
            longitudeA = longitude;

            mapBrowser.IsBrowserInitializedChanged += MapBrowserInit;
        }

        public MapWindow(string latitudeA, string longitudeA, string latitudeB, string longitudeB) : this(latitudeA, longitudeA)
        {
            this.latitudeB = latitudeB;
            this.longitudeB = longitudeB;
            dual = true;
        }

        private void MapBrowserInit(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (mapBrowser.IsBrowserInitialized)
            {
                string page = PageBuilder();
                mapBrowser.LoadHtml(page, "https://www.vutbr.cz");
            }
        }

        private string PageBuilder()
        {
            string markB = String.Empty;

            if (dual)
            {
                markB = $@"
                                            var znacka_b = JAK.mel(""div"");
                                            var obrazek_b = JAK.mel(""img"", {{src:SMap.CONFIG.img+""/marker/drop-red.png""}});
                                            znacka_b.appendChild(obrazek_b);
                                            var popisek_b = JAK.mel(""div"", {{}}, {{position:""absolute"", left:""0px"", top:""2px"", textAlign:""center"", width:""22px"", color:""white"", fontWeight:""bold""}});
                                            popisek_b.innerHTML = ""B"";
                                            znacka_b.appendChild(popisek_b);

                                            var b = SMap.Coords.fromWGS84({longitudeB}, {latitudeB});
                                            var marker_b = new SMap.Marker(b, ""Device B"", {{url:znacka_b}});
                                            layer.addMarker(marker_b);

                                            var pointer_b = new SMap.Control.Pointer({{type: SMap.Control.Pointer.TYPES.RED}});
                                            m.addControl(pointer_b);
                                            pointer_b.setCoords(b);";
            }

            string page = $@"<!doctype html>
                             <html>
                               <head>
                               <meta charset=""UTF-8"">
                                 <script src=""https://api.mapy.cz/loader.js""></script>
                                 <script>Loader.load()</script>
                               </head>
                               <body>
                                    <div id=""mapa"" style=""position:fixed; padding:0; margin:0; top:0; left:0; width: 100%; height: 100%;""></div>
                                    <script type=""text/javascript"">
                                            var a = SMap.Coords.fromWGS84({longitudeA}, {latitudeA});
                                            var m = new SMap(JAK.gel(""mapa""), a, 15);
                                            m.addDefaultLayer(SMap.DEF_BASE).enable();
                                            m.addDefaultControls();
                             
                                            var layer = new SMap.Layer.Marker();
                                            m.addLayer(layer);
                                            layer.enable();

                                            var sync = new SMap.Control.Sync();
                                            m.addControl(sync);
                             
                                            var znacka_a = JAK.mel(""div"");
                                            var obrazek_a = JAK.mel(""img"", {{src:SMap.CONFIG.img+""/marker/drop-red.png""}});
                                            znacka_a.appendChild(obrazek_a);
                                            var popisek_a = JAK.mel(""div"", {{}}, {{position:""absolute"", left:""0px"", top:""2px"", textAlign:""center"", width:""22px"", color:""white"", fontWeight:""bold""}});
                                            popisek_a.innerHTML = ""A"";
                                            znacka_a.appendChild(popisek_a);

                                            var marker_a = new SMap.Marker(a, ""Device A"", {{url:znacka_a}});
                                            layer.addMarker(marker_a);

                                            var pointer_a = new SMap.Control.Pointer({{type: SMap.Control.Pointer.TYPES.RED}});
                                            m.addControl(pointer_a);
                                            pointer_a.setCoords(a);
                                            {markB}
                                    </script>
                               </body>
                             </html>";
            return page;
        }
    }
}
