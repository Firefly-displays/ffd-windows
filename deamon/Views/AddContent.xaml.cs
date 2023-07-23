using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using deamon.Models;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace deamon.Views;

public partial class AddContent : Window
{
    public AddContent()
    {
        InitializeComponent();
    }

    private void SelectFiles_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Media (*.mp4;*.png;*.jpg)|*.mp4;*.png;*.jpg"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            List<string> selectedFiles = new List<string>(openFileDialog.FileNames);

            foreach (string filePath in selectedFiles)
            {
                FileInfo fileInfo = new FileInfo(filePath);

                string fileName = fileInfo.Name;
                string fileExtension = fileInfo.Extension;
                
                bool isVideo = fileExtension == ".mp4";

                string? thumpPath = !isVideo ? filePath : Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                    "VideoQueue", "Media", Guid.NewGuid() + fileName + ".png");

                Content newContent = new Content(
                    fileName,
                    isVideo
                        ? Models.Content.ContentType.Video 
                        : Models.Content.ContentType.Image, 
                    filePath,
                    !isVideo ? thumpPath : null
                    );

                Logger.Log("Content added");
                Logger.Log(JsonConvert.SerializeObject(newContent));

                var deamonApi = DeamonAPI.GetInstance();
                deamonApi.POST(newContent);

                if (isVideo)
                {
                    try
                    {
                        var thumbFileStream = new FileStream(thumpPath, FileMode.Create, FileAccess.Write);
                        var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                        ffMpeg.GetVideoThumbnail(filePath, thumbFileStream, 5);
                        thumbFileStream.Close();
                        Logger.Log("Preview saved");
                            
                        newContent.ThumbPath = thumpPath;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error while creating thumb.");
                        Logger.Log(ex.ToString());
                    }
                                    
                    deamonApi.UPDATE(newContent);
                }
            }

            Close();
        }
    }
}