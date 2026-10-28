-- =============================================
-- CRÉATION COMPLÈTE DE LA BASE DE DONNÉES
-- =============================================

-- Créer la base
CREATE DATABASE CliniqueVeto;
GO

-- Utiliser la base
USE CliniqueVeto;
GO

-- =============================================
-- CRÉATION DES TABLES
-- =============================================

-- Table Utilisateurs
CREATE TABLE Utilisateurs (
    Id INT PRIMARY KEY IDENTITY,
    Nom NVARCHAR(50) NOT NULL,
    Prenom NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    MotDePasse NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'Secretaire', 'Veterinaire'))
);
GO

-- Table Proprietaires
CREATE TABLE Proprietaires (
    Id INT PRIMARY KEY IDENTITY,
    Nom NVARCHAR(50) NOT NULL,
    Prenom NVARCHAR(50) NOT NULL,
    Telephone NVARCHAR(20) NOT NULL,
    Adresse NVARCHAR(200) NOT NULL
);
GO

-- Table Animaux
CREATE TABLE Animaux (
    Id INT PRIMARY KEY IDENTITY,
    Nom NVARCHAR(50) NOT NULL,
    Espece NVARCHAR(50) NOT NULL,
    Race NVARCHAR(50) NULL,
    DateNaissance DATE NULL,
    ProprietaireId INT NOT NULL FOREIGN KEY REFERENCES Proprietaires(Id) ON DELETE CASCADE
);
GO

-- Table Medicaments
CREATE TABLE Medicaments (
    Id INT PRIMARY KEY IDENTITY,
    NomCommercial NVARCHAR(100) NOT NULL,
    QuantiteStock INT NOT NULL DEFAULT 0,
    DateExpiration DATE NOT NULL
);
GO

-- Table RendezVous
CREATE TABLE RendezVous (
    Id INT PRIMARY KEY IDENTITY,
    DateHeure DATETIME NOT NULL,
    Motif NVARCHAR(200) NULL,
    Statut NVARCHAR(20) NOT NULL DEFAULT 'Planifié',
    AnimalId INT NOT NULL FOREIGN KEY REFERENCES Animaux(Id) ON DELETE CASCADE,
    VeterinaireId INT NOT NULL FOREIGN KEY REFERENCES Utilisateurs(Id)
);
GO

-- Table Consultations
CREATE TABLE Consultations (
    Id INT PRIMARY KEY IDENTITY,
    DateHeure DATETIME NOT NULL,
    Diagnostic NVARCHAR(MAX) NULL,
    Traitement NVARCHAR(MAX) NULL,
    Remarques NVARCHAR(MAX) NULL,
    RendezVousId INT NULL FOREIGN KEY REFERENCES RendezVous(Id),
    AnimalId INT NOT NULL FOREIGN KEY REFERENCES Animaux(Id) ON DELETE CASCADE
);
GO

-- Table Vaccinations
CREATE TABLE Vaccinations (
    Id INT PRIMARY KEY IDENTITY,
    NomVaccin NVARCHAR(100) NOT NULL,
    DateAdministration DATE NOT NULL,
    DateRappel DATE NULL,
    AnimalId INT NOT NULL FOREIGN KEY REFERENCES Animaux(Id) ON DELETE CASCADE
);
GO

-- Table Factures
CREATE TABLE Factures (
    Id INT PRIMARY KEY IDENTITY,
    DateEmission DATETIME NOT NULL DEFAULT GETDATE(),
    MontantTotal DECIMAL(10,2) NOT NULL,
    StatutPaiement NVARCHAR(20) NOT NULL DEFAULT 'Impayé',
    ConsultationId INT NOT NULL FOREIGN KEY REFERENCES Consultations(Id)
);
GO

-- Original seed data omitted for public sharing.

-- =============================================
-- VÉRIFICATION (optionnel)
-- =============================================
SELECT 'Base créée avec succès !' AS Message;
SELECT COUNT(*) AS NombreUtilisateurs FROM Utilisateurs;
SELECT COUNT(*) AS NombreProprietaires FROM Proprietaires;
SELECT COUNT(*) AS NombreAnimaux FROM Animaux;
