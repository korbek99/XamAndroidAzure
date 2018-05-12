using Android.App;
using Android.Widget;
using Android.OS;
using Uri = Android.Net.Uri;
using System.IO;
using Path = System.IO.Path;
using Plugin.Geolocator;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Android.Graphics;
using Android.Content;
using Android.Provider;
using Android.Runtime;
using System;

namespace XamAndroid_Azure
{
    [Activity(Label = "Storing with Azure", MainLauncher = true)]
    public class MainActivity : Activity
    {
        int camrequestcode = 100;
        Uri FileUri;
        ImageView Image;
        EditText txtName, txtAddress, txtAge, txtMail, txtRevenue;
        FileStream streamFile;
        double lat, lon;
        Intent intent;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            var btnStorage = FindViewById<Button>(Resource.Id.btnstorage);
            txtName = FindViewById<EditText>(Resource.Id.textname);
            txtAddress = FindViewById<EditText>(Resource.Id.textaddress);
            txtAge = FindViewById<EditText>(Resource.Id.textage);
            txtMail = FindViewById<EditText>(Resource.Id.textmail);
            txtRevenue = FindViewById<EditText>(Resource.Id.textrevenue);
            Image = FindViewById<ImageView>(Resource.Id.image);

            btnStorage.Click += async delegate
            {
                try
                {
                    var locator = CrossGeolocator.Current;
                    locator.DesiredAccuracy = 50;
                    var position = await locator.
                    GetPositionAsync(TimeSpan.FromSeconds(10),null,true);
                    lat = position.Latitude;
                    lon = position.Longitude;

                    var StorageAccount = CloudStorageAccount.Parse("https : // ruta de azure store");
                    var TableNoQL = StorageAccount.CreateCloudTableClient();
                    var Table = TableNoQL.GetTableReference("Administrative");
                    await Table.CreateIfNotExistsAsync();
                    var employee = new Employees("Administrative Staff", txtName.Text);
                    employee.Mail = txtMail.Text;
                    employee.Revenue =double.Parse(txtRevenue.Text);
                    employee.Age = int.Parse( txtAge.Text);
                    employee.Address = txtAddress.Text;
                    employee.Latitude = lat;
                    employee.Longitude = lon;
                    employee.Image = txtName.Text + ".jpg";
                    var Store = TableOperation.Insert(employee);
                    await Table.ExecuteAsync(Store);

                    Toast.MakeText(this.ApplicationContext,
                    "Data has been Stored",
                    ToastLength.Long).Show();

                    var BlobClient = StorageAccount.CreateCloudBlobClient();
                    var Content = BlobClient.GetContainerReference("images");
                    var resourceBlob = Content.GetBlockBlobReference(txtName.Text + ".jpg");
                    await resourceBlob.UploadFromFileAsync(FileUri.ToString());

                    Toast.MakeText(this.ApplicationContext,
                   "Image stored in azure",
                   ToastLength.Long).Show();

                }
                catch (System.Exception ex)
                {
                    Toast.MakeText(this.ApplicationContext,
                     ex.Message,
                     ToastLength.Long).Show();
                }
            };

            Image.Click += delegate
            {
                try
                {
                    intent = new Intent(MediaStore.ActionImageCapture);
                    intent.PutExtra(MediaStore.ExtraOutput, FileUri);
                    StartActivityForResult(intent, camrequestcode, bundle);
                }
                catch (System.Exception ex)
                {
                    Toast.MakeText(this.ApplicationContext,
                        ex.Message,
                        ToastLength.Long).Show();
                }
            };

        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == camrequestcode)
            {
                try {

                    if (txtName.Text != null)
                    {
                        FileUri = Android.Net.Uri.Parse(Path.Combine(
                            System.Environment.GetFolderPath(
                            System.Environment.SpecialFolder.Personal),
                            txtName.Text + ".jpg"));
                        if (File.Exists(FileUri.ToString()))
                        {
                            Toast.MakeText(this.ApplicationContext,
                                            "Image name already exists",
                                            ToastLength.Long).Show();
                        }else {
                            streamFile = new FileStream(Path.Combine(
                            System.Environment.GetFolderPath(
                            System.Environment.SpecialFolder.Personal),
                            txtName.Text + ".jpg"),FileMode.Create);
                            var bitmapImage = (Android.Graphics.Bitmap)data.Extras.Get("data");
                            bitmapImage.Compress(Bitmap.CompressFormat.Jpeg,100,streamFile);
                            streamFile.Close();
                            Image.SetImageBitmap(bitmapImage);

                        }
                     }

                
                    }
                    catch (System.Exception ex)
                    {
                         Toast.MakeText(this.ApplicationContext,
                             ex.Message,
                              ToastLength.Long).Show();
                    }
        }

        }
    }

    public class Employees : TableEntity
    {
        public Employees(string Category, string Name)
        {
            PartitionKey = Category;
            RowKey = Name;
        }

        public string Mail { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public double Revenue { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Image { get; set; }
    }
}

