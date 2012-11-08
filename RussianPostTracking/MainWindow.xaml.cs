using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using mshtml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

namespace RussianPostTracking
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LBIdentifier.ItemsSource = Model.Current.Listidentifier;
            webBrowser1.Navigated += (a, b) => { HideScriptErrors(webBrowser1, true); };
            webBrowser1.Navigate(new Uri("http://www.russianpost.ru/rp/servise/ru/home/postuslug/trackingpo"));
        }
        public void HideScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            object objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null) return;
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { Hide });
        }
        private void webBrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            var doc = (HTMLDocument)webBrowser1.Document;
            if (webBrowser1.Source.AbsoluteUri == "http://www.russianpost.ru/resp_engine.aspx?Path=rp/servise/ru/home/postuslug/trackingpo")
            {
                var el = doc.getElementById("BarCode");
                var ind = el.getAttribute("value");
                var html = doc.body.outerHTML;
                html = html.Replace("\r\n", "");
                var inds = html.IndexOf("<TABLE class=pagetext>");
                if (inds == -1)
                    return;
                var inde = html.IndexOf("</TABLE>", inds);
                var htmlTable = html.Substring(inds, inde - inds + 8);

                var ms = Regex.Replace(htmlTable, @"=(?<attr>\w*)", "=\"${attr}\"");
                var xml = XElement.Parse(ms);
                var tbody = xml.Element("TBODY");
                var trs = tbody.Elements("TR");

                var tab = tabControl1.Items.OfType<TabItem>().Single(a => a.Name == "tb_" + ind);
                var dg = tab.Content as DataGrid;

                foreach (var tr in trs)
                {
                    var tds = tr.Elements("TD").ToArray();
                    dg.Items.Add(new
                    {
                        Operation = tds[0].Value,
                        Date = tds[1].Value,
                        Index = tds[2].Value,
                        NameOPS = tds[3].Value,
                        AttributeOparetion = tds[4].Value,
                        Weight = tds[5].Value,
                        Worth = tds[6].Value,
                        Payment = tds[7].Value,
                        IndexAddress = tds[8].Value,
                        Address = tds[9].Value,
                    });
                }
                tab.Name = "Loaded_" + tab.Name;
                var nextTab = tabControl1.Items.OfType<TabItem>().FirstOrDefault(a => a.Name.StartsWith("tb_"));
                if (nextTab != null)
                {
                    var indTab = nextTab.Name.Replace("tb_", "");
                    el.setAttribute("value", indTab);
                    webBrowser1.InvokeScript("CheckInputCode");
                }
                else
                    Status.Content = "Готово";

            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Status.Content = "Загрузка...";
            tabControl1.Items.Clear();
            foreach (string ind in Model.Current.Listidentifier)
            {
                var tab = new TabItem() { Header = ind, Name = "tb_" + ind };
                tabControl1.Items.Add(tab);
                var dataGrid = new DataGrid() { AutoGenerateColumns = false, FontSize = 12 };
                dataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Операция",
                    Binding = new Binding("Date") { Mode = BindingMode.OneWay }
                });
                dataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Дата",
                    Binding = new Binding("Date") { Mode = BindingMode.OneWay }
                });
                dataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Индекс",
                    Binding = new Binding("Index") { Mode = BindingMode.OneWay }
                });

                dataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Название ОПС",
                    Binding = new Binding("NameOPS") { Mode = BindingMode.OneWay }
                });
                dataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Атрибут операции",
                    Binding = new Binding("AttributeOparetion") { Mode = BindingMode.OneWay }
                });
                dataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Вес(кг.)",
                    Binding = new Binding("Weight") { Mode = BindingMode.OneWay }
                });
                dataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Ценность(руб.)",
                    Binding = new Binding("Worth") { Mode = BindingMode.OneWay }
                });
                dataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Платёж(руб.)",
                    Binding = new Binding("Payment") { Mode = BindingMode.OneWay }
                });
                dataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Индекс",
                    Binding = new Binding("IndexAddress") { Mode = BindingMode.OneWay }
                });
                dataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Адрес",
                    Binding = new Binding("Address") { Mode = BindingMode.OneWay }
                });
                tab.Content = dataGrid;
            }
            var doc = (HTMLDocument)webBrowser1.Document;
            if (doc != null)
            {
                var el = doc.getElementById("BarCode");
                if (el != null)
                {
                    el.setAttribute("value", Model.Current.Listidentifier.First());
                    webBrowser1.InvokeScript("CheckInputCode");
                }
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            var newInd = TB_NewInd.Text;
            if (Model.Current.Listidentifier.Contains(newInd))
            {
                MessageBox.Show("Указанный идентификатор уже существует!");
                return;
            }
            Model.Current.Listidentifier.Add(newInd);
            LBIdentifier.ItemsSource = null;
            LBIdentifier.ItemsSource = Model.Current.Listidentifier;
            LBIdentifier.UpdateLayout();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (LBIdentifier.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите идентификатор из списка!");
                return;
            }
            Model.Current.Listidentifier.Remove(LBIdentifier.SelectedValue as string);
            LBIdentifier.ItemsSource = null;
            LBIdentifier.ItemsSource = Model.Current.Listidentifier;
            LBIdentifier.UpdateLayout();
        }
    }
}
