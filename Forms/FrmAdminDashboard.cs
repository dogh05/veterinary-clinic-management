using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using CliniqueVeterinaire.Helpers;
using CliniqueVeterinaire.Models;

namespace CliniqueVeterinaire.Forms
{
    public class FrmAdminDashboard : Form
    {
        private TabControl tabControl;
        private Panel pnlHeader;

        public FrmAdminDashboard()
        {
            this.Text = "Administrateur - Clinique Vétérinaire";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            InitializeUI();
            ChargerStatistiques();
        }

        private void InitializeUI()
        {
            // Header avec bienvenue
            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 80;
            pnlHeader.BackColor = Color.DarkSlateBlue;

            Label lblWelcome = new Label();
            lblWelcome.Text = $"Bienvenue {SessionUtilisateur.Current.Prenom} {SessionUtilisateur.Current.Nom} (Administrateur)";
            lblWelcome.ForeColor = Color.White;
            lblWelcome.Font = new Font("Arial", 14, FontStyle.Bold);
            lblWelcome.Location = new Point(20, 20);
            lblWelcome.Size = new Size(500, 30);

            Button btnLogout = new Button();
            btnLogout.Text = "Déconnexion";
            btnLogout.BackColor = Color.Crimson;
            btnLogout.ForeColor = Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Location = new Point(this.Width - 150, 25);
            btnLogout.Size = new Size(120, 35);
            btnLogout.Click += (s, e) => this.Close();

            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(btnLogout);

            // TabControl pour les différentes sections
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            // Ajouter les onglets
            tabControl.TabPages.Add(CreerOngletDashboard());
            tabControl.TabPages.Add(CreerOngletGestionUtilisateurs());
            tabControl.TabPages.Add(CreerOngletStockMedicaments());
            tabControl.TabPages.Add(CreerOngletRecherche());

            this.Controls.Add(tabControl);
            this.Controls.Add(pnlHeader);
        }

        // ==================== ONGLET 1 : DASHBOARD ====================
        private TabPage CreerOngletDashboard()
        {
            TabPage page = new TabPage("📊 Tableau de bord");

            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(20);

            Label lblStatsTitle = new Label();
            lblStatsTitle.Text = "Statistiques de la clinique";
            lblStatsTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            lblStatsTitle.Location = new Point(20, 20);
            lblStatsTitle.Size = new Size(300, 30);

            FlowLayoutPanel flowStats = new FlowLayoutPanel();
            flowStats.Location = new Point(20, 70);
            flowStats.Size = new Size(900, 150);

            panel.Controls.Add(lblStatsTitle);
            panel.Controls.Add(flowStats);

            // Charger les stats
            using (var db = DatabaseHelper.GetConnection())
            {
                int nbRdv = db.ExecuteScalar<int>("SELECT COUNT(*) FROM RendezVous");
                int nbConsult = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Consultations");
                decimal revenus = db.ExecuteScalar<decimal>("SELECT ISNULL(SUM(MontantTotal), 0) FROM Factures WHERE StatutPaiement = 'Payé'");
                int nbProprietaires = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Proprietaires");

                flowStats.Controls.Add(CreerCarteStatistique("Rendez-vous", nbRdv.ToString(), Color.SteelBlue));
                flowStats.Controls.Add(CreerCarteStatistique("Consultations", nbConsult.ToString(), Color.SeaGreen));
                flowStats.Controls.Add(CreerCarteStatistique("Revenus", $"{revenus} €", Color.Goldenrod));
                flowStats.Controls.Add(CreerCarteStatistique("Propriétaires", nbProprietaires.ToString(), Color.Purple));
            }

            page.Controls.Add(panel);
            return page;
        }

        private Panel CreerCarteStatistique(string titre, string valeur, Color couleur)
        {
            Panel card = new Panel();
            card.Size = new Size(200, 100);
            card.BackColor = couleur;
            card.Padding = new Padding(10);

            Label lblTitre = new Label();
            lblTitre.Text = titre;
            lblTitre.ForeColor = Color.White;
            lblTitre.Font = new Font("Arial", 12, FontStyle.Bold);
            lblTitre.Location = new Point(10, 10);
            lblTitre.Size = new Size(180, 25);
            lblTitre.TextAlign = ContentAlignment.MiddleCenter;

            Label lblValeur = new Label();
            lblValeur.Text = valeur;
            lblValeur.ForeColor = Color.White;
            lblValeur.Font = new Font("Arial", 20, FontStyle.Bold);
            lblValeur.Location = new Point(10, 45);
            lblValeur.Size = new Size(180, 40);
            lblValeur.TextAlign = ContentAlignment.MiddleCenter;

            card.Controls.Add(lblTitre);
            card.Controls.Add(lblValeur);

            return card;
        }

