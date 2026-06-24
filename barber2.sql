--- STORED PROSEDURE

DROP PROCEDURE sp_InsertReservasi

CREATE PROCEDURE sp_InsertReservasi
    @nama_pelanggan VARCHAR(100),
    @id_layanan INT,
    @id_capster INT,
    @id_jadwal INT,
    @status_reservasi VARCHAR(15),
    @status_pembayaran VARCHAR(15)
AS
BEGIN
    IF @nama_pelanggan = '' 
       OR @status_reservasi = '' 
       OR @status_pembayaran = ''
    BEGIN
        RAISERROR('Semua data harus diisi!', 16, 1)
        RETURN
    END

    IF @nama_pelanggan LIKE '%[0-9]%'
    BEGIN
        RAISERROR('Nama pelanggan tidak boleh mengandung angka!', 16, 1)
        RETURN
    END
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Reservasi WHERE nama_pelanggan = @nama_pelanggan AND id_capster = @id_capster AND id_jadwal = @id_jadwal)
    BEGIN
        RAISERROR('Gagal! Pelanggan sudah memesan Capster dan Jadwal ini sebelumnya.', 16, 1);
        RETURN;
    END

    INSERT INTO Reservasi (nama_pelanggan, id_layanan, id_capster, id_jadwal, status_reservasi, status_pembayaran)
    VALUES (@nama_pelanggan, @id_layanan, @id_capster, @id_jadwal, @status_reservasi, @status_pembayaran);
END
GO


CREATE PROCEDURE sp_UpdateReservasi
    @id_reservasi INT,
    @status_reservasi VARCHAR(15),
    @status_pembayaran VARCHAR(15)
AS
BEGIN
    UPDATE Reservasi
    SET 
        status_reservasi = @status_reservasi,
        status_pembayaran = @status_pembayaran
    WHERE id_reservasi = @id_reservasi
END
GO

CREATE PROCEDURE sp_DeleteReservasi
    @id_reservasi INT
AS
BEGIN
    DELETE FROM Reservasi
    WHERE id_reservasi = @id_reservasi
END
GO

CREATE PROCEDURE sp_SearchReservasi
    @nama_pelanggan VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM vw_Reservasi
    WHERE nama_pelanggan LIKE '%' + @nama_pelanggan + '%'
END
GO

CREATE PROCEDURE sp_GantiReservasi
    @id_reservasi INT,
    @nama_pelanggan VARCHAR(100),
    @id_layanan INT,
    @id_capster INT,
    @id_jadwal INT
AS
BEGIN
    UPDATE Reservasi
    SET
        nama_pelanggan = @nama_pelanggan,
        id_layanan = @id_layanan,
        id_capster = @id_capster,
        id_jadwal = @id_jadwal
    WHERE id_reservasi = @id_reservasi
END
GO


-- VIEWW

create view vw_data
as
SELECT id_layanan, 
nama_layanan + ' - Rp. ' + CAST(CAST(harga AS DECIMAL(10,0)) AS VARCHAR) AS LayananHarga 
FROM Layanan

select * from vw_data

CREATE VIEW vw_Reservasi
AS
SELECT 
    R.id_reservasi,
    R.nama_pelanggan,
    L.nama_layanan,
    C.nama AS nama_capster,
    J.hari,
    R.status_reservasi,
    R.status_pembayaran
FROM Reservasi R
JOIN Layanan L ON R.id_layanan = L.id_layanan
JOIN Capster C ON R.id_capster = C.id_capster
JOIN Jadwal J ON R.id_jadwal = J.id_jadwal
GO


-------
-- Membuat salinan data dari tabel Reservasi
SELECT * INTO Reservasi_Backup FROM Reservasi;