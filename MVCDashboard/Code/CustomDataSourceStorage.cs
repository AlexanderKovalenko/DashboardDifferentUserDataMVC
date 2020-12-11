﻿using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DataAccess.EntityFramework;
using DevExpress.DataAccess.Excel;
using DevExpress.DataAccess.Sql;
using System.Collections.Generic;
using System.Xml.Linq;

public class CustomDataSourceStorage : IDataSourceStorage {
    private Dictionary<string, XDocument> documents = new Dictionary<string, XDocument>();

    private const string sqlDataSourceId = "SQL Data Source";
    private const string jsonDataSourceId = "JSON Data Source";
    private const string odsDataSourceId = "Object Data Source";
    private const string olapDataSourceId = "OLAP Data Source";
    private const string excelDataSourceId = "Excel Data Source";
    private const string extractDataSourceId = "Extract Data Source";
    private const string efDataSourceId = "Entity Framework Data Source";

    public CustomDataSourceStorage() {
        DashboardSqlDataSource sqlDataSource = new DashboardSqlDataSource(sqlDataSourceId);
        SelectQuery query = SelectQueryFluentBuilder
            .AddTable("Categories")
            .SelectAllColumnsFromTable()
            .Build("Categories");
        sqlDataSource.Queries.Add(query);

        DashboardJsonDataSource jsonDataSource = new DashboardJsonDataSource(jsonDataSourceId) { RootElement = "Customers" };
        DashboardObjectDataSource objDataSource = new DashboardObjectDataSource(odsDataSourceId);
        DashboardOlapDataSource olapDataSource = new DashboardOlapDataSource(olapDataSourceId, "olapConnection");
        DashboardExtractDataSource extractDataSource = new DashboardExtractDataSource(extractDataSourceId);
        DashboardExcelDataSource excelDataSource = new DashboardExcelDataSource(excelDataSourceId) {
            SourceOptions = new ExcelSourceOptions(new ExcelWorksheetSettings("Sheet1"))
        };
        DashboardEFDataSource efDataSource = new DashboardEFDataSource(efDataSourceId, new EFConnectionParameters(typeof(NorthwindDbContext)));

        documents[sqlDataSourceId] = new XDocument(sqlDataSource.SaveToXml());
        documents[jsonDataSourceId] = new XDocument(jsonDataSource.SaveToXml());
        documents[odsDataSourceId] = new XDocument(objDataSource.SaveToXml());
        documents[olapDataSourceId] = new XDocument(olapDataSource.SaveToXml());
        documents[extractDataSourceId] = new XDocument(extractDataSource.SaveToXml());
        documents[excelDataSourceId] = new XDocument(excelDataSource.SaveToXml());
        documents[efDataSourceId] = new XDocument(efDataSource.SaveToXml());
    }

    public XDocument GetDataSource(string dataSourceID) {
        return documents[dataSourceID];
    }

    public IEnumerable<string> GetDataSourcesID() {
        return documents.Keys;
    }
}