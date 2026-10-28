using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using CliniqueVeterinaire.Helpers;
using CliniqueVeterinaire.Models;

namespace CliniqueVeterinaire.Forms
{
    public class FrmSecretaire : Form
    {
        private TabControl tabControl;
        private Panel pnlHeader;

        // Contrôles Propriétaires
        private DataGridView dgvProprietaires;
        private TextBox txtRechercheProp;

        // Contrôles Animaux
        private DataGridView dgvAnimaux;
        private ComboBox cboFiltreProprietaire;
        private TextBox txtRechercheAnimal;

        // Contrôles Rendez-vous
        private DataGridView dgvRendezVous;
        private DateTimePicker dtpPlanning;
        private ComboBox cboFiltreVeto;

        // Contrôles Factures
        private DataGridView dgvFactures;
        private ComboBox cboFiltreStatut;

        public FrmSecretaire()
        {
            this.Text = "Secrétaire - Clinique Vétérinaire";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Header
            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 70;
            pnlHeader.BackColor = Color.DarkSlateBlue;

            Label lblWelcome = new Label();
            lblWelcome.Text = $"Bienvenue {SessionUtilisateur.Current.Prenom} {SessionUtilisateur.Current.Nom} (Secrétaire)";
            lblWelcome.ForeColor = Color.White;
            lblWelcome.Font = new Font("Arial", 14, FontStyle.Bold);
            lblWelcome.Location = new Point(20, 20);
            lblWelcome.Size = new Size(500, 30);

            Button btnLogout = new Button();
            btnLogout.Text = "Déconnexion";
            btnLogout.BackColor = Color.Crimson;
            btnLogout.ForeColor = Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Location = new Point(this.Width - 150, 18);
            btnLogout.Size = new Size(120, 35);
            btnLogout.Click += (s, e) => this.Close();

            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(btnLogout);

            // Ajuster la position du bouton quand la fenêtre est redimensionnée
            this.Resize += (s, e) => { btnLogout.Location = new Point(this.Width - 150, 18); };

            // TabControl
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            tabControl.TabPages.Add(CreerOngletProprietaires());
            tabControl.TabPages.Add(CreerOngletAnimaux());
            tabControl.TabPages.Add(CreerOngletRendezVous());
            tabControl.TabPages.Add(CreerOngletFactures());

            this.Controls.Add(tabControl);
            this.Controls.Add(pnlHeader);
        }

        // ==================== ONGLET 1 : PROPRIETAIRES ====================
        private TabPage CreerOngletProprietaires()
        {
            TabPage page = new TabPage("🏠 Propriétaires");
            Panel panel = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(20) };

            Label lblTitle = new Label() { Text = "Gestion des propriétaires", Font = new Font("Arial", 14, FontStyle.Bold), Location = new Point(20, 20), Size = new Size(300, 30) };

            Label lblRecherche = new Label() { Text = "Rechercher :", Location = new Point(20, 60), Size = new Size(80, 25) };
            txtRechercheProp = new TextBox() { Location = new Point(110, 60), Size = new Size(200, 25) };
            txtRechercheProp.TextChanged += (s, e) => ChargerProprietaires();

            Button btnAjouter = new Button() { Text = "+ Ajouter", BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(750, 20), Size = new Size(120, 35) };
            btnAjouter.Click += BtnAjouterProprietaire_Click;

            Button btnModifier = new Button() { Text = "✏ Modifier", BackColor = Color.DarkOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(880, 20), Size = new Size(100, 35) };
            btnModifier.Click += BtnModifierProprietaire_Click;

            Button btnSupprimer = new Button() { Text = "🗑 Supprimer", BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(990, 20), Size = new Size(100, 35) };
            btnSupprimer.Click += BtnSupprimerProprietaire_Click;

            dgvProprietaires = new DataGridView() { Location = new Point(20, 100), Size = new Size(1070, 500), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblRecherche);
            panel.Controls.Add(txtRechercheProp);
            panel.Controls.Add(btnAjouter);
            panel.Controls.Add(btnModifier);
            panel.Controls.Add(btnSupprimer);
            panel.Controls.Add(dgvProprietaires);

            page.Controls.Add(panel);
            ChargerProprietaires();
            return page;
        }

        private void ChargerProprietaires()
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                string sql = "SELECT * FROM Proprietaires";
                if (!string.IsNullOrWhiteSpace(txtRechercheProp.Text))
                    sql += " WHERE Nom LIKE @Search OR Prenom LIKE @Search OR Telephone LIKE @Search";

