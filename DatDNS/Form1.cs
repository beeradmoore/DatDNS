using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatDNS
{
    public partial class Form1 : Form
    {
        private NetworkConfigurator _networkConfig = null;
        private List<DNSItem> _dnsItems = new List<DNSItem>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists("dns.json"))
                {
                    // Load DNS list.
                    string json = File.ReadAllText("dns.json");
                    List<DNSItem> tempList = JsonConvert.DeserializeObject<List<DNSItem>>(json);
                    if (tempList != null)
                    {
                        _dnsItems.AddRange(tempList);
                    }

                    // No DNS items found, insert defaults.
                    if (tempList.Count == 0)
                    {
                        _dnsItems.Add(new DNSItem() { Name = "Google DNS", PrimaryIP = "8.8.8.8", SecondaryIP = "8.8.4.4" });
                        _dnsItems.Add(new DNSItem() { Name = "OpenDNS", PrimaryIP = "208.67.222.222", SecondaryIP = "208.67.220.220" });

                        json = JsonConvert.SerializeObject(_dnsItems);
                        File.WriteAllText("dns.json", json);
                    }
                }
                else
                {
                    _dnsItems.Add(new DNSItem() { Name = "Google DNS", PrimaryIP = "8.8.8.8", SecondaryIP = "8.8.4.4" });
                    _dnsItems.Add(new DNSItem() { Name = "OpenDNS", PrimaryIP = "208.67.222.222", SecondaryIP = "208.67.220.220" });

                    string json = JsonConvert.SerializeObject(_dnsItems);
                    File.WriteAllText("dns.json", json);
                }
            }
            catch (Exception err)
            {
                // TODO: Handle loading dns.json config file.
            }

            // Not a fan of this manual DNS list updating. Would make the list bound to the dataGridView
            // Future work is to pull the DNS editor out to its own form and prevent this from being editable.
            foreach (DNSItem dnsItem in _dnsItems)
            {
                DataGridViewRow row = dataGridView1.Rows[dataGridView1.Rows.Add()];
                row.Cells["colName"].Value = dnsItem.Name;
                row.Cells["colPrimaryDNS"].Value = dnsItem.PrimaryIP;
                row.Cells["colSecondaryDNS"].Value = dnsItem.SecondaryIP;
                row.Cells["colButton"].Value = "Set as current DNS";
            }

            _networkConfig= new NetworkConfigurator();
            string [] networkCards = _networkConfig.GetNICs();
            foreach (string networkCard in networkCards)
            {
                deviceCB.Items.Add(networkCard);
            }

            if (networkCards.Length >= 1)
            {
                deviceCB.SelectedIndex = 0;

                UpdateCurrentNameservers();
            }
            else
            {
                MessageBox.Show("Error", "Could not detect any installed network cards.");
            }

          
            if (!IsAdministrator())
            {
                if (MessageBox.Show("DatDNS requires to be ran as an administrator in order to be awesome. Relaunch?", "Error", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    RelaunchAsAdmin();
                }
                else
                {
                    Close();
                }
            }
        }

        public bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void RelaunchAsAdmin()
        {
            if (!IsAdministrator())
            {
                // relaunch the application with admin rights
                string fileName = Assembly.GetExecutingAssembly().Location;
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.Verb = "runas";
                processInfo.FileName = fileName;
 
                try
                {
                    Process.Start(processInfo);
                    Close();
                }
                catch (Win32Exception)
                {
                    // This will be thrown if the user cancels the prompt
                    MessageBox.Show("DatDNS must be run as administrator.", "Error");
                }
 
                return;
            }
        }

        private void UpdateCurrentNameservers()
        {
            string[] nameServers = _networkConfig.GetNameservers(deviceCB.SelectedItem.ToString());
            if (nameServers == null)
            {
                primaryDNSTxt.Text = "error";
                secondaryDNSTxt.Text = "error";
            }
            else
            {
                primaryDNSTxt.Text = "error";
                secondaryDNSTxt.Text = "error";

                if (nameServers.Length == 1)
                {
                    primaryDNSTxt.Text = nameServers[0];
                    secondaryDNSTxt.Text = String.Empty;
                }
                else if (nameServers.Length == 2)
                {
                    primaryDNSTxt.Text = nameServers[0];
                    secondaryDNSTxt.Text = nameServers[1];
                }
            }
        }

        private void deviceCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCurrentNameservers();
        }

        private void SaveDNS()
        {
            string json = JsonConvert.SerializeObject(_dnsItems);
            File.WriteAllText("dns.json", json);
        }

        // all the following functions could be done A LOT better
        private void UpdateListFromGridView()
        {
            // Should think about changing save system from JSON to SQLite
            _dnsItems.Clear();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DNSItem tempItem = new DNSItem();
                // Appanretly cant get columns data to save...
                tempItem.Name = row.Cells[0].Value.ToString();
                tempItem.PrimaryIP = row.Cells["colPrimaryDNS"].Value.ToString();
                tempItem.SecondaryIP = row.Cells["colSecondaryDNS"].Value.ToString();

                _dnsItems.Add(tempItem);
            }

            SaveDNS();
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //UpdateListFromGridView();
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            //UpdateListFromGridView();
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //UpdateListFromGridView();
        }

        private void dataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            UpdateListFromGridView();
        }

        private void dataGridView1_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            UpdateListFromGridView();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3) // Our set DNS button.
            {
                // This needs some serious validation
                string primaryDNS = dataGridView1.Rows[e.RowIndex].Cells["colPrimaryDNS"].Value.ToString();
                string secondaryDNS = dataGridView1.Rows[e.RowIndex].Cells["colSecondaryDNS"].Value.ToString();
                // Like seriously... this is reallly bad...

                try
                {
                    _networkConfig.SetNameservers(deviceCB.SelectedItem.ToString(), primaryDNS + "," + secondaryDNS);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error");
                    return;
                }

                // Cant find documentation of what DnsFlushResolverCache return values mean...
                UInt32 result = DNSUtils.FlushDNSCache();
            }
        }
    }
}
