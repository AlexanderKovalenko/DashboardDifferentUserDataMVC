using System;
using System.Web;
using System.Web.Routing;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DashboardWeb.Mvc;
using DevExpress.Data.Filtering;
using DevExpress.DataAccess;
using DevExpress.DataAccess.Json;
using DevExpress.DataAccess.ConnectionParameters;

namespace MVCDashboard {
    public static class DashboardConfig {
        public static void RegisterService(RouteCollection routes) {
            routes.MapDashboardRoute("api/dashboard");

            DashboardConfigurator.Default.SetDashboardStorage(new DashboardFileStorage(@"~/App_Data/Dashboards"));
            DashboardConfigurator.Default.SetDataSourceStorage(new CustomDataSourceStorage());

            DashboardConfigurator.Default.CustomParameters += DashboardConfigurator_CustomParameters;
            DashboardConfigurator.Default.DataLoading += DashboardConfigurator_DataLoading;
            DashboardConfigurator.Default.CustomFilterExpression += DashboardConfigurator_CustomFilterExpression;
            DashboardConfigurator.Default.ConfigureDataConnection += DashboardConfigurator_ConfigureDataConnection;
        }

        // Configure user-specific data caching
        private static void DashboardConfigurator_CustomParameters(object sender, CustomParametersWebEventArgs e) {
            var userName = (string)HttpContext.Current.Session["CurrentUser"];
            e.Parameters.Add(new Parameter("UserRole", typeof(string), userName));
        }

        // Conditional data loading for ObjectDataSource
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

        // Conditional data loading for other datasource types
        private static void DashboardConfigurator_ConfigureDataConnection(object sender, ConfigureDataConnectionWebEventArgs e) {
            var userName = (string)HttpContext.Current.Session["CurrentUser"];

            if (e.ConnectionName == "sqlCategories") {
                var sqlConnectionParameters = e.ConnectionParameters as CustomStringConnectionParameters;

                if (userName == "Admin") {
                    sqlConnectionParameters.ConnectionString = @"XpoProvider=MSAccess;Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\nwind_admin.mdb;";
                }
                else if (userName == "User") {
                    sqlConnectionParameters.ConnectionString = @"XpoProvider=MSAccess;Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\nwind_user.mdb;";
                }
            }
            else if (e.ConnectionName == "jsonCustomers") {
                if (e.DashboardId == "JSON") {
                    string jsonFileName = "";

                    if (userName == "Admin") {
                        jsonFileName = "customers_admin.json";
                    }
                    else if (userName == "User") {
                        jsonFileName = "customers_user.json";
                    }

                    var fileUri = new Uri(HttpContext.Current.Server.MapPath(@"~/App_Data/" + jsonFileName), UriKind.RelativeOrAbsolute);
                    ((JsonSourceConnectionParameters)e.ConnectionParameters).JsonSource = new UriJsonSource(fileUri);
                }
                else if (e.DashboardId == "JSONFilter") {
                    var remoteUri = new Uri(GetBaseUrl() + "Home/GetCustomers");
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
            else if (e.ConnectionName == "excelSales") {
                var excelConnectionParameters = e.ConnectionParameters as ExcelDataSourceConnectionParameters;

                if (userName == "Admin") {
                    excelConnectionParameters.FileName = HttpContext.Current.Server.MapPath(@"~/App_Data/sales_admin.xlsx");
                }
                else if (userName == "User") {
                    excelConnectionParameters.FileName = HttpContext.Current.Server.MapPath(@"~/App_Data/sales_user.xlsx");
                }
            }
            else if (e.ConnectionName == "olapAdventureWorks") {
                if (userName == "Admin") {
                    ((OlapConnectionParameters)e.ConnectionParameters).ConnectionString = @"provider=MSOLAP;data source=http://demos.devexpress.com/Services/OLAP/msmdpump.dll;initial catalog=Adventure Works DW Standard Edition;cube name=Adventure Works;";
                }
                else if (userName == "User") {
                    throw new ApplicationException("You are not authorized to access OLAP data.");
                }
            }
            else if(e.ConnectionName == "extractSalesPerson") {
                if (userName == "Admin") {
                    ((ExtractDataSourceConnectionParameters)e.ConnectionParameters).FileName = HttpContext.Current.Server.MapPath(@"~/App_Data/SalesPersonExtract.dat");
                }
                else {
                    throw new ApplicationException("You are not authorized to access Extract data.");
                }
            }
        }

        // Custom data filtering for SqlDataSource
        private static void DashboardConfigurator_CustomFilterExpression(object sender, CustomFilterExpressionWebEventArgs e) {
            var userName = (string)HttpContext.Current.Session["CurrentUser"];

            if (e.DashboardId == "SQLFilter" && e.QueryName == "Categories") {
                if (userName == "User") {
                    e.FilterExpression = CriteriaOperator.Parse("StartsWith([CategoryName], 'C')");
                }
            }
        }

        private static string GetBaseUrl() {
            var request = HttpContext.Current.Request;
            var appRootFolder = request.ApplicationPath;

            if (!appRootFolder.EndsWith("/")) {
                appRootFolder += "/";
            }

            return string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appRootFolder);
        }
    }
}