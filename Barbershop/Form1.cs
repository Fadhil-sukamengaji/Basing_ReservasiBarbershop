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
            comboBoxLayanan.DataBindings.Clear();
            comboBoxCapster.DataBindings.Clear();
            dtpTanggal.DataBindings.Clear();
            dtpJam.DataBindings.Clear();

            textBoxNama.DataBindings.Add("Text", bs, "nama_pelanggan", true, DataSourceUpdateMode.OnPropertyChanged);
            comboBoxLayanan.DataBindings.Add("SelectedValue", bs, "id_layanan", true, DataSourceUpdateMode.OnPropertyChanged);
            comboBoxCapster.DataBindings.Add("SelectedValue", bs, "id_capster", true, DataSourceUpdateMode.OnPropertyChanged);

            dtpTanggal.DataBindings.Add("Value", bs, "jam_booking", true, DataSourceUpdateMode.OnPropertyChanged);
            dtpJam.DataBindings.Add("Value", bs, "jam_booking", true, DataSourceUpdateMode.OnPropertyChanged);

            if (bindingNavigator1 != null)
            {
                bindingNavigator1.BindingSource = bs;
            }
        }

        private void RefreshTable()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT * FROM vw_Reservasi";
                    da = new SqlDataAdapter(query, conn);
                    cb = new SqlCommandBuilder(da);

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    bs.DataSource = dt;
                    dataGridView1.DataSource = bs;

                    if (dataGridView1.Columns["id_layanan"] != null)
                        dataGridView1.Columns["id_layanan"].Visible = false;

                    if (dataGridView1.Columns["id_capster"] != null)
                        dataGridView1.Columns["id_capster"].Visible = false;

                    if (bindingNavigator1 != null)
                    {
                        bindingNavigator1.BindingSource = bs;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat tabel data: " + ex.Message, "Error Tampilan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadComboBoxes()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
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
                    DataTable dtC = new DataTable();
                    daC.Fill(dtC);
                    comboBoxCapster.DataSource = dtC;
                    comboBoxCapster.DisplayMember = "nama";
                    comboBoxCapster.ValueMember = "id_capster";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data pilihan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    DateTime waktuBooking = dtpTanggal.Value.Date + dtpJam.Value.TimeOfDay;

                    string queryCek = "SELECT COUNT(*) FROM Reservasi WHERE id_capster = @id_cap AND jam_booking = @jam AND status_reservasi <> 'Selesai'";
                    using (SqlCommand cmdCek = new SqlCommand(queryCek, conn))
                    {
                        cmdCek.Parameters.AddWithValue("@id_cap", comboBoxCapster.SelectedValue ?? DBNull.Value);
                        cmdCek.Parameters.AddWithValue("@jam", waktuBooking);

                        int jumlahBentrok = (int)cmdCek.ExecuteScalar();

                        if (jumlahBentrok > 0)
                        {
                            MessageBox.Show("Capster sudah memiliki jadwal booking aktif di jam dan tanggal tersebut! Silakan pilih waktu lain atau capster lain.",
                                            "Jadwal Bentrok", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    SqlCommand cmd = new SqlCommand("sp_TambahReservasi", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@nama", textBoxNama.Text);
                    cmd.Parameters.AddWithValue("@id_lay", comboBoxLayanan.SelectedValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id_cap", comboBoxCapster.SelectedValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@jam", waktuBooking);

                    cmd.ExecuteNonQuery();

                    using (SqlCommand cmdBackup = new SqlCommand("sp_AutoBackupReservasi", conn))
                    {
                        cmdBackup.CommandType = CommandType.StoredProcedure;
                        cmdBackup.ExecuteNonQuery();
                    }

                    MessageBox.Show("Data Berhasil Disimpan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshTable();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menyimpan data: " + ex.Message, "Error SQL Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                string id = dataGridView1.CurrentRow.Cells["id_reservasi"].Value.ToString();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("sp_UpdateReservasi", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@id_reservasi", Convert.ToInt32(id));
                            cmd.Parameters.AddWithValue("@status_reservasi", "Selesai");
                            cmd.Parameters.AddWithValue("@status_pembayaran", "Lunas");

                            cmd.ExecuteNonQuery();

                            using (SqlCommand cmdBackup = new SqlCommand("sp_AutoBackupReservasi", conn))
                            {
                                cmdBackup.CommandType = CommandType.StoredProcedure;
                                cmdBackup.ExecuteNonQuery();
                            }

                            MessageBox.Show("Status Reservasi Diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshTable();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal memperbarui status: " + ex.Message, "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                string id = dataGridView1.CurrentRow.Cells["id_reservasi"].Value.ToString();

                if (MessageBox.Show("Hapus data ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        try
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand("sp_DeleteReservasi", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@id_reservasi", SqlDbType.Int).Value = Convert.ToInt32(id);

                                cmd.ExecuteNonQuery();

                                using (SqlCommand cmdBackup = new SqlCommand("sp_AutoBackupReservasi", conn))
                                {
                                    cmdBackup.CommandType = CommandType.StoredProcedure;
                                    cmdBackup.ExecuteNonQuery();
                                }

                                MessageBox.Show("Data Berhasil Dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                RefreshTable();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Gagal menghapus data: " + ex.Message, "Gagal Menghapus", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        }
                    }
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_SearchReservasi", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@nama_pelanggan", txtSearch.Text);

                        SqlDataAdapter daSearch = new SqlDataAdapter(cmd);
                        DataTable dtSearch = new DataTable();
                        daSearch.Fill(dtSearch);

                        dataGridView1.DataSource = dtSearch;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Pencarian gagal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                        DateTime waktuBooking = dtpTanggal.Value.Date + dtpJam.Value.TimeOfDay;

                        string queryCek = "SELECT COUNT(*) FROM Reservasi WHERE id_capster = @id_cap AND jam_booking = @jam AND id_reservasi <> @id_res AND status_reservasi <> 'Selesai'";
                        using (SqlCommand cmdCek = new SqlCommand(queryCek, conn))
                        {
                            cmdCek.Parameters.AddWithValue("@id_cap", comboBoxCapster.SelectedValue ?? DBNull.Value);
                            cmdCek.Parameters.AddWithValue("@jam", waktuBooking);
                            cmdCek.Parameters.AddWithValue("@id_res", Convert.ToInt32(idReservasi));

                            int jumlahBentrok = (int)cmdCek.ExecuteScalar();

                            if (jumlahBentrok > 0)
                            {
                                MessageBox.Show("Gagal mengubah! Capster sudah memiliki jadwal booking aktif lain di jam dan tanggal tersebut.",
                                                "Jadwal Bentrok", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return; 
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand("sp_GantiReservasi", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@id_reservasi", Convert.ToInt32(idReservasi));
                            cmd.Parameters.AddWithValue("@nama_pelanggan", textBoxNama.Text);
                            cmd.Parameters.AddWithValue("@id_layanan", comboBoxLayanan.SelectedValue ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@id_capster", comboBoxCapster.SelectedValue ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@jam", waktuBooking);

                            cmd.ExecuteNonQuery();

                            using (SqlCommand cmdBackup = new SqlCommand("sp_AutoBackupReservasi", conn))
                            {
                                cmdBackup.CommandType = CommandType.StoredProcedure;
                                cmdBackup.ExecuteNonQuery();
                            }

                            MessageBox.Show("Data reservasi berhasil diubah!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshTable();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal mengubah data: " + ex.Message, "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
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

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        int result = cmd.ExecuteNonQuery();
                        MessageBox.Show(result + " baris berhasil dilakukan SQL Injection!", "Simulasi Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                RefreshTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    IF EXISTS (SELECT 1 FROM dbo.Reservasi_Backup)
                    BEGIN
                        DELETE FROM dbo.Reservasi;
                        SET IDENTITY_INSERT dbo.Reservasi ON;
                        
                        INSERT INTO dbo.Reservasi (id_reservasi, id_layanan, id_capster, status_reservasi, status_pembayaran, jam_booking, nama_pelanggan)
                        SELECT id_reservasi, id_layanan, id_capster, status_reservasi, status_pembayaran, jam_booking, nama_pelanggan 
                        FROM dbo.Reservasi_Backup;

                        SET IDENTITY_INSERT dbo.Reservasi OFF;
                    END";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Sistem berhasil dipulihkan!", "Pemulihan Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reset gagal: " + ex.Message, "Error Pemulihan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void comboBoxJadwal_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}