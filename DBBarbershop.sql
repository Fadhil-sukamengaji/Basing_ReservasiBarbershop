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

CREATE TABLE Reservasi (
    id_reservasi INT PRIMARY KEY IDENTITY(1,1),
    nama_pelanggan varchar(100),
    id_layanan INT,
    id_capster INT,
    id_jadwal INT,
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