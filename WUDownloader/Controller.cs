﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Data;

namespace WUDownloader
{
    class Controller
    {
        public void Run()
        {
            ConfigurationSetup();
            FolderStructureSetup();
            int menuSelection = selectMode();

            if (menuSelection == 1) //Collect update information
            {
                List<string> updateTitles = CollectUpdateTitles();
                if (updateTitles.Count > 0)
                {
                    SetupTable();
                    CollectUpdateDataForTable(updateTitles);
                    
                }
                else
                {
                    Console.WriteLine("No updates were found.");
                }
            }
            else if (menuSelection == 2) //Download Updates
            {
               // StartDownloadManager(updateTitles);
            }
            else if (menuSelection == 3) // Collect update information and download updates
            {

            }


            

            Console.WriteLine("Exiting...");
            System.Console.ReadKey();
        }
        public int selectMode()
        {
            Console.WriteLine("Select an option below:");
            Console.WriteLine("1. Collect Update Information");
            Console.WriteLine("2. Download Updates");
            Console.WriteLine("3. Collect Update Information & Download Updates");

            ConsoleInput c = new ConsoleInput();
            int input = 0;
            while (input < 1 || input > 3)
            {
                input = c.getUserInputInteger();
                if (input < 1 || input > 3)
                {
                    Console.WriteLine("Incorrect selection. Please try again.");
                }
            }
            return input;
        }
        private List<string> CollectUpdateTitles()
        {
            Console.WriteLine("Would you like to import update titles from file or scan current device?");
            ConsoleInput c = new ConsoleInput();
            int input = 0;
            do
            {
                Console.WriteLine("\nEnter 1 for Import or 2 for Scan: ");
                input = c.getUserInputInteger();
            } while (input != 1 && input != 2);

            List<string> updateTitles = new List<string>();
            if (input == 1)
            {
                Console.WriteLine("\nYou have chosen to Import. Importing...");
                updateTitles = ImportUpdateTitles();
                return updateTitles;
            }
            else if (input == 2)
            {
                Console.WriteLine("\nYou have chosen to Scan. Scanning...");
                updateTitles = WindowsUpdate.GetPendingUpdateTitles();
                return updateTitles;
            }
            else
            {
                Console.WriteLine("Something went wrong. Input equals: '" + input + "'");
                return updateTitles;
            }
        }
        private void FolderStructureSetup()
        {
            if (!Directory.Exists(Configuration.RootPath))
            {
                Console.WriteLine("WUDownload folder structure is missing. Reconstructing using configuration settings...");
                Directory.CreateDirectory(Configuration.RootPath);
            }
            if (Directory.Exists(Configuration.RootPath))
            {
                if (!Directory.Exists(Configuration.DownloadPath))
                {
                    Directory.CreateDirectory(Configuration.DownloadPath);
                }
                if (!Directory.Exists(Configuration.ImportPath))
                {
                    Directory.CreateDirectory(Configuration.ImportPath);
                }
                if (!Directory.Exists(Configuration.TablePath))
                {
                    Directory.CreateDirectory(Configuration.TablePath);
                }
            }
            if (Directory.Exists(Configuration.RootPath) && Directory.Exists(Configuration.DownloadPath) &&
                    Directory.Exists(Configuration.ImportPath) && Directory.Exists(Configuration.TablePath))
            {
                Console.WriteLine("Folder creation successful. Root folder located at: " + Configuration.RootPath);
            }
            else
            {
                Console.WriteLine("Folder creation failed. Attempted root folder creation at: " + Configuration.RootPath);
            }
        }
        private void ConfigurationSetup()
        {
            string executionPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\","");
            //Console.WriteLine(executionPath);
            if (File.Exists(executionPath + "\\" + "portable.txt"))
            {
                Configuration.IsPortable = true;
                Configuration.ConfigurationFolderPath = executionPath;
                //Configuration.ConfigurationFilePath = Configuration.ConfigurationFolderPath + "\\" + "config.txt";
            }

            Console.WriteLine("Attempting to import configuration file at " + Configuration.ConfigurationFilePath);
            
            if (!Directory.Exists(Configuration.ConfigurationFolderPath)) //Config file is missing
            {
                Directory.CreateDirectory(Configuration.ConfigurationFolderPath);
            }
            if (!File.Exists(Configuration.ConfigurationFilePath))
            {
                Console.WriteLine("Configuration file does not exist. Recreating with default settings.");

                Configuration.setDefaultConfiguration(); //sets default values

                FileIO.ExportStringListToFile(Configuration.ConfigurationFilePath, Configuration.getCurrentConfiguration());
                if (!File.Exists(Configuration.ConfigurationFilePath))
                {
                    Console.WriteLine("Something went wrong. Configuration file not saved.");
                }
                else
                {
                    Console.WriteLine("Configuration file saved successfully.");
                }
            }
            else // Config File exists, import
            {
                Console.WriteLine("Configuration file detected. Importing...");
                List<string> configLines = FileIO.ImportFileToStringList(Configuration.ConfigurationFilePath);
                List<Object> configValues = Parser.parseConfigFile(configLines);
                Configuration.setNewConfiguration(configValues);
                Console.WriteLine("Configuration settings imported.");
            }
        }

