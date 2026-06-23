# SQL Injection Testing Scenario

## Deskripsi
---
Pengujian ini dilakukan untuk tujuan edukasi keamanan aplikasi pada project **Reservasi Barbershop**. Simulasi SQL Injection diterapkan pada fitur update data reservasi untuk menunjukkan bagaimana query SQL dapat dimanipulasi melalui input pengguna.
---

## Source Code yang Diuji
```
private void btnTestInjection_Click(object sender, EventArgs e)
{
    try
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = "UPDATE Reservasi SET nama_pelanggan = 'HACKED' WHERE nama_pelanggan = '" + textBoxNama.Text + "'";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();

            MessageBox.Show("Query berhasil dijalankan");
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.Message);
    }
}
```

## Penjelasan Kerentanan
Kode di atas rentan terhadap SQL Injection karena query dibuat menggunakan penggabungan string langsung (*string concatenation*).
Input dari `textBoxNama.Text` langsung dimasukkan ke query SQL tanpa validasi atau parameterized query.
---

## Payload SQL Injection
Input berikut dimasukkan ke dalam `textBoxNama`:
```sql
' OR '1'='1
```
---
## Query yang Terbentuk

Setelah payload dimasukkan, query berubah menjadi:

```sql
UPDATE Reservasi
SET nama_pelanggan = 'HACKED'
WHERE nama_pelanggan = '' OR '1'='1'
```
Karena kondisi `'1'='1'` selalu TRUE, maka seluruh data pada tabel `Reservasi` akan terkena update.
Akibatnya seluruh `nama_pelanggan` berubah menjadi:
```text
HACKED
```
## Langkah Pengujian

1. Jalankan aplikasi Reservasi Barbershop.
2. Masukkan payload berikut pada field nama pelanggan:
```sql
' OR '1'='1
```
3. Klik tombol `Test Injection`.
4. Sistem menjalankan query SQL yang telah dimanipulasi.
5. Seluruh data `nama_pelanggan` pada tabel `Reservasi` berubah menjadi:

```text
HACKED
```

## Dampak SQL Injection
SQL Injection dapat menyebabkan:
- Manipulasi seluruh data database
- Perubahan data tanpa izin
- Kehilangan integritas data
- Kerusakan sistem aplikasi
- Kebocoran informasi penting
---

# Pencegahan SQL Injection

Untuk mengatasi dampak SQL Injection, aplikasi menyediakan fitur reset data menggunakan backup tabel database.

Berikut implementasi fitur reset data:

```csharp
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

                INSERT INTO dbo.Reservasi 
                (id_reservasi, id_layanan, id_capster, id_jadwal, 
                 status_reservasi, status_pembayaran, nama_pelanggan)

                SELECT 
                id_reservasi, id_layanan, id_capster, id_jadwal,
                status_reservasi, status_pembayaran, nama_pelanggan

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
```
Metode tersebut mencegah manipulasi query oleh pengguna.
---

## Kesimpulan
Kerentanan SQL Injection terjadi karena query SQL dibuat menggunakan penggabungan string langsung tanpa validasi input.  
Penggunaan parameterized query sangat penting untuk menjaga keamanan aplikasi dan database.
Kerentanan SQL Injection terjadi karena query SQL dibuat menggunakan penggabungan string langsung tanpa validasi input.  
Penggunaan parameterized query sangat penting untuk menjaga keamanan aplikasi dan database.

Sebagai langkah penanganan, aplikasi menyediakan fitur reset data menggunakan tabel backup untuk mengembalikan data ke kondisi awal setelah terjadi serangan SQL Injection.
