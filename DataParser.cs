using System;

namespace MerakiLocationVisualizer
{
    public class DataParser
    {
        internal static void ProcessData(DevicesSeen observationReport)
        {

            // which AP is sending the report
            string queryString = "SELECT * FROM ReportingAPs WHERE apMac = '" + observationReport.Data.ApMac+ "'";

            if (!DBConnection.DoesRecordExist(queryString))
            {
                // new reporting AP
                queryString = "INSERT INTO ReportingAPs (apMac) Values ('" + 
                                observationReport.Data.ApMac + "')";

                DBConnection.UpdateDB(queryString);
            }

            // hacking temporal proximity...
            long reportReceived = DateTimeOffset.Now.ToUnixTimeSeconds();

            // Carve up the observationReport and push it into db
            int x;
            
            for (x = 0; x < observationReport.Data.Observations.Count; x++)
            {
                if (observationReport.Data.Observations[x].Location != null)
                {
                    queryString = "INSERT INTO ObservationReports " +
                                    "(ipv4, locationlat, locationlng, locationunc, x, y, seenTime, ssid, os, " +
                                    "clientMac, name, seenEpoch, rssi, ipv6, manufacturer, seenByApMac, " +
                                    "reportReceived) " +
                                    "Values (";

                    // a crude hack.  the reports dont contain the hostname preceding the / so just del the /
                    // v3 of the api drops the hostname entirely anyway
                    if (observationReport.Data.Observations[x].Ipv4 != null)
                    {
                        queryString += "'" + (observationReport.Data.Observations[x].Ipv4).Substring(1) + "', ";
                    }
                    else
                    {
                        queryString += "'" + observationReport.Data.Observations[x].Ipv4 + "', ";
                    }

                    queryString +=
                    "'" + observationReport.Data.Observations[x].Location.Lat + "', " +
                    "'" + observationReport.Data.Observations[x].Location.Lng + "', " +
                    "'" + observationReport.Data.Observations[x].Location.Unc + "', " +
                    // x and y are actually lists as the values can be returned as offsets
                    // on multiple floorplans.  just grabbing the first value for simplicity 
                    "'" + observationReport.Data.Observations[x].Location.X[0] + "', " +
                    "'" + observationReport.Data.Observations[x].Location.Y[0] + "', " +
                    "'" + observationReport.Data.Observations[x].SeenTime + "', " +
                    "'" + observationReport.Data.Observations[x].Ssid + "', " +
                    "'" + observationReport.Data.Observations[x].Os + "', " +
                    "'" + observationReport.Data.Observations[x].ClientMac + "', " +
                    "'" + observationReport.Data.Observations[x].Name + "', " +
                    "'" + observationReport.Data.Observations[x].SeenEpoch + "', " +
                    "'" + observationReport.Data.Observations[x].Rssi + "', " +
                    "'" + observationReport.Data.Observations[x].Ipv6 + "', " +
                    "'" + observationReport.Data.Observations[x].Manufacturer + "', " +
                    "'" + observationReport.Data.ApMac + "', " +
                    "'" + reportReceived + "'" +
                    ")";

                    DBConnection.UpdateDB(queryString);
                }
            }

            Console.WriteLine("{0} reports received in this POST", x);
        }

    }
}
