﻿using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TableBuilderLibrary;

namespace WUDownloader
{
    public static class MyExtensions
    {
        public static DataTable AddUpdatesToTable(this DataTable table, List<UpdateInfo> updates, string tablePath, string fileName)
        {
            foreach (UpdateInfo update in updates) //Adds rows to table
            {
                for (int z = 0; z < update.DownloadUrls.Count; z++)
                {
                    object[] cellData = new object[] { update.Id, update.Title, update.Product, update.Classification, update.LastUpdated,
                        update.Version, update.Size, update.DownloadUrls.Cast<DictionaryEntry>().ElementAt(z).Key.ToString(), update.DownloadUrls.Cast<DictionaryEntry>().ElementAt(z).Value.ToString() };
                    DataRow datarow = TableBuilder.CreateDataRow(table, TableBuilder.AssignTypesToData(table, cellData, false), false);
                    table.AddRowToTable(datarow);
                }              
            }

            FileIO.ExportDataTableToCSV(table, tablePath, fileName); //Saves table to CSV

            return table;
        }
    }
}
