using System;
using System.Web;
using System.Web.Routing;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DashboardWeb.Mvc;
using DevExpress.Data.Filtering;
using DevExpress.DataAccess;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Json;
using DevExpress.DataAccess.Web;

namespace MVCDashboard {
    public static class DashboardConfig {
        public static void RegisterService(RouteCollection routes) {
            routes.MapDashboardRoute("api/dashboard");

            DashboardConfigurator.Default.SetConnectionStringsProvider(new ConfigFileConnectionStringsProvider());
            DashboardConfigurator.Default.SetDataSourceStorage(new CustomDataSourceStorage());
            DashboardConfigurator.Default.SetDashboardStorage(new DashboardFileStorage(@"~/App_Data/Dashboards"));

            DashboardConfigurator.Default.CustomParameters += DashboardConfigurator_CustomParameters;
            DashboardConfigurator.Default.DataLoading += DashboardConfigurator_DataLoading;
            DashboardConfigurator.Default.CustomFilterExpression += DashboardConfigurator_CustomFilterExpression;
            DashboardConfigurator.Default.ConfigureDataConnection += DashboardConfigurator_ConfigureDataConnection;
        }

        private static void DashboardConfigurator_CustomParameters(object sender, CustomParametersWebEventArgs e) {
            var userName = (string)HttpContext.Current.Session["CurrentUser"];
            e.Parameters.Add(new Parameter("UserRole", typeof(string), userName));
        }

        private static void DashboardConfigurator_DataLoading(object sender, DataLoadingWebEventArgs e) {
            var userName = (string)HttpContext.Current.Session["CurrentUser"];

            if (userName == "Admin") {
                e.Data = SalesData.GetSalesData();
            }
            else if (userName == "User") {
                e.Data = SalesData.GetSalesDataLimited();
            }
        }

        private static void DashboardConfigurator_CustomFilterExpression(object sender, CustomFilterExpressionWebEventArgs e) {
            var userName = (string)HttpContext.Current.Session["CurrentUser"];

            if (userName == "User" && e.QueryName == "Categories") {
                e.FilterExpression = CriteriaOperator.Parse("StartsWith([CategoryName], 'C')");
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
                    Uri fileUri = new Uri(HttpContext.Current.Server.MapPath(@"~/App_Data/customers.json"), UriKind.RelativeOrAbsolute);
                    ((JsonSourceConnectionParameters)e.ConnectionParameters).JsonSource = new UriJsonSource(fileUri);

                }
                else if (userName == "User") {
                    //Uri fileUri = new Uri(HttpContext.Current.Server.MapPath(@"~/App_Data/customers2.json"), UriKind.RelativeOrAbsolute);
                    //((JsonSourceConnectionParameters)e.ConnectionParameters).JsonSource = new UriJsonSource(fileUri);
                    Uri remoteUri = new Uri("http://northwind.netcore.io/query/customers.json");
                    var jsonSource = new UriJsonSource(remoteUri);

                    jsonSource.PathParameters.AddRange(new[] {
                        // "CountryPattern" is a dashboard parameter whose value is used for the "CountryStartsWith" path parameter.
                        new PathParameter("CountryStartsWith", typeof(string), new Expression("Parameters.CountryPattern"))
                    });

                    ((JsonSourceConnectionParameters)e.ConnectionParameters).JsonSource = jsonSource;
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