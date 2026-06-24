CREATE DATABASE DBBarbershop;
GO

USE DBBarbershop;
GO

CREATE TABLE Capster (
    id_capster INT PRIMARY KEY IDENTITY(1,1),
    nama VARCHAR(100) NOT NULL
);

CREATE TABLE Layanan (
    id_layanan INT PRIMARY KEY IDENTITY(1,1),
    nama_layanan VARCHAR(50) NOT NULL,
    harga DECIMAL(10, 2) NOT NULL
);

CREATE TABLE Jadwal (
    id_jadwal INT PRIMARY KEY IDENTITY(1,1),
    id_capster INT,
    hari VARCHAR(20) NOT NULL,
    status_jadwal VARCHAR(20) DEFAULT 'Tersedia',
    FOREIGN KEY (id_capster) REFERENCES Capster(id_capster)
);

DROP TABLE Reservasi;

CREATE TABLE Reservasi (
    id_reservasi INT PRIMARY KEY IDENTITY(1,1),
    nama_pelanggan varchar(100),
    id_layanan INT,
    id_capster INT,
    id_jadwal INT,
    jam_booking TIME NOT NULL DEFAULT '00:00:00',
    status_reservasi VARCHAR(30) DEFAULT 'Menunggu Konfirmasi',
    status_pembayaran VARCHAR(20) DEFAULT 'Belum Bayar',
    FOREIGN KEY (id_capster) REFERENCES Capster(id_capster),
    FOREIGN KEY (id_jadwal) REFERENCES Jadwal(id_jadwal),
    FOREIGN KEY (id_layanan) REFERENCES Layanan(id_layanan)
);


INSERT INTO Capster (nama)
VALUES ('Ahmad'), ('Budi');

INSERT INTO Layanan (nama_layanan, harga) 
VALUES ('Haircut Reguler', 35000), ('Haircut + Wash', 50000);

INSERT INTO Jadwal (id_capster, hari, status_jadwal)
VALUES (1, 'Senin', 'Tersedia'), 
       (1, 'Selasa', 'Penuh'), 
       (2, 'Selasa', 'Tersedia');

INSERT INTO Reservasi (nama_pelanggan, id_layanan, id_capster, id_jadwal) 
VALUES (10, 1, 1, 1); 

SELECT 
    R.id_reservasi, 
    R.nama_pelanggan, 
    L.nama_layanan, 
    C.nama AS Nama_Capster, 
    J.hari, 
    J.status_jadwal, 
    R.status_reservasi
FROM Reservasi R
JOIN Layanan L ON R.id_layanan = L.id_layanan
JOIN Capster C ON R.id_capster = C.id_capster
JOIN Jadwal J ON R.id_jadwal = J.id_jadwal;

--------------------------------------------------------------------------------

ALTER TABLE Reservasi
ADD jam_booking TIME NOT NULL DEFAULT '00:00:00';
GO

DROP VIEW IF EXISTS v_DaftarReservasi;
GO

CREATE VIEW v_DaftarReservasi AS
SELECT R.id_reservasi, R.nama_pelanggan, L.nama_layanan, 
       C.nama AS Capster, J.hari, R.jam_booking, R.status_reservasi 
FROM Reservasi R
JOIN Layanan L ON R.id_layanan = L.id_layanan
JOIN Capster C ON R.id_capster = C.id_capster
JOIN Jadwal J ON R.id_jadwal = J.id_jadwal;
GO

CREATE PROCEDURE sp_TambahReservasi
    @nama VARCHAR(100),
    @id_lay INT,
    @id_cap INT,
    @id_jad INT,
    @jam TIME
AS
BEGIN
    INSERT INTO Reservasi (nama_pelanggan, id_layanan, id_capster, id_jadwal, jam_booking)
    VALUES (@nama, @id_lay, @id_cap, @id_jad, @jam);
END;
GO

DROP PROCEDURE sp_GantiReservasi;

CREATE PROCEDURE sp_GantiReservasi
    @id_res INT,
    @nama VARCHAR(100),
    @id_lay INT,
    @id_cap INT,
    @id_jad INT,
    @jam TIME