        private void ChargerStatistiques()
        {
            // Les statistiques sont déjà chargées dans l'onglet
        }

        // ==================== ONGLET 2 : GESTION UTILISATEURS ====================
        private DataGridView dgvUtilisateurs;

        private TabPage CreerOngletGestionUtilisateurs()
        {
            TabPage page = new TabPage("👥 Utilisateurs");

            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(20);

            Button btnAjouter = new Button();
            btnAjouter.Text = "+ Ajouter un utilisateur";
            btnAjouter.BackColor = Color.SeaGreen;
            btnAjouter.ForeColor = Color.White;
            btnAjouter.FlatStyle = FlatStyle.Flat;
            btnAjouter.Location = new Point(20, 20);
            btnAjouter.Size = new Size(180, 40);
            btnAjouter.Click += BtnAjouterUtilisateur_Click;

            Button btnModifier = new Button();
            btnModifier.Text = "✏ Modifier";
            btnModifier.BackColor = Color.DarkOrange;
            btnModifier.ForeColor = Color.White;
            btnModifier.FlatStyle = FlatStyle.Flat;
            btnModifier.Location = new Point(210, 20);
            btnModifier.Size = new Size(120, 40);
            btnModifier.Click += BtnModifierUtilisateur_Click;

            Button btnSupprimer = new Button();
            btnSupprimer.Text = "🗑 Supprimer";
            btnSupprimer.BackColor = Color.Crimson;
            btnSupprimer.ForeColor = Color.White;
            btnSupprimer.FlatStyle = FlatStyle.Flat;
            btnSupprimer.Location = new Point(340, 20);
            btnSupprimer.Size = new Size(120, 40);
            btnSupprimer.Click += BtnSupprimerUtilisateur_Click;

            Button btnRafraichir = new Button();
            btnRafraichir.Text = "🔄 Rafraîchir";
            btnRafraichir.Location = new Point(700, 20);
            btnRafraichir.Size = new Size(120, 40);
            btnRafraichir.Click += (s, e) => ChargerUtilisateurs();

            dgvUtilisateurs = new DataGridView();
            dgvUtilisateurs.Location = new Point(20, 80);
            dgvUtilisateurs.Size = new Size(900, 450);
            dgvUtilisateurs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUtilisateurs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUtilisateurs.MultiSelect = false;

            panel.Controls.Add(btnAjouter);
            panel.Controls.Add(btnModifier);
            panel.Controls.Add(btnSupprimer);
            panel.Controls.Add(btnRafraichir);
            panel.Controls.Add(dgvUtilisateurs);

            page.Controls.Add(panel);

            ChargerUtilisateurs();

            return page;
        }