                var result = db.Query<Proprietaire>(sql, new { Search = $"%{txtRechercheProp.Text}%" });
                dgvProprietaires.DataSource = result.ToList();
            }
        }

        private void BtnAjouterProprietaire_Click(object sender, EventArgs e)
        {
            Form form = new Form() { Text = "Ajouter un propriétaire", Size = new Size(450, 350), StartPosition = FormStartPosition.CenterParent };

            TextBox txtNom = new TextBox() { Location = new Point(120, 30), Size = new Size(250, 25) };
            TextBox txtPrenom = new TextBox() { Location = new Point(120, 70), Size = new Size(250, 25) };
            TextBox txtTel = new TextBox() { Location = new Point(120, 110), Size = new Size(250, 25) };
            TextBox txtAdresse = new TextBox() { Location = new Point(120, 150), Size = new Size(250, 60), Multiline = true };

            Button btnValider = new Button() { Text = "Enregistrer", Location = new Point(120, 230), Size = new Size(100, 35), BackColor = Color.SeaGreen, ForeColor = Color.White };
            Button btnAnnuler = new Button() { Text = "Annuler", Location = new Point(230, 230), Size = new Size(100, 35) };

            form.Controls.Add(new Label() { Text = "Nom:", Location = new Point(30, 33), Size = new Size(80, 20) });
            form.Controls.Add(new Label() { Text = "Prénom:", Location = new Point(30, 73), Size = new Size(80, 20) });
            form.Controls.Add(new Label() { Text = "Téléphone:", Location = new Point(30, 113), Size = new Size(80, 20) });
            form.Controls.Add(new Label() { Text = "Adresse:", Location = new Point(30, 153), Size = new Size(80, 20) });
            form.Controls.Add(txtNom);
            form.Controls.Add(txtPrenom);
            form.Controls.Add(txtTel);
            form.Controls.Add(txtAdresse);
            form.Controls.Add(btnValider);
            form.Controls.Add(btnAnnuler);

            btnValider.Click += (s, ev) =>
            {
                if (!string.IsNullOrWhiteSpace(txtNom.Text))
                {
                    using (var db = DatabaseHelper.GetConnection())
                    {
                        db.Execute("INSERT INTO Proprietaires (Nom, Prenom, Telephone, Adresse) VALUES (@Nom, @Prenom, @Telephone, @Adresse)",
                            new { Nom = txtNom.Text, Prenom = txtPrenom.Text, Telephone = txtTel.Text, Adresse = txtAdresse.Text });
                    }
                    ChargerProprietaires();
                    MessageBox.Show("Propriétaire ajouté avec succès !");
                    form.Close();
                }
                else
                {
                    MessageBox.Show("Le nom est obligatoire.");
                }
            };
            btnAnnuler.Click += (s, ev) => form.Close();

            form.ShowDialog();
        }

        private void BtnModifierProprietaire_Click(object sender, EventArgs e)
        {
            if (dgvProprietaires.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un propriétaire à modifier.");
                return;
            }
            var prop = (Proprietaire)dgvProprietaires.SelectedRows[0].DataBoundItem;

            Form form = new Form() { Text = "Modifier un propriétaire", Size = new Size(450, 350), StartPosition = FormStartPosition.CenterParent };

            TextBox txtNom = new TextBox() { Location = new Point(120, 30), Size = new Size(250, 25), Text = prop.Nom };
            TextBox txtPrenom = new TextBox() { Location = new Point(120, 70), Size = new Size(250, 25), Text = prop.Prenom };
            TextBox txtTel = new TextBox() { Location = new Point(120, 110), Size = new Size(250, 25), Text = prop.Telephone };
            TextBox txtAdresse = new TextBox() { Location = new Point(120, 150), Size = new Size(250, 60), Multiline = true, Text = prop.Adresse };

            Button btnValider = new Button() { Text = "Modifier", Location = new Point(120, 230), Size = new Size(100, 35), BackColor = Color.DarkOrange, ForeColor = Color.White };
            Button btnAnnuler = new Button() { Text = "Annuler", Location = new Point(230, 230), Size = new Size(100, 35) };

            form.Controls.Add(new Label() { Text = "Nom:", Location = new Point(30, 33), Size = new Size(80, 20) });
            form.Controls.Add(new Label() { Text = "Prénom:", Location = new Point(30, 73), Size = new Size(80, 20) });
            form.Controls.Add(new Label() { Text = "Téléphone:", Location = new Point(30, 113), Size = new Size(80, 20) });
            form.Controls.Add(new Label() { Text = "Adresse:", Location = new Point(30, 153), Size = new Size(80, 20) });
            form.Controls.Add(txtNom);
            form.Controls.Add(txtPrenom);
            form.Controls.Add(txtTel);
            form.Controls.Add(txtAdresse);
            form.Controls.Add(btnValider);
            form.Controls.Add(btnAnnuler);

            btnValider.Click += (s, ev) =>
            {
                using (var db = DatabaseHelper.GetConnection())
                {
                    db.Execute("UPDATE Proprietaires SET Nom = @Nom, Prenom = @Prenom, Telephone = @Telephone, Adresse = @Adresse WHERE Id = @Id",
                        new { prop.Id, Nom = txtNom.Text, Prenom = txtPrenom.Text, Telephone = txtTel.Text, Adresse = txtAdresse.Text });
                }
                ChargerProprietaires();
                MessageBox.Show("Propriétaire modifié avec succès !");
                form.Close();
            };
            btnAnnuler.Click += (s, ev) => form.Close();

            form.ShowDialog();
        }

        private void BtnSupprimerProprietaire_Click(object sender, EventArgs e)
        {
            if (dgvProprietaires.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un propriétaire à supprimer.");
                return;
            }
            var prop = (Proprietaire)dgvProprietaires.SelectedRows[0].DataBoundItem;
            if (MessageBox.Show($"Supprimer {prop.Prenom} {prop.Nom} ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (var db = DatabaseHelper.GetConnection())
                {
                    db.Execute("DELETE FROM Proprietaires WHERE Id = @Id", new { prop.Id });
                }
                ChargerProprietaires();
                MessageBox.Show("Propriétaire supprimé.");
            }
        }

        // ==================== ONGLET 2 : ANIMAUX ====================
        private TabPage CreerOngletAnimaux()
        {
            TabPage page = new TabPage("🐾 Animaux");
            Panel panel = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(20) };

            Label lblTitle = new Label() { Text = "Gestion des animaux", Font = new Font("Arial", 14, FontStyle.Bold), Location = new Point(20, 20), Size = new Size(300, 30) };

            Label lblFiltre = new Label() { Text = "Filtrer par propriétaire:", Location = new Point(20, 60), Size = new Size(150, 25) };
            cboFiltreProprietaire = new ComboBox() { Location = new Point(180, 60), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboFiltreProprietaire.SelectedIndexChanged += (s, e) => ChargerAnimaux();

            Label lblRecherche = new Label() { Text = "Nom animal:", Location = new Point(420, 60), Size = new Size(80, 25) };
            txtRechercheAnimal = new TextBox() { Location = new Point(510, 60), Size = new Size(150, 25) };
            txtRechercheAnimal.TextChanged += (s, e) => ChargerAnimaux();

            Button btnAjouter = new Button() { Text = "+ Ajouter", BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(750, 20), Size = new Size(120, 35) };
            btnAjouter.Click += BtnAjouterAnimal_Click;

            Button btnModifier = new Button() { Text = "✏ Modifier", BackColor = Color.DarkOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(880, 20), Size = new Size(100, 35) };
            btnModifier.Click += BtnModifierAnimal_Click;

            Button btnSupprimer = new Button() { Text = "🗑 Supprimer", BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(990, 20), Size = new Size(100, 35) };
            btnSupprimer.Click += BtnSupprimerAnimal_Click;

            dgvAnimaux = new DataGridView() { Location = new Point(20, 100), Size = new Size(1070, 500), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblFiltre);
            panel.Controls.Add(cboFiltreProprietaire);
            panel.Controls.Add(lblRecherche);
            panel.Controls.Add(txtRechercheAnimal);
            panel.Controls.Add(btnAjouter);
            panel.Controls.Add(btnModifier);
            panel.Controls.Add(btnSupprimer);
            panel.Controls.Add(dgvAnimaux);

            page.Controls.Add(panel);
            ChargerComboProprietaires();
            ChargerAnimaux();
            return page;
        }

        private void ChargerComboProprietaires()
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                var props = db.Query<Proprietaire>("SELECT Id, Nom, Prenom FROM Proprietaires ORDER BY Nom");
                var liste = props.Select(p => new { Id = p.Id, NomComplet = $"{p.Nom} {p.Prenom}" }).ToList();
                cboFiltreProprietaire.DataSource = liste;
                cboFiltreProprietaire.DisplayMember = "NomComplet";
                cboFiltreProprietaire.ValueMember = "Id";
            }
        }

        private void ChargerAnimaux()
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                string sql = @"
                    SELECT a.*, p.Nom + ' ' + p.Prenom as NomProprietaire 
                    FROM Animaux a
                    JOIN Proprietaires p ON a.ProprietaireId = p.Id
                    WHERE 1=1";

                if (cboFiltreProprietaire.SelectedValue != null)
                    sql += " AND a.ProprietaireId = @PropId";
                if (!string.IsNullOrWhiteSpace(txtRechercheAnimal.Text))
                    sql += " AND a.Nom LIKE @Search";

                var result = db.Query(sql, new { PropId = cboFiltreProprietaire.SelectedValue, Search = $"%{txtRechercheAnimal.Text}%" });
                dgvAnimaux.DataSource = result.ToList();
            }
        }

        private void BtnAjouterAnimal_Click(object sender, EventArgs e)
        {
            Form form = new Form() { Text = "Ajouter un animal", Size = new Size(450, 400), StartPosition = FormStartPosition.CenterParent };

            TextBox txtNom = new TextBox() { Location = new Point(150, 30), Size = new Size(220, 25) };
            TextBox txtEspece = new TextBox() { Location = new Point(150, 70), Size = new Size(220, 25) };
            TextBox txtRace = new TextBox() { Location = new Point(150, 110), Size = new Size(220, 25) };
            DateTimePicker dtpNaissance = new DateTimePicker() { Location = new Point(150, 150), Size = new Size(220, 25), Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddYears(-1) };
            ComboBox cboProprietaire = new ComboBox() { Location = new Point(150, 190), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            using (var db = DatabaseHelper.GetConnection())
            {
                var props = db.Query<Proprietaire>("SELECT Id, Nom, Prenom FROM Proprietaires ORDER BY Nom");
                cboProprietaire.DataSource = props.Select(p => new { Id = p.Id, NomComplet = $"{p.Nom} {p.Prenom}" }).ToList();
                cboProprietaire.DisplayMember = "NomComplet";
                cboProprietaire.ValueMember = "Id";
            }

            Button btnValider = new Button() { Text = "Enregistrer", Location = new Point(150, 250), Size = new Size(100, 35), BackColor = Color.SeaGreen, ForeColor = Color.White };
            Button btnAnnuler = new Button() { Text = "Annuler", Location = new Point(260, 250), Size = new Size(100, 35) };

            form.Controls.Add(new Label() { Text = "Nom:", Location = new Point(30, 33), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Espèce:", Location = new Point(30, 73), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Race:", Location = new Point(30, 113), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Date naissance:", Location = new Point(30, 153), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Propriétaire:", Location = new Point(30, 193), Size = new Size(100, 20) });
            form.Controls.Add(txtNom);
            form.Controls.Add(txtEspece);
            form.Controls.Add(txtRace);
            form.Controls.Add(dtpNaissance);
            form.Controls.Add(cboProprietaire);
            form.Controls.Add(btnValider);
            form.Controls.Add(btnAnnuler);

            btnValider.Click += (s, ev) =>
            {
                if (!string.IsNullOrWhiteSpace(txtNom.Text) && cboProprietaire.SelectedValue != null)
                {
                    using (var db = DatabaseHelper.GetConnection())
                    {
                        db.Execute("INSERT INTO Animaux (Nom, Espece, Race, DateNaissance, ProprietaireId) VALUES (@Nom, @Espece, @Race, @DateNaissance, @ProprietaireId)",
                            new { Nom = txtNom.Text, Espece = txtEspece.Text, Race = txtRace.Text, DateNaissance = dtpNaissance.Value, ProprietaireId = (int)cboProprietaire.SelectedValue });
                    }
                    ChargerAnimaux();
                    MessageBox.Show("Animal ajouté avec succès !");
                    form.Close();
                }
                else
                {
                    MessageBox.Show("Le nom et le propriétaire sont obligatoires.");
                }
            };
            btnAnnuler.Click += (s, ev) => form.Close();

            form.ShowDialog();
        }

        private void BtnModifierAnimal_Click(object sender, EventArgs e)
        {
            if (dgvAnimaux.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un animal à modifier.");
                return;
            }
            dynamic animal = dgvAnimaux.SelectedRows[0].DataBoundItem;

            Form form = new Form() { Text = "Modifier un animal", Size = new Size(450, 400), StartPosition = FormStartPosition.CenterParent };

            TextBox txtNom = new TextBox() { Location = new Point(150, 30), Size = new Size(220, 25), Text = animal.Nom };
            TextBox txtEspece = new TextBox() { Location = new Point(150, 70), Size = new Size(220, 25), Text = animal.Espece };
            TextBox txtRace = new TextBox() { Location = new Point(150, 110), Size = new Size(220, 25), Text = animal.Race };
            DateTimePicker dtpNaissance = new DateTimePicker() { Location = new Point(150, 150), Size = new Size(220, 25), Format = DateTimePickerFormat.Short, Value = animal.DateNaissance };
            ComboBox cboProprietaire = new ComboBox() { Location = new Point(150, 190), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            using (var db = DatabaseHelper.GetConnection())
            {
                var props = db.Query<Proprietaire>("SELECT Id, Nom, Prenom FROM Proprietaires ORDER BY Nom");
                cboProprietaire.DataSource = props.Select(p => new { Id = p.Id, NomComplet = $"{p.Nom} {p.Prenom}" }).ToList();
                cboProprietaire.DisplayMember = "NomComplet";
                cboProprietaire.ValueMember = "Id";
                cboProprietaire.SelectedValue = (int)animal.ProprietaireId;
            }

            Button btnValider = new Button() { Text = "Modifier", Location = new Point(150, 250), Size = new Size(100, 35), BackColor = Color.DarkOrange, ForeColor = Color.White };
            Button btnAnnuler = new Button() { Text = "Annuler", Location = new Point(260, 250), Size = new Size(100, 35) };

            form.Controls.Add(new Label() { Text = "Nom:", Location = new Point(30, 33), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Espèce:", Location = new Point(30, 73), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Race:", Location = new Point(30, 113), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Date naissance:", Location = new Point(30, 153), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Propriétaire:", Location = new Point(30, 193), Size = new Size(100, 20) });
            form.Controls.Add(txtNom);
            form.Controls.Add(txtEspece);
            form.Controls.Add(txtRace);
            form.Controls.Add(dtpNaissance);
            form.Controls.Add(cboProprietaire);
            form.Controls.Add(btnValider);
            form.Controls.Add(btnAnnuler);

            btnValider.Click += (s, ev) =>
            {
                using (var db = DatabaseHelper.GetConnection())
                {
                    db.Execute("UPDATE Animaux SET Nom = @Nom, Espece = @Espece, Race = @Race, DateNaissance = @DateNaissance, ProprietaireId = @ProprietaireId WHERE Id = @Id",
                        new { Id = animal.Id, Nom = txtNom.Text, Espece = txtEspece.Text, Race = txtRace.Text, DateNaissance = dtpNaissance.Value, ProprietaireId = (int)cboProprietaire.SelectedValue });
                }
                ChargerAnimaux();
                MessageBox.Show("Animal modifié avec succès !");
                form.Close();
            };
            btnAnnuler.Click += (s, ev) => form.Close();

            form.ShowDialog();
        }

        private void BtnSupprimerAnimal_Click(object sender, EventArgs e)
        {
            if (dgvAnimaux.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un animal à supprimer.");
                return;
            }
            dynamic animal = dgvAnimaux.SelectedRows[0].DataBoundItem;
            if (MessageBox.Show($"Supprimer {animal.Nom} ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (var db = DatabaseHelper.GetConnection())
                {
                    db.Execute("DELETE FROM Animaux WHERE Id = @Id", new { Id = animal.Id });
                }
                ChargerAnimaux();
                MessageBox.Show("Animal supprimé.");
            }
        }

        // ==================== ONGLET 3 : RENDEZ-VOUS ====================
        private TabPage CreerOngletRendezVous()
        {
            TabPage page = new TabPage("📅 Rendez-vous");
            Panel panel = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(20) };

            Label lblTitle = new Label() { Text = "Gestion des rendez-vous", Font = new Font("Arial", 14, FontStyle.Bold), Location = new Point(20, 20), Size = new Size(300, 30) };

            Label lblDate = new Label() { Text = "Date:", Location = new Point(20, 60), Size = new Size(50, 25) };
            dtpPlanning = new DateTimePicker() { Location = new Point(80, 60), Size = new Size(150, 25), Format = DateTimePickerFormat.Short };
            dtpPlanning.ValueChanged += (s, e) => ChargerRendezVous();

            Label lblVeto = new Label() { Text = "Vétérinaire:", Location = new Point(260, 60), Size = new Size(80, 25) };
            cboFiltreVeto = new ComboBox() { Location = new Point(350, 60), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboFiltreVeto.SelectedIndexChanged += (s, e) => ChargerRendezVous();

            Button btnAjouter = new Button() { Text = "+ Nouveau RDV", BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(750, 20), Size = new Size(120, 35) };
            btnAjouter.Click += BtnAjouterRendezVous_Click;

            Button btnModifier = new Button() { Text = "✏ Modifier", BackColor = Color.DarkOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(880, 20), Size = new Size(100, 35) };
            btnModifier.Click += BtnModifierRendezVous_Click;

            Button btnAnnuler = new Button() { Text = "❌ Annuler", BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(990, 20), Size = new Size(100, 35) };
            btnAnnuler.Click += BtnAnnulerRendezVous_Click;

            dgvRendezVous = new DataGridView() { Location = new Point(20, 100), Size = new Size(1070, 500), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblDate);
            panel.Controls.Add(dtpPlanning);
            panel.Controls.Add(lblVeto);
            panel.Controls.Add(cboFiltreVeto);
            panel.Controls.Add(btnAjouter);
            panel.Controls.Add(btnModifier);
            panel.Controls.Add(btnAnnuler);
            panel.Controls.Add(dgvRendezVous);

            page.Controls.Add(panel);
            ChargerComboVeterinaires();
            ChargerRendezVous();
            return page;
        }

        private void ChargerComboVeterinaires()
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                var vetos = db.Query<Utilisateur>("SELECT Id, Nom, Prenom FROM Utilisateurs WHERE Role = 'Veterinaire'");
                var liste = vetos.Select(v => new { Id = v.Id, NomComplet = $"{v.Prenom} {v.Nom}" }).ToList();
                cboFiltreVeto.DataSource = liste;
                cboFiltreVeto.DisplayMember = "NomComplet";
                cboFiltreVeto.ValueMember = "Id";
            }
        }

        private void ChargerRendezVous()
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                string sql = @"
                    SELECT r.*, a.Nom as NomAnimal, u.Prenom + ' ' + u.Nom as NomVeterinaire 
                    FROM RendezVous r
                    JOIN Animaux a ON r.AnimalId = a.Id
                    JOIN Utilisateurs u ON r.VeterinaireId = u.Id
                    WHERE CAST(r.DateHeure AS DATE) = @Date";

                if (cboFiltreVeto.SelectedValue != null)
                    sql += " AND r.VeterinaireId = @VetoId";

                var result = db.Query(sql, new { Date = dtpPlanning.Value.Date, VetoId = cboFiltreVeto.SelectedValue });
                dgvRendezVous.DataSource = result.ToList();
            }
        }

        private void BtnAjouterRendezVous_Click(object sender, EventArgs e)
        {
            Form form = new Form() { Text = "Ajouter un rendez-vous", Size = new Size(500, 350), StartPosition = FormStartPosition.CenterParent };

            DateTimePicker dtpDate = new DateTimePicker() { Location = new Point(150, 30), Size = new Size(200, 25), Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", ShowUpDown = true };
            TextBox txtMotif = new TextBox() { Location = new Point(150, 70), Size = new Size(280, 25) };
            ComboBox cboAnimal = new ComboBox() { Location = new Point(150, 110), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            ComboBox cboVeto = new ComboBox() { Location = new Point(150, 150), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            using (var db = DatabaseHelper.GetConnection())
            {
                var animaux = db.Query("SELECT a.Id, a.Nom, p.Nom + ' ' + p.Prenom as PropNom FROM Animaux a JOIN Proprietaires p ON a.ProprietaireId = p.Id");
                cboAnimal.DataSource = animaux.Select(a => new { Id = (int)a.Id, NomComplet = $"{a.Nom} (propriétaire: {a.PropNom})" }).ToList();
                cboAnimal.DisplayMember = "NomComplet";
                cboAnimal.ValueMember = "Id";

                var vetos = db.Query<Utilisateur>("SELECT Id, Nom, Prenom FROM Utilisateurs WHERE Role = 'Veterinaire'");
                cboVeto.DataSource = vetos.Select(v => new { Id = v.Id, NomComplet = $"{v.Prenom} {v.Nom}" }).ToList();
                cboVeto.DisplayMember = "NomComplet";
                cboVeto.ValueMember = "Id";
            }

            Button btnValider = new Button() { Text = "Enregistrer", Location = new Point(150, 220), Size = new Size(100, 35), BackColor = Color.SeaGreen, ForeColor = Color.White };
            Button btnAnnuler = new Button() { Text = "Annuler", Location = new Point(260, 220), Size = new Size(100, 35) };

            form.Controls.Add(new Label() { Text = "Date & Heure:", Location = new Point(30, 33), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Motif:", Location = new Point(30, 73), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Animal:", Location = new Point(30, 113), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Vétérinaire:", Location = new Point(30, 153), Size = new Size(100, 20) });
            form.Controls.Add(dtpDate);
            form.Controls.Add(txtMotif);
            form.Controls.Add(cboAnimal);
            form.Controls.Add(cboVeto);
            form.Controls.Add(btnValider);
            form.Controls.Add(btnAnnuler);

            btnValider.Click += (s, ev) =>
            {
                if (cboAnimal.SelectedValue != null && cboVeto.SelectedValue != null)
                {
                    using (var db = DatabaseHelper.GetConnection())
                    {
                        db.Execute("INSERT INTO RendezVous (DateHeure, Motif, AnimalId, VeterinaireId, Statut) VALUES (@DateHeure, @Motif, @AnimalId, @VeterinaireId, 'Planifié')",
                            new { DateHeure = dtpDate.Value, Motif = txtMotif.Text, AnimalId = (int)cboAnimal.SelectedValue, VeterinaireId = (int)cboVeto.SelectedValue });
                    }
                    ChargerRendezVous();
                    MessageBox.Show("Rendez-vous ajouté avec succès !");
                    form.Close();
                }
                else
                {
                    MessageBox.Show("Veuillez sélectionner un animal et un vétérinaire.");
                }
            };
            btnAnnuler.Click += (s, ev) => form.Close();

            form.ShowDialog();
        }

        private void BtnModifierRendezVous_Click(object sender, EventArgs e)
        {
            if (dgvRendezVous.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un rendez-vous à modifier.");
                return;
            }
            dynamic rdv = dgvRendezVous.SelectedRows[0].DataBoundItem;

            Form form = new Form() { Text = "Modifier un rendez-vous", Size = new Size(500, 350), StartPosition = FormStartPosition.CenterParent };

            DateTimePicker dtpDate = new DateTimePicker() { Location = new Point(150, 30), Size = new Size(200, 25), Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", ShowUpDown = true, Value = rdv.DateHeure };
            TextBox txtMotif = new TextBox() { Location = new Point(150, 70), Size = new Size(280, 25), Text = rdv.Motif };
            ComboBox cboAnimal = new ComboBox() { Location = new Point(150, 110), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            ComboBox cboVeto = new ComboBox() { Location = new Point(150, 150), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            using (var db = DatabaseHelper.GetConnection())
            {
                var animaux = db.Query("SELECT a.Id, a.Nom, p.Nom + ' ' + p.Prenom as PropNom FROM Animaux a JOIN Proprietaires p ON a.ProprietaireId = p.Id");
                cboAnimal.DataSource = animaux.Select(a => new { Id = (int)a.Id, NomComplet = $"{a.Nom} (propriétaire: {a.PropNom})" }).ToList();
                cboAnimal.DisplayMember = "NomComplet";
                cboAnimal.ValueMember = "Id";
                cboAnimal.SelectedValue = (int)rdv.AnimalId;

                var vetos = db.Query<Utilisateur>("SELECT Id, Nom, Prenom FROM Utilisateurs WHERE Role = 'Veterinaire'");
                cboVeto.DataSource = vetos.Select(v => new { Id = v.Id, NomComplet = $"{v.Prenom} {v.Nom}" }).ToList();
                cboVeto.DisplayMember = "NomComplet";
                cboVeto.ValueMember = "Id";
                cboVeto.SelectedValue = (int)rdv.VeterinaireId;
            }

            Button btnValider = new Button() { Text = "Modifier", Location = new Point(150, 220), Size = new Size(100, 35), BackColor = Color.DarkOrange, ForeColor = Color.White };
            Button btnAnnuler = new Button() { Text = "Annuler", Location = new Point(260, 220), Size = new Size(100, 35) };

            form.Controls.Add(new Label() { Text = "Date & Heure:", Location = new Point(30, 33), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Motif:", Location = new Point(30, 73), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Animal:", Location = new Point(30, 113), Size = new Size(100, 20) });
            form.Controls.Add(new Label() { Text = "Vétérinaire:", Location = new Point(30, 153), Size = new Size(100, 20) });
            form.Controls.Add(dtpDate);
            form.Controls.Add(txtMotif);
            form.Controls.Add(cboAnimal);
            form.Controls.Add(cboVeto);
            form.Controls.Add(btnValider);
            form.Controls.Add(btnAnnuler);

            btnValider.Click += (s, ev) =>
            {
                using (var db = DatabaseHelper.GetConnection())
                {
                    db.Execute("UPDATE RendezVous SET DateHeure = @DateHeure, Motif = @Motif, AnimalId = @AnimalId, VeterinaireId = @VeterinaireId WHERE Id = @Id",
                        new { Id = rdv.Id, DateHeure = dtpDate.Value, Motif = txtMotif.Text, AnimalId = (int)cboAnimal.SelectedValue, VeterinaireId = (int)cboVeto.SelectedValue });
                }
                ChargerRendezVous();
                MessageBox.Show("Rendez-vous modifié avec succès !");
                form.Close();
            };
            btnAnnuler.Click += (s, ev) => form.Close();

            form.ShowDialog();
        }

        private void BtnAnnulerRendezVous_Click(object sender, EventArgs e)
        {
            if (dgvRendezVous.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un rendez-vous à annuler.");
                return;
            }
            dynamic rdv = dgvRendezVous.SelectedRows[0].DataBoundItem;
            if (MessageBox.Show($"Annuler le rendez-vous du {rdv.DateHeure} ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (var db = DatabaseHelper.GetConnection())
                {
                    db.Execute("UPDATE RendezVous SET Statut = 'Annulé' WHERE Id = @Id", new { Id = rdv.Id });
                }
                ChargerRendezVous();
                MessageBox.Show("Rendez-vous annulé.");
            }
        }

        // ==================== ONGLET 4 : FACTURES ====================
        private TabPage CreerOngletFactures()
        {
            TabPage page = new TabPage("💰 Factures");
            Panel panel = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(20) };

            Label lblTitle = new Label() { Text = "Gestion des factures", Font = new Font("Arial", 14, FontStyle.Bold), Location = new Point(20, 20), Size = new Size(300, 30) };

            Label lblStatut = new Label() { Text = "Statut:", Location = new Point(20, 60), Size = new Size(50, 25) };
            cboFiltreStatut = new ComboBox() { Location = new Point(80, 60), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboFiltreStatut.Items.AddRange(new string[] { "-- Tous --", "Impayé", "Payé" });
            cboFiltreStatut.SelectedIndex = 0;
            cboFiltreStatut.SelectedIndexChanged += (s, e) => ChargerFactures();

            Button btnRafraichir = new Button() { Text = "🔄 Rafraîchir", Location = new Point(250, 60), Size = new Size(100, 25) };
            btnRafraichir.Click += (s, e) => ChargerFactures();

            Button btnGenerer = new Button() { Text = "+ Nouvelle facture", BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(750, 20), Size = new Size(150, 35) };
            btnGenerer.Click += BtnGenererFacture_Click;

            Button btnPayer = new Button() { Text = "💰 Marquer payée", BackColor = Color.DarkOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(910, 20), Size = new Size(130, 35) };
            btnPayer.Click += BtnPayerFacture_Click;

            dgvFactures = new DataGridView() { Location = new Point(20, 100), Size = new Size(1070, 500), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblStatut);
            panel.Controls.Add(cboFiltreStatut);
            panel.Controls.Add(btnRafraichir);
            panel.Controls.Add(btnGenerer);
            panel.Controls.Add(btnPayer);
            panel.Controls.Add(dgvFactures);

            page.Controls.Add(panel);
            ChargerFactures();
            return page;
        }

        private void ChargerFactures()
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                string sql = @"
                    SELECT f.*, a.Nom as NomAnimal 
                    FROM Factures f
                    JOIN Consultations c ON f.ConsultationId = c.Id
                    JOIN Animaux a ON c.AnimalId = a.Id
                    WHERE 1=1";

                if (cboFiltreStatut.SelectedIndex == 1)
                    sql += " AND f.StatutPaiement = 'Impayé'";
                else if (cboFiltreStatut.SelectedIndex == 2)
                    sql += " AND f.StatutPaiement = 'Payé'";

                sql += " ORDER BY f.DateEmission DESC";

                var result = db.Query(sql);
                dgvFactures.DataSource = result.ToList();
            }
        }

        private void BtnGenererFacture_Click(object sender, EventArgs e)
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                var consultations = db.Query(@"
                    SELECT c.Id, a.Nom as NomAnimal, c.DateHeure 
                    FROM Consultations c
                    JOIN Animaux a ON c.AnimalId = a.Id
                    LEFT JOIN Factures f ON c.Id = f.ConsultationId
                    WHERE f.Id IS NULL");

                if (!consultations.Any())
                {
                    MessageBox.Show("Toutes les consultations ont déjà une facture.");
                    return;
                }

                Form form = new Form() { Text = "Générer une facture", Size = new Size(400, 250), StartPosition = FormStartPosition.CenterParent };
                ComboBox cboConsult = new ComboBox() { Location = new Point(150, 30), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
                cboConsult.DataSource = consultations.Select(c => new { Id = (int)c.Id, Description = $"Consultation du {c.DateHeure:dd/MM/yyyy} - {c.NomAnimal}" }).ToList();
                cboConsult.DisplayMember = "Description";
                cboConsult.ValueMember = "Id";

                TextBox txtMontant = new TextBox() { Location = new Point(150, 70), Size = new Size(200, 25), Text = "50" };
                Button btnValider = new Button() { Text = "Générer", Location = new Point(150, 130), Size = new Size(100, 35), BackColor = Color.SeaGreen, ForeColor = Color.White };
                Button btnAnnuler = new Button() { Text = "Annuler", Location = new Point(260, 130), Size = new Size(100, 35) };

                form.Controls.Add(new Label() { Text = "Consultation:", Location = new Point(30, 33), Size = new Size(100, 20) });
                form.Controls.Add(new Label() { Text = "Montant (€):", Location = new Point(30, 73), Size = new Size(100, 20) });
                form.Controls.Add(cboConsult);
                form.Controls.Add(txtMontant);
                form.Controls.Add(btnValider);
                form.Controls.Add(btnAnnuler);

                btnAnnuler.Click += (s, ev) => form.Close();
                btnValider.Click += (s, ev) =>
                {
                    if (decimal.TryParse(txtMontant.Text, out decimal montant))
                    {
                        db.Execute("INSERT INTO Factures (DateEmission, MontantTotal, ConsultationId) VALUES (GETDATE(), @Montant, @ConsultId)",
                            new { Montant = montant, ConsultId = (int)cboConsult.SelectedValue });
                        MessageBox.Show("Facture générée avec succès !");
                        ChargerFactures();
                        form.Close();
                    }
                    else
                    {
                        MessageBox.Show("Montant invalide.");
                    }
                };

                form.ShowDialog();
            }
        }

        private void BtnPayerFacture_Click(object sender, EventArgs e)
        {
            if (dgvFactures.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez une facture à payer.");
                return;
            }
            dynamic facture = dgvFactures.SelectedRows[0].DataBoundItem;
            if (facture.StatutPaiement == "Payé")
            {
                MessageBox.Show("Cette facture est déjà payée.");
                return;
            }
            if (MessageBox.Show($"Payer la facture de {facture.MontantTotal} € ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (var db = DatabaseHelper.GetConnection())
                {
                    db.Execute("UPDATE Factures SET StatutPaiement = 'Payé' WHERE Id = @Id", new { Id = facture.Id });
                }
                ChargerFactures();
                MessageBox.Show("Facture payée avec succès !");
            }
        }
    }
}