AS
BEGIN
    -- VALIDASI: Cek apakah status transaksi saat ini sudah 'Selesai'
    IF EXISTS (SELECT 1 FROM Reservasi WHERE id_reservasi = @id_res AND status_reservasi = 'Selesai')
    BEGIN
        RAISERROR('Transaksi sudah SELESAI! Data tidak boleh diubah lagi.', 16, 1);
    END
    ELSE
    BEGIN
        UPDATE Reservasi 
        SET nama_pelanggan = @nama, 
            id_layanan = @id_lay, 
            id_capster = @id_cap, 
            id_jadwal = @id_jad,
            jam_booking = @jam
        WHERE id_reservasi = @id_res;
    END
END;
GO

CREATE PROCEDURE sp_HapusReservasi
    @id_res INT
AS
BEGIN

    IF EXISTS (SELECT 1 FROM Reservasi WHERE id_reservasi = @id_res AND status_reservasi = 'Selesai')
    BEGIN
        RAISERROR('Transaksi sudah SELESAI! Data tidak boleh dihapus.', 16, 1);
    END
    ELSE
    BEGIN
        DELETE FROM Reservasi WHERE id_reservasi = @id_res;
    END
END;
GO


USE DBBarbershop;
GO

DECLARE @ConstraintName NVARCHAR(200);
SELECT @ConstraintName = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('Reservasi') 
  AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('Reservasi'), 'jam_booking', 'ColumnId');

IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Reservasi DROP CONSTRAINT ' + @ConstraintName);
END;
GO

ALTER TABLE Reservasi 
ALTER COLUMN jam_booking DATETIME NOT NULL;
GO

USE DBBarbershop;
GO

ALTER TABLE Reservasi DROP CONSTRAINT DF__Reservasi__jam_b__72C60C4A;
GO

ALTER TABLE Reservasi ALTER COLUMN jam_booking DATETIME NOT NULL;
GO

ALTER PROCEDURE sp_TambahReservasi
    @nama VARCHAR(100),
    @id_lay INT,
    @id_cap INT,
    @id_jad INT, 
    @jam DATETIME
AS
BEGIN
    INSERT INTO Reservasi (nama_pelanggan, id_layanan, id_capster, id_jadwal, jam_booking)
    VALUES (@nama, @id_lay, @id_cap, @id_jad, @jam);
END;
GO

ALTER PROCEDURE sp_GantiReservasi
    @id_reservasi INT,
    @nama_pelanggan VARCHAR(100),
    @id_layanan INT,
    @id_capster INT,
    @id_jadwal INT,
    @jam DATETIME
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Reservasi WHERE id_reservasi = @id_reservasi AND status_reservasi = 'Selesai')
    BEGIN
        RAISERROR('Transaksi sudah SELESAI! Data tidak boleh diubah lagi.', 16, 1);
    END
    ELSE
    BEGIN
        UPDATE Reservasi 
        SET nama_pelanggan = @nama_pelanggan, 
            id_layanan = @id_layanan, 
            id_capster = @id_capster, 
            id_jadwal = @id_jadwal,
            jam_booking = @jam
        WHERE id_reservasi = @id_reservasi;
    END
END;
GO

ALTER PROCEDURE sp_DeleteReservasi
    @id_reservasi INT
AS
BEGIN

    IF EXISTS (SELECT 1 FROM Reservasi WHERE id_reservasi = @id_reservasi AND status_reservasi = 'Selesai')
    BEGIN
        RAISERROR('Transaksi sudah SELESAI! Data tidak boleh dihapus.', 16, 1);
    END
    ELSE
    BEGIN
        DELETE FROM dbo.Reservasi WHERE id_reservasi = @id_reservasi;
    END
END;
GO


USE DBBarbershop;
GO

DECLARE @FKName NVARCHAR(200);
SELECT @FKName = name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('dbo.Reservasi') 
  AND referenced_object_id = OBJECT_ID('dbo.Jadwal');

IF @FKName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.Reservasi DROP CONSTRAINT ' + @FKName);
END;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Reservasi') AND name = 'id_jadwal')
BEGIN
    ALTER TABLE dbo.Reservasi DROP COLUMN id_jadwal;
END;
GO


ALTER PROCEDURE sp_TambahReservasi
    @nama VARCHAR(100),
    @id_lay INT,
    @id_cap INT,
    @jam DATETIME
AS
BEGIN
    INSERT INTO Reservasi (nama_pelanggan, id_layanan, id_capster, jam_booking)
    VALUES (@nama, @id_lay, @id_cap, @jam);
END;
GO


