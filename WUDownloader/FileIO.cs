﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TableBuilderLibrary;

namespace WUDownloader
{
    class FileIO
    {
        public static List<string> ImportFileToStringList(string filepath)
        {
            List<string> lines = System.IO.File.ReadAllLines(filepath).ToList();
            return lines;
        }

        public static void ExportStringListToFile(string filepath, List<string> lines)
        {
            System.IO.File.WriteAllLines(filepath, lines);
        }

        public static List<string> ImportCsvToStringList(string filepath)
        {
            List<string> csv = System.IO.File.ReadAllLines(filepath + ".csv").ToList();
            return csv;
        }

        public static DataTable ImportTableFromCsv()
        {
            //Build table with schema
            DataTable table = TableBuilder.BuildTableSchema(Configuration.TableName, Configuration.TableHeaders, Configuration.TableColumnTypes);
            //Check if file exists
            if (File.Exists(Configuration.TableFolderPath + "\\" + Configuration.TableName + ".csv")) //If exists
            {
                Console.WriteLine("CSV file exists. Importing...");
                //Import file
                //List<string> csv = FileIO.ImportCsvToStringList(Configuration.TablePath + "\\" + Configuration.TableName);

                ////Build table with schema
                //DataTable table = TableBuilder.BuildTableSchema(Configuration.TableName, Configuration.TableHeaders, Configuration.TableColumnTypes);

                //Populate table from file
                table.PopulateTableFromCsv(Configuration.TableFolderPath, Configuration.TableName, '|', true);
                return table;
            }
            else //If not exists
            {
                Console.WriteLine("CSV file does not exists. Generating...");
                //Build table from scratch
                //DataTable table = TableBuilder.BuildTableSchema(Configuration.TableName, headers, columnTypes);
                ExportDataTableToCSV(table, Configuration.TableFolderPath, Configuration.TableName);
                Console.WriteLine("CSV file saved.");
                return table;
            }
        }

        public static void ExportDataTableToCSV(DataTable table, string folderPath, string fileName)
        {
            using (StreamWriter writer = new StreamWriter(folderPath + "\\" + fileName + ".csv"))
            {
                WriteDataTable(table, writer, true);
                writer.Close();
            }
        }

        private static void WriteDataTable(DataTable sourceTable, TextWriter writer, bool includeHeaders)
        {
            if (includeHeaders)
            {
                IEnumerable<String> headerValues = sourceTable.Columns
                    .OfType<DataColumn>()
                    .Select(column => QuoteValue(column.ColumnName));

                writer.WriteLine(String.Join("|", headerValues));
            }

            IEnumerable<String> items = null;

            foreach (DataRow row in sourceTable.Rows)
            {
                items = row.ItemArray.Select(o => QuoteValue(o.ToString()));
                writer.WriteLine(String.Join("|", items));
            }

            writer.Flush();
        }

        private static string QuoteValue(string value)
        {
            return String.Concat("\"",
            value.Replace("\"", "\"\""), "\"");
        }
    }
}
