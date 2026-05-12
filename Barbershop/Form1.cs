using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Barbershop
{
    public partial class Form1 : Form
    {
 
        string connectionString = @"Data Source=PADILSU\PADIL;Initial Catalog=DBBarbershop;Integrated Security=True";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshTable();
            LoadComboBoxes();
        }

        private void RefreshTable()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM v_DaftarReservasi", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

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
                    string sql = "UPDATE Reservasi SET status_reservasi = 'Selesai', status_pembayaran = 'Lunas' WHERE id_reservasi = @id";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", id);
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
                        string sql = "DELETE FROM Reservasi WHERE id_reservasi = @id";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@id", id);
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
                        string sql = @"UPDATE Reservasi 
                               SET nama_pelanggan = @nama, 
                                   id_layanan = @lay, 
                                   id_capster = @cap, 
                                   id_jadwal = @jad 
                               WHERE id_reservasi = @id";

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@nama", textBoxNama.Text);
                        cmd.Parameters.AddWithValue("@lay", comboBoxLayanan.SelectedValue);
                        cmd.Parameters.AddWithValue("@cap", comboBoxCapster.SelectedValue);
                        cmd.Parameters.AddWithValue("@jad", comboBoxJadwal.SelectedValue);
                        cmd.Parameters.AddWithValue("@id", idReservasi);

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

        }

    }
}