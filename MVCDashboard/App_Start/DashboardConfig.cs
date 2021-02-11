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

namespace MVCDashboard {
    public static class DashboardConfig {
        public static void RegisterService(RouteCollection routes) {
            routes.MapDashboardRoute("api/dashboard");

            DashboardConfigurator.Default.SetDashboardStorage(new DashboardFileStorage(@"~/App_Data/Dashboards"));

            //DashboardConfigurator.Default.SetDataSourceStorage(new CustomDataSourceStorage());

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

            if (e.DataId == "odsSales") {
                if (userName == "Admin") {
                    e.Data = SalesData.GetSalesData();
                }
                else if (userName == "User") {
                    e.Data = SalesData.GetSalesDataLimited();
                }
            }
        }

        private static void DashboardConfigurator_CustomFilterExpression(object sender, CustomFilterExpressionWebEventArgs e) {
            var userName = (string)HttpContext.Current.Session["CurrentUser"];

            if (e.DashboardId == "SQLFilter" && e.QueryName == "Categories") {
                if (userName == "User") {
                    e.FilterExpression = CriteriaOperator.Parse("StartsWith([CategoryName], 'C')");
                }
            }
        }

        private static void DashboardConfigurator_ConfigureDataConnection(object sender, ConfigureDataConnectionWebEventArgs e) {
            var userName = (string)HttpContext.Current.Session["CurrentUser"];

            if (e.ConnectionName == "sqlConnection") {
                if (userName == "Admin") {
                    ((CustomStringConnectionParameters)e.ConnectionParameters).ConnectionString = @"XpoProvider=MSAccess; Provider=Microsoft.Jet.OLEDB.4.0; Data Source=|DataDirectory|\nwind.mdb;";
                }
                else if (userName == "User") {
                    ((CustomStringConnectionParameters)e.ConnectionParameters).ConnectionString = @"XpoProvider=MSAccess; Provider=Microsoft.Jet.OLEDB.4.0; Data Source=|DataDirectory|\nwind2.mdb;";
                }
            }
            else if (e.ConnectionName == "jsonConnection") {
                if (e.DashboardId == "JSON") {
                    string jsonFileName = "";

                    if (userName == "Admin") {
                        jsonFileName = "customers.json";
                    }
                    else if (userName == "User") {
                        jsonFileName = "customers2.json";
                    }

                    Uri fileUri = new Uri(HttpContext.Current.Server.MapPath(@"~/App_Data/" + jsonFileName), UriKind.RelativeOrAbsolute);
                    ((JsonSourceConnectionParameters)e.ConnectionParameters).JsonSource = new UriJsonSource(fileUri);
                }
                else if (e.DashboardId == "JSONFilter") {
                    Uri remoteUri = new Uri("http://northwind.netcore.io/query/customers.json");
                    var jsonSource = new UriJsonSource(remoteUri);

                    if (userName == "User") {
                        jsonSource.QueryParameters.AddRange(new[] {
                            // "CountryPattern" is a dashboard parameter whose value is used for the "CountryStartsWith" query parameter
                            new QueryParameter("CountryStartsWith", typeof(Expression), new Expression("Parameters.CountryPattern"))
                        });
                    }

                    ((JsonSourceConnectionParameters)e.ConnectionParameters).JsonSource = jsonSource;
                }
            }
            else if (e.ConnectionName == "excelConnection") {
                if (userName == "Admin") {
                    ((ExcelDataSourceConnectionParameters)e.ConnectionParameters).FileName = HttpContext.Current.Server.MapPath(@"~/App_Data/Sales.xlsx");
                }
                else if (userName == "User") {
                    ((ExcelDataSourceConnectionParameters)e.ConnectionParameters).FileName = HttpContext.Current.Server.MapPath(@"~/App_Data/Sales2.xlsx");
                }
            }
            else if (e.ConnectionName == "olapConnection") {
                if (userName == "Admin") {
                    ((OlapConnectionParameters)e.ConnectionParameters).ConnectionString = @"provider=MSOLAP;data source=http://demos.devexpress.com/Services/OLAP/msmdpump.dll;initial catalog=Adventure Works DW Standard Edition;cube name=Adventure Works;";
                }
                else if (userName == "User") {
                    throw new ApplicationException("You are not authorized to access OLAP data.");
                }
            }
            else if(e.ConnectionName == "extractConnection") {
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