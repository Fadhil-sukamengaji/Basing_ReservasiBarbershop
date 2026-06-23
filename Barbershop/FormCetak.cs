using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;

namespace Barbershop
{
    public partial class FormCetak : Form
    {
        // Pastikan variabel koneksi kamu sudah benar
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

                // 1. Pastikan nama tabel setelah kata FROM di bawah ini adalah nama tabel asli di database SQL Server-mu
                string query = "SELECT id_reservasi, nama_pelanggan, nama_layanan, nama_capster, harga FROM Reservasi";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                dsBarbershop ds = new dsBarbershop();

                // 2. Mengisi dtReport di dalam dataset
                da.Fill(ds, "dtReport");

                // 3. Tes apakah data dari SQL berhasil ditarik atau tidak
                if (ds.Tables["dtReport"].Rows.Count == 0)
                {
                    MessageBox.Show("Peringatan: Tidak ada data yang ditarik dari database! Cek kembali nama tabel atau koneksi Anda.", "Data Kosong");
                }

                ReportDocument reportDoc = new ReportDocument();
                string reportPath = Application.StartupPath + @"\CrystalReportReservasi.rpt";
                reportDoc.Load(reportPath);

                // 4. PASANG DATASET SECARA SPESIFIK KE TABEL REPORT
                // Cara ini memaksa Crystal Report membaca DataTable "dtReport" yang baru diisi dari SQL
                reportDoc.Database.Tables["dtReport"].SetDataSource(ds.Tables["dtReport"]);

                crystalReportViewer1.ReportSource = reportDoc;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                // Ini akan menampilkan pesan error asli dari SQL Server atau C# jika koneksi gagal
                MessageBox.Show("Sistem mendeteksi error: \n\n" + ex.ToString(),
                                "Detail Error Koneksi/Query",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }


    }
}