        private void ChargerUtilisateurs()
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                var utilisateurs = db.Query<Utilisateur>("SELECT * FROM Utilisateurs ORDER BY Id");
                dgvUtilisateurs.DataSource = utilisateurs.ToList();
                if (dgvUtilisateurs.Columns.Contains("MotDePasse"))
                    dgvUtilisateurs.Columns["MotDePasse"].Visible = false;
            }
        }

        private void BtnAjouterUtilisateur_Click(object sender, EventArgs e)
        {
            Form formAjout = new Form();
            formAjout.Text = "Ajouter un utilisateur";
            formAjout.Size = new Size(400, 350);
            formAjout.StartPosition = FormStartPosition.CenterParent;

            // Champs du formulaire
            TextBox txtNom = new TextBox() { Location = new Point(120, 30), Size = new Size(200, 25) };
            TextBox txtPrenom = new TextBox() { Location = new Point(120, 70), Size = new Size(200, 25) };
            TextBox txtEmail = new TextBox() { Location = new Point(120, 110), Size = new Size(200, 25) };
            TextBox txtMdp = new TextBox() { Location = new Point(120, 150), Size = new Size(200, 25), PasswordChar = '*' };
            ComboBox cboRole = new ComboBox() { Location = new Point(120, 190), Size = new Size(200, 25) };
            cboRole.Items.AddRange(new string[] { "Admin", "Secretaire", "Veterinaire" });
            cboRole.SelectedIndex = 0;

            Button btnValider = new Button() { Text = "Ajouter", Location = new Point(120, 240), Size = new Size(100, 35), BackColor = Color.SeaGreen, ForeColor = Color.White };
            Button btnAnnuler = new Button() { Text = "Annuler", Location = new Point(230, 240), Size = new Size(100, 35) };

            // Labels
            formAjout.Controls.Add(new Label() { Text = "Nom:", Location = new Point(30, 33), Size = new Size(80, 20) });
            formAjout.Controls.Add(new Label() { Text = "Prénom:", Location = new Point(30, 73), Size = new Size(80, 20) });
            formAjout.Controls.Add(new Label() { Text = "Email:", Location = new Point(30, 113), Size = new Size(80, 20) });
            formAjout.Controls.Add(new Label() { Text = "Mot de passe:", Location = new Point(30, 153), Size = new Size(80, 20) });
            formAjout.Controls.Add(new Label() { Text = "Rôle:", Location = new Point(30, 193), Size = new Size(80, 20) });

            formAjout.Controls.Add(txtNom);
            formAjout.Controls.Add(txtPrenom);
            formAjout.Controls.Add(txtEmail);
            formAjout.Controls.Add(txtMdp);
            formAjout.Controls.Add(cboRole);
            formAjout.Controls.Add(btnValider);
            formAjout.Controls.Add(btnAnnuler);

            btnAnnuler.Click += (s, ev) => formAjout.Close();

            btnValider.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtNom.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtMdp.Text))
                {
                    MessageBox.Show("Veuillez remplir tous les champs obligatoires.");
                    return;
                }

                using (var db = DatabaseHelper.GetConnection())
                {
                    try
                    {
                        db.Execute(
                            "INSERT INTO Utilisateurs (Nom, Prenom, Email, MotDePasse, Role) VALUES (@Nom, @Prenom, @Email, @Mdp, @Role)",
                            new { Nom = txtNom.Text, Prenom = txtPrenom.Text, Email = txtEmail.Text, Mdp = PasswordHasher.Hash(txtMdp.Text), Role = cboRole.SelectedItem.ToString() });

                        MessageBox.Show("Utilisateur ajouté avec succès !");
                        ChargerUtilisateurs();
                        formAjout.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur : " + ex.Message);
                    }
                }
            };

            formAjout.ShowDialog();
        }

        private void BtnModifierUtilisateur_Click(object sender, EventArgs e)
        {
            if (dgvUtilisateurs.SelectedRows.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur à modifier.");
                return;
            }

            var utilisateur = (Utilisateur)dgvUtilisateurs.SelectedRows[0].DataBoundItem;

            Form formModif = new Form();
            formModif.Text = "Modifier un utilisateur";
            formModif.Size = new Size(400, 350);
            formModif.StartPosition = FormStartPosition.CenterParent;

            TextBox txtNom = new TextBox() { Location = new Point(120, 30), Size = new Size(200, 25), Text = utilisateur.Nom };
            TextBox txtPrenom = new TextBox() { Location = new Point(120, 70), Size = new Size(200, 25), Text = utilisateur.Prenom };
            TextBox txtEmail = new TextBox() { Location = new Point(120, 110), Size = new Size(200, 25), Text = utilisateur.Email };
            TextBox txtMdp = new TextBox() { Location = new Point(120, 150), Size = new Size(200, 25), PasswordChar = '*' };
            txtMdp.Text = ""; // Laisser vide pour ne pas changer
            ComboBox cboRole = new ComboBox() { Location = new Point(120, 190), Size = new Size(200, 25) };
            cboRole.Items.AddRange(new string[] { "Admin", "Secretaire", "Veterinaire" });
            cboRole.SelectedItem = utilisateur.Role;

            Button btnValider = new Button() { Text = "Modifier", Location = new Point(120, 240), Size = new Size(100, 35), BackColor = Color.DarkOrange, ForeColor = Color.White };
            Button btnAnnuler = new Button() { Text = "Annuler", Location = new Point(230, 240), Size = new Size(100, 35) };

            formModif.Controls.Add(new Label() { Text = "Nom:", Location = new Point(30, 33), Size = new Size(80, 20) });
            formModif.Controls.Add(new Label() { Text = "Prénom:", Location = new Point(30, 73), Size = new Size(80, 20) });
            formModif.Controls.Add(new Label() { Text = "Email:", Location = new Point(30, 113), Size = new Size(80, 20) });
            formModif.Controls.Add(new Label() { Text = "Mot de passe:", Location = new Point(30, 153), Size = new Size(80, 20) });
            formModif.Controls.Add(new Label() { Text = "Rôle:", Location = new Point(30, 193), Size = new Size(80, 20) });

            formModif.Controls.Add(txtNom);
            formModif.Controls.Add(txtPrenom);
            formModif.Controls.Add(txtEmail);
            formModif.Controls.Add(txtMdp);
            formModif.Controls.Add(cboRole);
            formModif.Controls.Add(btnValider);
            formModif.Controls.Add(btnAnnuler);

            btnAnnuler.Click += (s, ev) => formModif.Close();

            btnValider.Click += (s, ev) =>
            {
                using (var db = DatabaseHelper.GetConnection())
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(txtMdp.Text))
                        {
                            // Sans changer le mot de passe
                            db.Execute(
                                "UPDATE Utilisateurs SET Nom = @Nom, Prenom = @Prenom, Email = @Email, Role = @Role WHERE Id = @Id",
                                new { Id = utilisateur.Id, Nom = txtNom.Text, Prenom = txtPrenom.Text, Email = txtEmail.Text, Role = cboRole.SelectedItem.ToString() });
                        }
                        else
                        {
                            // Avec changement de mot de passe
                            db.Execute(
                                "UPDATE Utilisateurs SET Nom = @Nom, Prenom = @Prenom, Email = @Email, MotDePasse = @Mdp, Role = @Role WHERE Id = @Id",
                                new { Id = utilisateur.Id, Nom = txtNom.Text, Prenom = txtPrenom.Text, Email = txtEmail.Text, Mdp = PasswordHasher.Hash(txtMdp.Text), Role = cboRole.SelectedItem.ToString() });
                        }

                        MessageBox.Show("Utilisateur modifié avec succès !");
                        ChargerUtilisateurs();
                        formModif.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur : " + ex.Message);
                    }
                }
            };

            formModif.ShowDialog();
        }

        private void BtnSupprimerUtilisateur_Click(object sender, EventArgs e)
        {
            if (dgvUtilisateurs.SelectedRows.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur à supprimer.");
                return;
            }

            var utilisateur = (Utilisateur)dgvUtilisateurs.SelectedRows[0].DataBoundItem;

            if (utilisateur.Email == SessionUtilisateur.Current.Email)
            {
                MessageBox.Show("Vous ne pouvez pas supprimer votre propre compte.");
                return;
            }

            if (MessageBox.Show($"Supprimer {utilisateur.Prenom} {utilisateur.Nom} ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (var db = DatabaseHelper.GetConnection())
                {
                    db.Execute("DELETE FROM Utilisateurs WHERE Id = @Id", new { Id = utilisateur.Id });
                    MessageBox.Show("Utilisateur supprimé.");
                    ChargerUtilisateurs();
                }
            }
        }

        // ==================== ONGLET 3 : STOCK MÉDICAMENTS ====================
        private DataGridView dgvMedicaments;

        private TabPage CreerOngletStockMedicaments()
        {
            TabPage page = new TabPage("💊 Stock médicaments");

            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(20);

            Label lblTitle = new Label();
            lblTitle.Text = "Gestion du stock de médicaments";
            lblTitle.Font = new Font("Arial", 14, FontStyle.Bold);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Size = new Size(300, 30);

            Button btnRafraichir = new Button();
            btnRafraichir.Text = "🔄 Rafraîchir";
            btnRafraichir.Location = new Point(800, 20);
            btnRafraichir.Size = new Size(120, 35);
            btnRafraichir.Click += (s, e) => ChargerMedicaments();

            dgvMedicaments = new DataGridView();
            dgvMedicaments.Location = new Point(20, 70);
            dgvMedicaments.Size = new Size(900, 450);
            dgvMedicaments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(btnRafraichir);
            panel.Controls.Add(dgvMedicaments);

            page.Controls.Add(panel);

            ChargerMedicaments();

            return page;
        }

        private void ChargerMedicaments()
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                var medicaments = db.Query<Medicament>("SELECT * FROM Medicaments ORDER BY DateExpiration");
                dgvMedicaments.DataSource = medicaments.ToList();
            }
        }

        // ==================== ONGLET 4 : RECHERCHE AVANCÉE ====================
        private TextBox txtRechercheAnimal;
        private TextBox txtRechercheProprietaire;
        private DateTimePicker dtpRechercheDebut;
        private DateTimePicker dtpRechercheFin;
        private DataGridView dgvResultats;

        private TabPage CreerOngletRecherche()
        {
            TabPage page = new TabPage("🔍 Recherche avancée");

            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(20);

            Label lblTitle = new Label();
            lblTitle.Text = "Recherche de consultations";
            lblTitle.Font = new Font("Arial", 14, FontStyle.Bold);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Size = new Size(250, 30);

            // Critères de recherche
            Label lblAnimal = new Label() { Text = "Nom animal:", Location = new Point(20, 70), Size = new Size(100, 25) };
            txtRechercheAnimal = new TextBox() { Location = new Point(130, 70), Size = new Size(150, 25) };

            Label lblProprietaire = new Label() { Text = "Propriétaire:", Location = new Point(300, 70), Size = new Size(100, 25) };
            txtRechercheProprietaire = new TextBox() { Location = new Point(410, 70), Size = new Size(150, 25) };

            Label lblDateDebut = new Label() { Text = "Date début:", Location = new Point(580, 70), Size = new Size(80, 25) };
            dtpRechercheDebut = new DateTimePicker() { Location = new Point(670, 70), Size = new Size(120, 25) };
            dtpRechercheDebut.Value = DateTime.Now.AddMonths(-1);

            Label lblDateFin = new Label() { Text = "Date fin:", Location = new Point(580, 105), Size = new Size(80, 25) };
            dtpRechercheFin = new DateTimePicker() { Location = new Point(670, 105), Size = new Size(120, 25) };
            dtpRechercheFin.Value = DateTime.Now;

            Button btnRechercher = new Button();
            btnRechercher.Text = "🔍 Rechercher";
            btnRechercher.BackColor = Color.SteelBlue;
            btnRechercher.ForeColor = Color.White;
            btnRechercher.FlatStyle = FlatStyle.Flat;
            btnRechercher.Location = new Point(810, 70);
            btnRechercher.Size = new Size(110, 60);
            btnRechercher.Click += BtnRechercher_Click;

            dgvResultats = new DataGridView();
            dgvResultats.Location = new Point(20, 150);
            dgvResultats.Size = new Size(900, 350);
            dgvResultats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblAnimal);
            panel.Controls.Add(txtRechercheAnimal);
            panel.Controls.Add(lblProprietaire);
            panel.Controls.Add(txtRechercheProprietaire);
            panel.Controls.Add(lblDateDebut);
            panel.Controls.Add(dtpRechercheDebut);
            panel.Controls.Add(lblDateFin);
            panel.Controls.Add(dtpRechercheFin);
            panel.Controls.Add(btnRechercher);
            panel.Controls.Add(dgvResultats);

            page.Controls.Add(panel);
            return page;
        }

        private void BtnRechercher_Click(object sender, EventArgs e)
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                string sql = @"
                    SELECT c.*, a.Nom as NomAnimal 
                    FROM Consultations c
                    JOIN Animaux a ON c.AnimalId = a.Id
                    JOIN Proprietaires p ON a.ProprietaireId = p.Id
                    WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(txtRechercheAnimal.Text))
                    sql += " AND a.Nom LIKE @AnimalNom";
                if (!string.IsNullOrWhiteSpace(txtRechercheProprietaire.Text))
                    sql += " AND (p.Nom LIKE @PropNom OR p.Prenom LIKE @PropNom)";

                sql += " AND c.DateHeure BETWEEN @DateDebut AND @DateFin";
                sql += " ORDER BY c.DateHeure DESC";

                var resultats = db.Query<Consultation>(sql, new
                {
                    AnimalNom = $"%{txtRechercheAnimal.Text}%",
                    PropNom = $"%{txtRechercheProprietaire.Text}%",
                    DateDebut = dtpRechercheDebut.Value.Date,
                    DateFin = dtpRechercheFin.Value.Date.AddDays(1)
                });

                dgvResultats.DataSource = resultats.ToList();
            }
        }
    }
}