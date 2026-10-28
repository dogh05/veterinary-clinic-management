-- Fictional accounts for a LOCAL academic demo only.
-- Requires SQLQuery1.sql to have been executed first.
USE CliniqueVeto;
GO
INSERT INTO Utilisateurs (Nom, Prenom, Email, MotDePasse, Role) VALUES
(N'Demo', N'Admin', N'admin@example.invalid', N'PBKDF2-SHA256$600000$ImKP5yB78Pipjl72IkAX6g==$cRfq+G9ZS0zqIMlUByDWknxAxCcztewBTOLWKuDWEkI=', N'Admin'),
(N'Demo', N'Secretary', N'secretary@example.invalid', N'PBKDF2-SHA256$600000$moOp2685OiYPceDKJB+lRQ==$K0JE4t01IZcHVcWP7eV2mKKXW6golKze0WcSz8g1RCM=', N'Secretaire'),
(N'Demo', N'Veterinarian', N'vet@example.invalid', N'PBKDF2-SHA256$600000$mnbxOmcR65Cv5q21hiM5Vg==$7zykuq55RD37rOQr2VIrxZUTekhImMtHGWAAxIwsk78=', N'Veterinaire');
GO
