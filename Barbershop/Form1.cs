using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Barbershop
{
    public partial class Form1 : Form
    {

        DataSet ds = new DataSet();
        SqlDataAdapter da;
        BindingSource bs = new BindingSource();
        SqlCommandBuilder cb;

        string connectionString = @"Data Source=PADILSU\PADIL;Initial Catalog=DBBarbershop;Integrated Security=True";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshTable();
            LoadComboBoxes();

            textBoxNama.DataBindings.Clear();
            textBoxNama.DataBindings.Add("Text", bs, "nama_pelanggan", true, DataSourceUpdateMode.OnPropertyChanged);

            if (bindingNavigator1 != null)
            {
                bindingNavigator1.BindingSource = bs;
            }
        }

        private void RefreshTable()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM vw_Reservasi";
                da = new SqlDataAdapter(query, conn);
                cb = new SqlCommandBuilder(da);
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM v_DaftarReservasi", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                bs.DataSource = dt;
                dataGridView1.DataSource = bs;

                BindingSource bs = new BindingSource();
                bs.DataSource = dt;
                dataGridView1.DataSource = bs;

                bindingNavigator1.BindingSource = bs;
            }
        }

        private void LoadComboBoxes()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string queryLayanan = "SELECT id_layanan, " +
                                      "nama_layanan + ' - Rp. ' + CAST(CAST(harga AS DECIMAL(10,0)) AS VARCHAR) AS LayananHarga " +
                                      "FROM Layanan";

                SqlDataAdapter daL = new SqlDataAdapter(queryLayanan, conn);
                DataTable dtL = new DataTable();
                daL.Fill(dtL);

                comboBoxLayanan.DataSource = dtL;
                comboBoxLayanan.DisplayMember = "LayananHarga";
                comboBoxLayanan.ValueMember = "id_layanan";

                SqlDataAdapter daC = new SqlDataAdapter("SELECT id_capster, nama FROM Capster", conn);
                DataTable dtC = new DataTable(); daC.Fill(dtC);
                comboBoxCapster.DataSource = dtC;
                comboBoxCapster.DisplayMember = "nama";
                comboBoxCapster.ValueMember = "id_capster";

                SqlDataAdapter daJ = new SqlDataAdapter("SELECT id_jadwal, hari FROM Jadwal WHERE status_jadwal = 'Tersedia'", conn);
                DataTable dtJ = new DataTable(); daJ.Fill(dtJ);
                comboBoxJadwal.DataSource = dtJ;
                comboBoxJadwal.DisplayMember = "hari";
                comboBoxJadwal.ValueMember = "id_jadwal";
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("sp_InsertReservasi", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@nama_pelanggan", textBoxNama.Text);
                    cmd.Parameters.AddWithValue("@id_layanan", comboBoxLayanan.SelectedValue);
                    cmd.Parameters.AddWithValue("@id_capster", comboBoxCapster.SelectedValue);
                    cmd.Parameters.AddWithValue("@id_jadwal", comboBoxJadwal.SelectedValue);
                    cmd.Parameters.AddWithValue("@status_reservasi", "Pending");
                    cmd.Parameters.AddWithValue("@status_pembayaran", "Belum Bayar");

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Data Berhasil Disimpan!");

                    RefreshTable();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
                SqlCommand cmd = new SqlCommand("sp_TambahReservasi", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@nama", textBoxNama.Text);
                cmd.Parameters.AddWithValue("@id_lay", comboBoxLayanan.SelectedValue);
                cmd.Parameters.AddWithValue("@id_cap", comboBoxCapster.SelectedValue);
                cmd.Parameters.AddWithValue("@id_jad", comboBoxJadwal.SelectedValue);

                conn.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Berhasil!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message); // Akan memunculkan error "Jadwal penuh" dari SQL
                }
                RefreshTable();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                string id = dataGridView1.CurrentRow.Cells["id_reservasi"].Value.ToString();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("sp_UpdateReservasi", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_reservasi", id);
                    cmd.Parameters.AddWithValue("@status_reservasi", "Selesai");
                    cmd.Parameters.AddWithValue("@status_pembayaran", "Lunas");

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Status Reservasi Diperbarui!");
                    RefreshTable();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                string id = dataGridView1.CurrentRow.Cells["id_reservasi"].Value.ToString();

                if (MessageBox.Show("Hapus data ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        SqlCommand cmd = new SqlCommand("sp_DeleteReservasi", conn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@id_reservasi", id);

                        cmd.ExecuteNonQuery();

                        RefreshTable();
                    }
                }
            }
        }

        private void btnGanti_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                string idReservasi = dataGridView1.CurrentRow.Cells["id_reservasi"].Value.ToString();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();

                        SqlCommand cmd = new SqlCommand("sp_GantiReservasi", conn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@nama_pelanggan", textBoxNama.Text);
                        cmd.Parameters.AddWithValue("@id_layanan", comboBoxLayanan.SelectedValue);
                        cmd.Parameters.AddWithValue("@id_capster", comboBoxCapster.SelectedValue);
                        cmd.Parameters.AddWithValue("@id_jadwal", comboBoxJadwal.SelectedValue);
                        cmd.Parameters.AddWithValue("@id_reservasi", idReservasi);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Data reservasi berhasil diubah!");
                        RefreshTable();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal mengubah data: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Pilih baris di tabel yang ingin diganti!");
            }
        }

        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Reservasi SET nama_pelanggan = 'HACKED' WHERE nama_pelanggan = '" + textBoxNama.Text + "'";
        private void btnSearch_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM Reservasi WHERE nama_pelanggan = '" + txtSearch.Text + "'";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        int result = cmd.ExecuteNonQuery();
                        MessageBox.Show(result + " baris berhasil dilakukan SQL Injection!");
                    }
                }
                RefreshTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnResetData_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                IF OBJECT_ID('dbo.Reservasi_Backup') IS NOT NULL
                BEGIN
                    DELETE FROM dbo.Reservasi;
                    SET IDENTITY_INSERT dbo.Reservasi ON;
                    INSERT INTO dbo.Reservasi (id_reservasi, id_layanan, id_capster, id_jadwal, status_reservasi, status_pembayaran, nama_pelanggan)
                    SELECT id_reservasi, id_layanan, id_capster, id_jadwal, status_reservasi, status_pembayaran, nama_pelanggan 
                    FROM dbo.Reservasi_Backup;

                    SET IDENTITY_INSERT dbo.Reservasi OFF;
                END";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Data berhasil direset!");
                RefreshTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reset gagal: " + ex.Message);
            }
        }
    }
}