        private List<string> ImportUpdateTitles()
        {
            string updatesFilePath = Configuration.ImportPath + "\\Updates.txt";
            List<string> updateTitles = new List<string>();
            Console.WriteLine("Importing update title list from file: " + updatesFilePath);

            if (File.Exists(updatesFilePath))
            {
                //Import Update List from File
                List<string> lines = FileIO.ImportFileToStringList(updatesFilePath);

                //Parse Update Titles from imported lines
                updateTitles = Parser.ParseLinesContaining(lines, "(KB");
                Console.WriteLine(updateTitles.Count + " update titles collected.");
            }
            else
            {
                Console.WriteLine("Update.txt file not found at " + updatesFilePath);
                File.Create(updatesFilePath).Dispose();
                if (File.Exists(updatesFilePath))
                {
                    Console.WriteLine("Updates.txt file created. Please populate it with update information and restart WUDownloader.");

                }
                else
                {
                    Console.WriteLine("Something went wrong. Updates.txt not created at " + updatesFilePath);
                }
                Console.WriteLine("Exiting...");
                System.Console.ReadKey();
                Environment.Exit(0);
            }
            
            return updateTitles;
        }
        private void SetupTable()
        {
            //Check if file exists
            if (File.Exists(Configuration.TablePath + "\\" + Configuration.TableName + ".csv")) //If exists
            {
                Console.WriteLine("CSV file exists. Importing...");
                //Import file
                List<string> csv = FileIO.ImportCsvToStringList(Configuration.TablePath + "\\" + Configuration.TableName);

                //Build table with schema
                //DataTable table = TableBuilder.BuildTableSchema(Configuration.TableName, );
                //Populate table from file
                //t.populateTableFromCsv(csv, true);
            }
            else //If not exists
            {
                Console.WriteLine("CSV file does not exists. Generating...");
                //Build table from scratch
                //t.buildTableSchema();
                //FileIO.ExportDataTableToCSV(TableBuilder.Table, Configuration.TablePath, Configuration.TableName);
                Console.WriteLine("CSV file saved.");
            }
        }
        private void CollectUpdateDataForTable(List<string> updateTitles)
        {
            Console.WriteLine("Attempting to collect data for " + updateTitles.Count + " updates...");
            int x = 0;
            foreach (string updateTitle in updateTitles) //For each update
            {
                Console.WriteLine("Collecting data for update " + (x + 1) + " of " + updateTitles.Count + ".");

                Console.WriteLine("Title is: " + updateTitle);

                //If data exists in CSV file
                //if (QueryController.doesUpdateTitleExistInTable(TableBuilder.Table, updateTitle) == true)
                //{
                //    Console.WriteLine("Update data already exists in table. Skipping...");
                //}
                //else //Data doesn't exist in CSV file, so collect it and populate the table
                //{
                //    string kb = updateTitle.Split('(', ')')[1];
                //    HtmlDocument siteAsHtml = WebController.getSiteAsHTML(Configuration.CATALOG_URL + kb);
                //    t.populateTableFromSite(siteAsHtml, Configuration.TablePath, Configuration.TableName);
                //}
                x++;
            }
            Console.WriteLine("Data collection complete.");
        }
        private void StartDownloadManager(List<string> updateTitles)
        {
            List<string> productList = getProductList();
            DownloadManager d = new DownloadManager(productList);
            
            Console.WriteLine("Populating download queue...");
            d.populateDownloadQueue(updateTitles);
            Console.WriteLine("Queue loading complete...");
            Console.WriteLine("Initializing download sequence...");
            d.downloadFilesFromQueue();
            Console.WriteLine("Downloads complete.");
        }
        public List<string> getProductList()
        {
            //string columnName = "product";
            //var productsFromTable = t.getAllDataFromColumn(columnName);
            List<string> productList = new List<string>();
            //for (int x = 0; x < productsFromTable.Count; x++)
            //{
            //    string productsAtCurrentRow = (string)productsFromTable[x];
            //    string[] splitProducts = productsAtCurrentRow.Split(',');
            //    foreach (string product in splitProducts)
            //    {
            //        string trimmedProduct = product.Trim();
            //        if (!productList.Contains(trimmedProduct))
            //        {
            //            productList.Add(trimmedProduct);
            //        }
            //    }
                
            //}
            
            return productList;
        }
    }
}
