using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;

namespace Barbershop
{
    public partial class FormCetak : Form
    {
        private string connString = @"Data Source=DESKTOP-FHLKCTQ\SEHAB;Initial Catalog=DBBarbershop;Integrated Security=True";
        private string statusFilter;

        public FormCetak(string statusBayar)
        {
            InitializeComponent();
            this.statusFilter = statusBayar;
        }

        private void FormCetak_Load(object sender, EventArgs e)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connString);

                string query = "SELECT id_reservasi, nama_pelanggan, nama_layanan, nama_capster, harga FROM Reservasi";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                dsBarbershop ds = new dsBarbershop();

                da.Fill(ds, "dtReport");

                if (ds.Tables["dtReport"].Rows.Count == 0)
                {
                    MessageBox.Show("Peringatan: Tidak ada data yang ditarik dari database! Cek kembali nama tabel atau koneksi Anda.", "Data Kosong");
                }

                ReportDocument reportDoc = new ReportDocument();
                string reportPath = Application.StartupPath + @"\CrystalReportReservasi.rpt";
                reportDoc.Load(reportPath);

                reportDoc.Database.Tables["dtReport"].SetDataSource(ds.Tables["dtReport"]);

                crystalReportViewer1.ReportSource = reportDoc;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sistem mendeteksi error: \n\n" + ex.ToString(),
                                "Detail Error Koneksi/Query",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void crystalReportViewer1_Load(object sender, EventArgs e)
        {

        }
    }
}