ALTER PROCEDURE sp_GantiReservasi
    @id_reservasi INT,
    @nama_pelanggan VARCHAR(100),
    @id_layanan INT,
    @id_capster INT,
    @jam DATETIME
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Reservasi WHERE id_reservasi = @id_reservasi AND status_reservasi = 'Selesai')
    BEGIN
        RAISERROR('Transaksi sudah SELESAI! Data tidak boleh diubah lagi.', 16, 1);
    END
    ELSE
    BEGIN
        UPDATE Reservasi 
        SET nama_pelanggan = @nama_pelanggan, 
            id_layanan = @id_layanan, 
            id_capster = @id_capster, 
            jam_booking = @jam
        WHERE id_reservasi = @id_reservasi;
    END
END;
GO

USE DBBarbershop;
GO

ALTER VIEW vw_Reservasi AS
SELECT 
    r.id_reservasi,
    r.nama_pelanggan,
    l.nama_layanan,
    l.harga,
    c.nama AS nama_capster,
    r.jam_booking, -- Kolom DATETIME baru
    r.status_reservasi,
    r.status_pembayaran
FROM Reservasi r
INNER JOIN Layanan l ON r.id_layanan = l.id_layanan
INNER JOIN Capster c ON r.id_capster = c.id_capster;
GO

USE DBBarbershop;
GO

IF OBJECT_ID('dbo.Reservasi_Backup') IS NOT NULL
    DROP TABLE dbo.Reservasi_Backup;
GO

CREATE TABLE Reservasi_Backup (
    id_reservasi INT,
    id_layanan INT,
    id_capster INT,
    jam_booking DATETIME,
    status_reservasi VARCHAR(50),
    status_pembayaran VARCHAR(50),
    nama_pelanggan VARCHAR(100)
);
GO

CREATE PROCEDURE sp_AutoBackupReservasi
AS
BEGIN
    TRUNCATE TABLE dbo.Reservasi_Backup;
    
    INSERT INTO dbo.Reservasi_Backup (id_reservasi, id_layanan, id_capster, jam_booking, status_reservasi, status_pembayaran, nama_pelanggan)
    SELECT id_reservasi, id_layanan, id_capster, jam_booking, status_reservasi, status_pembayaran, nama_pelanggan
    FROM dbo.Reservasi;
END;
GO


DROP VIEW IF EXISTS vw_Reservasi;
GO

CREATE VIEW vw_Reservasi AS
SELECT 
    r.id_reservasi,
    r.nama_pelanggan,
    r.id_layanan,
    l.nama_layanan,
    l.harga,
    r.id_capster,
    c.nama AS nama_capster, 
    r.jam_booking,          
    r.status_reservasi,
    r.status_pembayaran
FROM Reservasi r
INNER JOIN Layanan l ON r.id_layanan = l.id_layanan
INNER JOIN Capster c ON r.id_capster = c.id_capster;
GO


USE DBBarbershop;
GO

CREATE TABLE Log_AktivitasReservasi (
    id_log INT IDENTITY(1,1) PRIMARY KEY,
    id_reservasi INT,
    nama_pelanggan VARCHAR(100),
    status_lama VARCHAR(30),
    status_baru VARCHAR(30),
    waktu_perubahan DATETIME DEFAULT GETDATE(),
    keterangan VARCHAR(255)
);
GO

USE DBBarbershop;
GO

CREATE TRIGGER trg_LogStatusReservasi
ON Reservasi
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(status_reservasi)
    BEGIN
        INSERT INTO Log_AktivitasReservasi (id_reservasi, nama_pelanggan, status_lama, status_baru, keterangan)
        SELECT 
            i.id_reservasi,
            i.nama_pelanggan,
            d.status_reservasi AS status_lama,
            i.status_reservasi AS status_baru,
            'Status reservasi diubah oleh sistem kasir.'
        FROM inserted i
        JOIN deleted d ON i.id_reservasi = d.id_reservasi;
    END
END;
GO


SELECT * FROM Log_AktivitasReservasi;


USE DBBarbershop;
GO

CREATE PROCEDURE sp_ReportReservasi
    @status_bayar VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        r.id_reservasi,
        r.nama_pelanggan,
        l.nama_layanan,
        l.harga,
        c.nama AS nama_capster,
        r.jam_booking,
        r.status_reservasi,
        r.status_pembayaran
    FROM Reservasi r
    INNER JOIN Layanan l ON r.id_layanan = l.id_layanan
    INNER JOIN Capster c ON r.id_capster = c.id_capster
    WHERE r.status_pembayaran = @status_bayar;
END;
GO

