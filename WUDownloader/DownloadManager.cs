﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace WUDownloader
{

    class DownloadManager
    {
        public DownloadManager(List<string> productFilter, List<string> languageFilter)
        {
            this.ProductFilter = productFilter;
            this.LanguageFilter = languageFilter;
            this.DownloadQueue = new List<DownloadItem>();
        }

        public List<DownloadItem> DownloadQueue { get; private set; }
        public List<string> ProductFilter { get; set; }

        public List<string> LanguageFilter { get; set; }

        public void AddDownloadItemToQueue(DownloadItem downloadItem)
        {
            int queueSize = this.DownloadQueue.Count;
            this.DownloadQueue.Add(downloadItem);
            if (queueSize + 1 == this.DownloadQueue.Count) //Checks if download queue size increased
            {
                Console.WriteLine("Item added to download queue. Queue Size: " + this.DownloadQueue.Count);
            }
            else //Queue size did not increase, therefore item was not added successfully
            {
                Console.WriteLine("Item not successfully added to download queue.");
            }
        }
        public void DownloadFilesFromQueue()
        {
            Console.WriteLine("Initializing downloads...");
            Console.WriteLine("Download Queue Size: " + this.DownloadQueue.Count);

            List<DownloadItem> SortedDownloadQueue = this.DownloadQueue.OrderBy(o => o.Kb).ToList();

            for (int x = 0; x < SortedDownloadQueue.Count; x++) //Runs through sorted download queue
            {
                string downloadUrl = SortedDownloadQueue[x].DownloadUrl;
                Uri uri = new Uri(downloadUrl);
                string fileName = System.IO.Path.GetFileName(uri.LocalPath);
                Console.WriteLine("\nDownloading file for update: " + SortedDownloadQueue[x].Title);
                Console.WriteLine("File #{0} - {1}", (x + 1), fileName); //{0} is current index + 1, {1} is download's file name
                string[] products = SortedDownloadQueue[x].Product.Split(',');

                //for (int y = 0; y < products.Length; y++)
                foreach (string product in products)
                {
                    string downloadFolderPath = Configuration.DownloadFolderPath + "\\" + product.Trim() + "\\" + SortedDownloadQueue[x].Title;
                    string fullFilePath = downloadFolderPath + "\\" + System.IO.Path.GetFileName(uri.LocalPath);
                    if (fullFilePath.Length > 260)
                    {
                        int overflow = fullFilePath.Length - 260;
                        string downloadFolderPathWithoutKB = downloadFolderPath.Split('(').First();
                        downloadFolderPath = downloadFolderPathWithoutKB.Substring(0, downloadFolderPathWithoutKB.Length - overflow - 10) + " (" + SortedDownloadQueue[x].Kb + ")";
                        fullFilePath = downloadFolderPath + "\\" + System.IO.Path.GetFileName(uri.LocalPath);
                        int test = fullFilePath.Length;
                    }
                    if (File.Exists(fullFilePath))//Checks if file is already downloaded
                    {
                        Console.WriteLine("File already exists. Skipping...");
                    }
                    else //File is not already downloaded. Download.
                    {
                        Console.WriteLine("Download Path: " + downloadFolderPath);
                        Console.WriteLine("Downloading for " + product);
                        System.IO.Directory.CreateDirectory(downloadFolderPath);
                        DownloadWorker d = new DownloadWorker();
                        d.StartDownload(SortedDownloadQueue[x], downloadFolderPath);
                    }
                }
            }
        }
        public void PopulateDownloadQueue(List<UpdateInfo> updates) //, DataTable table)
        {
            foreach (UpdateInfo update in updates) //For each update
            {
                string kb = Parser.GetKbFromTitle(update.Title);

                if (kb.Length > 0)
                {
                    for (int x = 0; x < update.DownloadUrls.Count; x++)
                    {
                        string title = update.Title;
                        string product = update.Product.Trim();
                        string language = update.DownloadUrls.Cast<DictionaryEntry>().ElementAt(x).Value.ToString();
                        string downloadUrl = update.DownloadUrls.Cast<DictionaryEntry>().ElementAt(x).Key.ToString();
                        if ( this.LanguageFilter.Contains(language) && this.ProductFilter.Contains(product))
                        {
                            DownloadItem downloadItem = new DownloadItem(
                                title,
                                kb,
                                product,
                                downloadUrl,
                                language
                            );

                            AddDownloadItemToQueue(downloadItem);
                        }
                    }
                }
            }
        }
    }
}