CREATE DATABASE IF NOT EXISTS mini_restoran;
USE mini_restoran;

CREATE TABLE proizvodi (
    id INT AUTO_INCREMENT PRIMARY KEY,
    naziv VARCHAR(100) NOT NULL,
    cijena DECIMAL(10,2) NOT NULL
);

CREATE TABLE narudzbe (
    id INT AUTO_INCREMENT PRIMARY KEY,
    datum DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE stavke_narudzbe (
    id INT AUTO_INCREMENT PRIMARY KEY,
    narudzba_id INT NOT NULL,
    proizvod_id INT NOT NULL,
    kolicina INT NOT NULL,
    cijena DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (narudzba_id) REFERENCES narudzbe(id),
    FOREIGN KEY (proizvod_id) REFERENCES proizvodi(id)
);

INSERT INTO proizvodi (naziv, cijena)
VALUES ('Pizza', 10.00), ('Burger', 8.00), ('Sok', 3.00);
