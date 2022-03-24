using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace ISRPO_SREZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public ObservableCollection<Sale> sales { get; set; }
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://localhost:7100/api/Sale");

            var postData = "startDate=" + Uri.EscapeDataString(this.startDate.ToShortDateString());
            postData += "&endDate=" + Uri.EscapeDataString(this.endDate.ToShortDateString());
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var resp = new StreamReader(response.GetResponseStream()).ReadToEnd();

            this.sales = JsonSerializer.Deserialize<ObservableCollection<Sale>>(resp);
            this.dg.ItemsSource = this.sales;
        }

        private void cb_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (((ComboBoxItem)(((ComboBox)sender).SelectedItem)).Content.ToString() == "Фирмы")
                {
                    double[] data = this.sales.Select(s => (double)s.Telephones.Select(t => t.Manufacturer).Count()).ToArray();
                    var pie = this.chart.Plot.AddPie(data);
                    pie.ShowPercentages = true;
                    this.chart.Refresh();
                }
                else if (((ComboBoxItem)(((ComboBox)sender).SelectedItem)).Content.ToString() == "Продажи")
                {
                    DateTime[] X = this.sales.Select(s => s.DateSale).Distinct().ToArray();
                    List<double> Y = new List<double>();

                    foreach (var x in X)
                    {
                        Y.Add(this.sales.Where(s => s.DateSale == x).Sum(s => (double)s.Telephones.Sum(t => t.Cost)));
                    }

                    this.chart.Plot.XAxis.DateTimeFormat(true);
                    this.chart.Plot.AddScatter(X.Select(x => x.ToOADate()).ToArray(), Y.ToArray());
                    this.chart.Refresh();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
    public partial class Client
    {
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        [JsonPropertyName("patronymic")]
        public string Patronymic { get; set; }
    }
    partial class Client
    {
        public string FullName
        {
            get
            {
                return $"{this.LastName} {this.FirstName} {this.Patronymic}";
            }
        }
    }
    public class Telephone
    {
        [JsonPropertyName("articul")]
        public int Articul { get; set; }
        [JsonPropertyName("nameTelephone")]
        public string NameTelephone { get; set; }
        [JsonPropertyName("category")]
        public string Category { get; set; }
        [JsonPropertyName("cost")]
        public decimal Cost { get; set; }
        [JsonPropertyName("count")]
        public int Count { get; set; }
        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; }
    }
    public class Sale
    {
        [JsonPropertyName("dateSale")]
        public DateTime DateSale { get; set; }
        [JsonPropertyName("client")]
        public Client Client { get; set; }
        [JsonPropertyName("telephones")]
        public List<Telephone> Telephones { get; set; }

    }
}
