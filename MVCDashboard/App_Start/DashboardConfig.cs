using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DashboardWeb.Mvc;
using DevExpress.DataAccess;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Json;
using DevExpress.DataAccess.Web;
using System;
using System.Web;
using System.Web.Routing;

namespace MVCDashboard {
    public static class DashboardConfig {
        public static void RegisterService(RouteCollection routes) {
            routes.MapDashboardRoute("api/dashboard");

            DashboardConfigurator.Default.SetConnectionStringsProvider(new ConfigFileConnectionStringsProvider());
            DashboardConfigurator.Default.SetDataSourceStorage(new CustomDataSourceStorage());
            DashboardConfigurator.Default.SetDashboardStorage(new DashboardFileStorage(@"~/App_Data/Dashboards"));

            DashboardConfigurator.Default.CustomParameters += DashboardConfigurator_CustomParameters;
            DashboardConfigurator.Default.DataLoading += DashboardConfigurator_DataLoading;
            DashboardConfigurator.Default.ConfigureDataConnection += DashboardConfigurator_ConfigureDataConnection;
        }

        private static void DashboardConfigurator_CustomParameters(object sender, CustomParametersWebEventArgs e) {
            var userName = (string)HttpContext.Current.Session["CurrentUser"];
            e.Parameters.Add(new Parameter("UserRole", typeof(string), userName));
        }

        private static void DashboardConfigurator_DataLoading(object sender, DataLoadingWebEventArgs e) {
            var userName = (string)HttpContext.Current.Session["CurrentUser"];

            if (userName == "Admin") {
                e.Data = SalesPersonData.GetSalesData();
            }
            else if (userName == "User") {
                e.Data = SalesPersonData.GetSalesDataLimited();
            }
        }

        private static void DashboardConfigurator_ConfigureDataConnection(object sender, ConfigureDataConnectionWebEventArgs e) {
            var userName = (string)HttpContext.Current.Session["CurrentUser"];

            if (e.DataSourceName == "SQL Data Source") {
                if (userName == "Admin") {
                    ((CustomStringConnectionParameters)e.ConnectionParameters).ConnectionString = @"XpoProvider=MSAccess; Provider=Microsoft.Jet.OLEDB.4.0; Data Source=|DataDirectory|\nwind.mdb;";
                }
                else if (userName == "User") {
                    ((CustomStringConnectionParameters)e.ConnectionParameters).ConnectionString = @"XpoProvider=MSAccess; Provider=Microsoft.Jet.OLEDB.4.0; Data Source=|DataDirectory|\nwind2.mdb;";
                }
            }
            else if (e.DataSourceName == "JSON Data Source") {
                if (userName == "Admin") {
                    Uri remoteUri = new Uri("https://raw.githubusercontent.com/DevExpress-Examples/DataSources/master/JSON/customers.json");
                    ((JsonSourceConnectionParameters)e.ConnectionParameters).JsonSource = new UriJsonSource(remoteUri);
                }
                else if (userName == "User") {
                    Uri fileUri = new Uri(HttpContext.Current.Server.MapPath(@"~/App_Data/customers2.json"), UriKind.RelativeOrAbsolute);
                    ((JsonSourceConnectionParameters)e.ConnectionParameters).JsonSource = new UriJsonSource(fileUri);
                }
            }
            else if (e.DataSourceName == "Excel Data Source") {
                if (userName == "Admin") {
                    ((ExcelDataSourceConnectionParameters)e.ConnectionParameters).FileName = HttpContext.Current.Server.MapPath(@"~/App_Data/Sales.xlsx");
                }
                else if (userName == "User") {
                    ((ExcelDataSourceConnectionParameters)e.ConnectionParameters).FileName = HttpContext.Current.Server.MapPath(@"~/App_Data/Sales2.xlsx");
                }
            }
            else if (e.DataSourceName == "OLAP Data Source") {
                if (userName == "Admin") {
                    ((OlapConnectionParameters)e.ConnectionParameters).ConnectionString = @"provider=MSOLAP;data source=http://demos.devexpress.com/Services/OLAP/msmdpump.dll;initial catalog=Adventure Works DW Standard Edition;cube name=Adventure Works;";
                }
                else if (userName == "User") {
                    throw new ApplicationException("You are not authorized to access OLAP data.");
                }
            }
            else if(e.DataSourceName == "Extract Data Source") {
                if (userName == "Admin") {
                    ((ExtractDataSourceConnectionParameters)e.ConnectionParameters).FileName = HttpContext.Current.Server.MapPath(@"~/App_Data/SalesPersonExtract.dat");
                }
                else {
                    throw new ApplicationException("You are not authorized to access Extract data.");
                }
            }
        }
    }
}