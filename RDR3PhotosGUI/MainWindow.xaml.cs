using Ookii.Dialogs.Wpf;
using RDR3PhotosGUI.RDR3Photos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace RDR3PhotosGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<PhotoList> PhotosList = new List<PhotoList>();

        private readonly BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {

            InitializeComponent();

            Settings.LoadSettings();

            if (!Directory.Exists(Settings.SettingsDic["profile_folder"]))
            {
                var dialog = new VistaFolderBrowserDialog();

                dialog.ShowDialog();

                Settings.SettingsDic.Remove("profile_folder");
                Settings.SettingsDic.Add("profile_folder", dialog.SelectedPath);
                Settings.SaveSettings();
            }

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                this.GetPhotosFiles();
            }));

        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                PhotosList = PhotosList.OrderByDescending(_p => _p.Data.Info.GetDate().Ticks).ToList();
                PhotoListViewer.ItemsSource = PhotosList;
                Loading_Label.Visibility = Visibility.Hidden;
            }));
        }

        public void GetPhotosFiles()
        {
            try
            {
                foreach (string path in Directory.GetFiles(Settings.SettingsDic["profile_folder"], "PRDR*"))
                {
                    if (path.EndsWith(".jpg"))
                        continue;

                    Photo photo = new Photo(File.ReadAllBytes(path));

                    Image bitmap = Bitmap.FromStream(new MemoryStream(photo.PhotoData.Image));

                    this.PhotosList.Add(new PhotoList() { Name = photo.Info.GetDate().ToString(), Image = bitmap.ToImageSource(ImageFormat.Jpeg), Data = photo, Path = path });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ExportImages_Click(object sender, RoutedEventArgs e)
        {
            foreach (PhotoList photo in this.PhotoListViewer.SelectedItems)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;

                if (!Directory.Exists(Path.Combine(path, "exported")))
                    Directory.CreateDirectory(Path.Combine(path, "exported"));

                photo.Data.JsonData.SaveToDisk(Path.Combine(path, "exported", $"JSON_{photo.Path.Split('\\').Last()}.json"));
                photo.Data.PhotoData.SaveToDisk(Path.Combine(path, "exported", $"IMG_{photo.Path.Split('\\').Last()}.jpg"));
            }
        }

        private void DeleteImages_Click(object sender, RoutedEventArgs e)
        {
            foreach (PhotoList photo in this.PhotoListViewer.SelectedItems)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;

                if(!Directory.Exists(Path.Combine(path, "deleted")))
                    Directory.CreateDirectory(Path.Combine(path, "deleted"));

                File.Move(photo.Path, Path.Combine(path, "deleted", photo.Path.Split('\\').Last()));

                PhotosList.Remove(photo);
            }
            PhotoListViewer.ClearValue(System.Windows.Controls.ItemsControl.ItemsSourceProperty);
            PhotoListViewer.ItemsSource = PhotosList;
        }
    }

    public class PhotoList
    {
        public string Name { get; set; }
        public ImageSource Image { get; set; }
        public Photo Data { get; set; }
        public string Path { get; set; }
    }
}
