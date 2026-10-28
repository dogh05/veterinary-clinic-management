using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using CliniqueVeterinaire.Helpers;
using CliniqueVeterinaire.Models;

namespace CliniqueVeterinaire.Forms
{
    public class FrmVeterinaire : Form
    {
        private TabControl tabControl;
        private Panel pnlHeader;

        // Contrôles Planning
        private DataGridView dgvPlanning;
        private DateTimePicker dtpDatePlanning;

        // Contrôles Consultations
        private ComboBox cboAnimalConsult;
        private TextBox txtDiagnostic;
        private TextBox txtTraitement;
        private TextBox txtRemarques;
        private Button btnEnregistrerConsult;
        private DataGridView dgvHistorique;

        // Contrôles Vaccinations
        private ComboBox cboAnimalVaccin;
        private TextBox txtNomVaccin;
        private DateTimePicker dtpDateVaccin;
        private DateTimePicker dtpDateRappel;
        private Button btnAjouterVaccin;
        private DataGridView dgvVaccinations;

        // Contrôles Médicaments
        private DataGridView dgvMedicaments;
        private TextBox txtRechercheMedoc;

        public FrmVeterinaire()
        {
            this.Text = "Vétérinaire - Clinique Vétérinaire";
            this.Size = new Size(1200, 750);
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
            lblWelcome.Text = $"Dr. {SessionUtilisateur.Current.Prenom} {SessionUtilisateur.Current.Nom} (Vétérinaire)";
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

            this.Resize += (s, e) => { btnLogout.Location = new Point(this.Width - 150, 18); };

            // TabControl
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            tabControl.TabPages.Add(CreerOngletPlanning());
            tabControl.TabPages.Add(CreerOngletConsultations());
            tabControl.TabPages.Add(CreerOngletVaccinations());
            tabControl.TabPages.Add(CreerOngletMedicaments());

            this.Controls.Add(tabControl);
            this.Controls.Add(pnlHeader);
        }

        // ==================== ONGLET 1 : PLANNING ====================
        private TabPage CreerOngletPlanning()
        {
            TabPage page = new TabPage("📅 Mon planning");
            Panel panel = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(20) };

            Label lblTitle = new Label() { Text = "Mes rendez-vous", Font = new Font("Arial", 14, FontStyle.Bold), Location = new Point(20, 20), Size = new Size(300, 30) };

            Label lblDate = new Label() { Text = "Date:", Location = new Point(20, 60), Size = new Size(50, 25) };
            dtpDatePlanning = new DateTimePicker() { Location = new Point(80, 60), Size = new Size(200, 25), Format = DateTimePickerFormat.Short };
            dtpDatePlanning.ValueChanged += (s, e) => ChargerPlanning();

            Button btnRafraichir = new Button() { Text = "🔄 Rafraîchir", Location = new Point(300, 60), Size = new Size(100, 25) };
            btnRafraichir.Click += (s, e) => ChargerPlanning();

            dgvPlanning = new DataGridView() { Location = new Point(20, 100), Size = new Size(1100, 500), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
            dgvPlanning.DoubleClick += (s, e) => DemarrerConsultation();

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblDate);
            panel.Controls.Add(dtpDatePlanning);
            panel.Controls.Add(btnRafraichir);
            panel.Controls.Add(dgvPlanning);

            page.Controls.Add(panel);
            ChargerPlanning();
            return page;
        }

        private void ChargerPlanning()
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                string sql = @"
                    SELECT r.Id, r.DateHeure, r.Motif, r.Statut, 
                           a.Nom as NomAnimal, a.Espece, p.Nom + ' ' + p.Prenom as NomProprietaire
                    FROM RendezVous r
                    JOIN Animaux a ON r.AnimalId = a.Id
                    JOIN Proprietaires p ON a.ProprietaireId = p.Id
                    WHERE r.VeterinaireId = @VetoId AND CAST(r.DateHeure AS DATE) = @Date
                    ORDER BY r.DateHeure";

                var result = db.Query(sql, new { VetoId = SessionUtilisateur.Current.Id, Date = dtpDatePlanning.Value.Date });
                dgvPlanning.DataSource = result.ToList();

                // Colorer les lignes selon le statut
                foreach (DataGridViewRow row in dgvPlanning.Rows)
                {
                    if (row.Cells["Statut"].Value?.ToString() == "Annulé")
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                }
            }
        }

        private void DemarrerConsultation()
        {
            if (dgvPlanning.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un rendez-vous pour démarrer la consultation.");
                return;
            }
            dynamic rdv = dgvPlanning.SelectedRows[0].DataBoundItem;
            if (rdv.Statut == "Annulé")
            {
                MessageBox.Show("Ce rendez-vous est annulé.");
                return;
            }

            // Aller à l'onglet Consultations et pré-remplir
            tabControl.SelectedIndex = 1;
            ChargerConsultation((int)rdv.Id, (int)rdv.AnimalId, rdv.NomAnimal);
        }

        // ==================== ONGLET 2 : CONSULTATIONS ====================
        private TabPage CreerOngletConsultations()
        {
            TabPage page = new TabPage("🩺 Consultations");
            Panel panel = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Section gauche : Nouvelle consultation
            GroupBox gbNouvelle = new GroupBox() { Text = "Nouvelle consultation", Location = new Point(20, 20), Size = new Size(500, 320) };

            Label lblAnimal = new Label() { Text = "Animal:", Location = new Point(20, 40), Size = new Size(80, 25) };
            cboAnimalConsult = new ComboBox() { Location = new Point(120, 40), Size = new Size(350, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblDiagnostic = new Label() { Text = "Diagnostic:", Location = new Point(20, 80), Size = new Size(80, 25) };
            txtDiagnostic = new TextBox() { Location = new Point(120, 80), Size = new Size(350, 60), Multiline = true };

            Label lblTraitement = new Label() { Text = "Traitement:", Location = new Point(20, 155), Size = new Size(80, 25) };
            txtTraitement = new TextBox() { Location = new Point(120, 155), Size = new Size(350, 60), Multiline = true };

            Label lblRemarques = new Label() { Text = "Remarques:", Location = new Point(20, 230), Size = new Size(80, 25) };
            txtRemarques = new TextBox() { Location = new Point(120, 230), Size = new Size(350, 60), Multiline = true };

            btnEnregistrerConsult = new Button() { Text = "💾 Enregistrer la consultation", Location = new Point(120, 305), Size = new Size(200, 40), BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnEnregistrerConsult.Click += BtnEnregistrerConsult_Click;

            gbNouvelle.Controls.Add(lblAnimal);
            gbNouvelle.Controls.Add(cboAnimalConsult);
            gbNouvelle.Controls.Add(lblDiagnostic);
            gbNouvelle.Controls.Add(txtDiagnostic);
            gbNouvelle.Controls.Add(lblTraitement);
            gbNouvelle.Controls.Add(txtTraitement);
            gbNouvelle.Controls.Add(lblRemarques);
            gbNouvelle.Controls.Add(txtRemarques);
            gbNouvelle.Controls.Add(btnEnregistrerConsult);

            // Section droite : Historique médical
            GroupBox gbHistorique = new GroupBox() { Text = "Historique médical", Location = new Point(540, 20), Size = new Size(570, 600) };

            Label lblSelectAnimal = new Label() { Text = "Sélectionner un animal:", Location = new Point(20, 30), Size = new Size(140, 25) };
            ComboBox cboHistoriqueAnimal = new ComboBox() { Location = new Point(170, 30), Size = new Size(370, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboHistoriqueAnimal.SelectedIndexChanged += (s, e) => ChargerHistorique((int)cboHistoriqueAnimal.SelectedValue);

            dgvHistorique = new DataGridView() { Location = new Point(20, 70), Size = new Size(530, 510), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            gbHistorique.Controls.Add(lblSelectAnimal);
            gbHistorique.Controls.Add(cboHistoriqueAnimal);
            gbHistorique.Controls.Add(dgvHistorique);

            panel.Controls.Add(gbNouvelle);
            panel.Controls.Add(gbHistorique);

            page.Controls.Add(panel);
            ChargerComboAnimaux();
            ChargerComboHistorique(cboHistoriqueAnimal);
            return page;
        }

        private void ChargerComboAnimaux()
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                var animaux = db.Query("SELECT a.Id, a.Nom, p.Nom + ' ' + p.Prenom as PropNom FROM Animaux a JOIN Proprietaires p ON a.ProprietaireId = p.Id ORDER BY a.Nom");
                cboAnimalConsult.DataSource = animaux.Select(a => new { Id = (int)a.Id, NomComplet = $"{a.Nom} (propriétaire: {a.PropNom})" }).ToList();
                cboAnimalConsult.DisplayMember = "NomComplet";
                cboAnimalConsult.ValueMember = "Id";
            }
        }

        private void ChargerComboHistorique(ComboBox cbo)
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                var animaux = db.Query("SELECT a.Id, a.Nom, p.Nom + ' ' + p.Prenom as PropNom FROM Animaux a JOIN Proprietaires p ON a.ProprietaireId = p.Id ORDER BY a.Nom");
                cbo.DataSource = animaux.Select(a => new { Id = (int)a.Id, NomComplet = $"{a.Nom} (propriétaire: {a.PropNom})" }).ToList();
                cbo.DisplayMember = "NomComplet";
                cbo.ValueMember = "Id";
            }
        }

        private void ChargerConsultation(int rdvId, int animalId, string nomAnimal)
        {
            // Sélectionner l'animal dans le combo
            foreach (var item in cboAnimalConsult.Items)
            {
                var prop = item.GetType().GetProperty("Id")?.GetValue(item, null);
                if (prop != null && (int)prop == animalId)
                {
                    cboAnimalConsult.SelectedItem = item;
                    break;
                }
            }

            MessageBox.Show($"Consultation pour {nomAnimal}. Veuillez remplir les informations médicales.");
            txtDiagnostic.Focus();
        }

        private void BtnEnregistrerConsult_Click(object sender, EventArgs e)
        {
            if (cboAnimalConsult.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un animal.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDiagnostic.Text))
            {
                MessageBox.Show("Le diagnostic est obligatoire.");
                return;
            }

            using (var db = DatabaseHelper.GetConnection())
            {
                // Vérifier s'il y a un rendez-vous aujourd'hui pour cet animal
                var rdv = db.QueryFirstOrDefault("SELECT Id FROM RendezVous WHERE AnimalId = @AnimalId AND VeterinaireId = @VetoId AND CAST(DateHeure AS DATE) = @Date",
                    new { AnimalId = (int)cboAnimalConsult.SelectedValue, VetoId = SessionUtilisateur.Current.Id, Date = DateTime.Now.Date });

                int consultationId;

                if (rdv != null)
                {
                    consultationId = db.ExecuteScalar<int>(@"
                        INSERT INTO Consultations (DateHeure, Diagnostic, Traitement, Remarques, RendezVousId, AnimalId) 
                        VALUES (GETDATE(), @Diagnostic, @Traitement, @Remarques, @RdvId, @AnimalId);
                        SELECT CAST(SCOPE_IDENTITY() AS INT)",
                        new { Diagnostic = txtDiagnostic.Text, Traitement = txtTraitement.Text, Remarques = txtRemarques.Text, RdvId = rdv.Id, AnimalId = (int)cboAnimalConsult.SelectedValue });
                }
                else
                {
                    consultationId = db.ExecuteScalar<int>(@"
                        INSERT INTO Consultations (DateHeure, Diagnostic, Traitement, Remarques, AnimalId) 
                        VALUES (GETDATE(), @Diagnostic, @Traitement, @Remarques, @AnimalId);
                        SELECT CAST(SCOPE_IDENTITY() AS INT)",
                        new { Diagnostic = txtDiagnostic.Text, Traitement = txtTraitement.Text, Remarques = txtRemarques.Text, AnimalId = (int)cboAnimalConsult.SelectedValue });
                }

                MessageBox.Show("Consultation enregistrée avec succès !");

                // Vider les champs
                txtDiagnostic.Text = "";
                txtTraitement.Text = "";
                txtRemarques.Text = "";

                // Rafraîchir l'historique si l'animal est sélectionné
                var cboHistorique = FindControl<ComboBox>("cboHistoriqueAnimal");
                if (cboHistorique != null && cboHistorique.SelectedValue != null)
                    ChargerHistorique((int)cboHistorique.SelectedValue);
            }
        }

        private void ChargerHistorique(int animalId)
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                var historique = db.Query(@"
                    SELECT Id, DateHeure, Diagnostic, Traitement, Remarques 
                    FROM Consultations 
                    WHERE AnimalId = @AnimalId 
                    ORDER BY DateHeure DESC", new { AnimalId = animalId });
                dgvHistorique.DataSource = historique.ToList();
            }
        }

        // ==================== ONGLET 3 : VACCINATIONS ====================
        private TabPage CreerOngletVaccinations()
        {
            TabPage page = new TabPage("💉 Vaccinations");
            Panel panel = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Section gauche : Ajouter vaccination
            GroupBox gbAjout = new GroupBox() { Text = "Enregistrer une vaccination", Location = new Point(20, 20), Size = new Size(500, 320) };

            Label lblAnimalVaccin = new Label() { Text = "Animal:", Location = new Point(20, 40), Size = new Size(80, 25) };
            cboAnimalVaccin = new ComboBox() { Location = new Point(120, 40), Size = new Size(350, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblNomVaccin = new Label() { Text = "Vaccin:", Location = new Point(20, 80), Size = new Size(80, 25) };
            txtNomVaccin = new TextBox() { Location = new Point(120, 80), Size = new Size(350, 25) };

            Label lblDateVaccin = new Label() { Text = "Date administration:", Location = new Point(20, 120), Size = new Size(120, 25) };
            dtpDateVaccin = new DateTimePicker() { Location = new Point(150, 120), Size = new Size(150, 25), Format = DateTimePickerFormat.Short };

            Label lblDateRappel = new Label() { Text = "Date rappel:", Location = new Point(20, 160), Size = new Size(120, 25) };
            dtpDateRappel = new DateTimePicker() { Location = new Point(150, 160), Size = new Size(150, 25), Format = DateTimePickerFormat.Short };
            dtpDateRappel.Value = DateTime.Now.AddYears(1);

            btnAjouterVaccin = new Button() { Text = "💾 Enregistrer le vaccin", Location = new Point(120, 210), Size = new Size(220, 40), BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAjouterVaccin.Click += BtnAjouterVaccin_Click;

            gbAjout.Controls.Add(lblAnimalVaccin);
            gbAjout.Controls.Add(cboAnimalVaccin);
            gbAjout.Controls.Add(lblNomVaccin);
            gbAjout.Controls.Add(txtNomVaccin);
            gbAjout.Controls.Add(lblDateVaccin);
            gbAjout.Controls.Add(dtpDateVaccin);
            gbAjout.Controls.Add(lblDateRappel);
            gbAjout.Controls.Add(dtpDateRappel);
            gbAjout.Controls.Add(btnAjouterVaccin);

            // Section droite : Liste des vaccinations
            GroupBox gbListe = new GroupBox() { Text = "Historique des vaccinations", Location = new Point(540, 20), Size = new Size(570, 600) };

            Label lblFiltreVaccin = new Label() { Text = "Filtrer par animal:", Location = new Point(20, 30), Size = new Size(120, 25) };
            ComboBox cboFiltreVaccin = new ComboBox() { Location = new Point(150, 30), Size = new Size(390, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboFiltreVaccin.SelectedIndexChanged += (s, e) => ChargerVaccinations((int)cboFiltreVaccin.SelectedValue);

            dgvVaccinations = new DataGridView() { Location = new Point(20, 70), Size = new Size(530, 510), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            gbListe.Controls.Add(lblFiltreVaccin);
            gbListe.Controls.Add(cboFiltreVaccin);
            gbListe.Controls.Add(dgvVaccinations);

            panel.Controls.Add(gbAjout);
            panel.Controls.Add(gbListe);

            page.Controls.Add(panel);
            ChargerComboAnimauxVaccin();
            ChargerComboFiltreVaccin(cboFiltreVaccin);
            return page;
        }

        private void ChargerComboAnimauxVaccin()
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                var animaux = db.Query("SELECT a.Id, a.Nom, p.Nom + ' ' + p.Prenom as PropNom FROM Animaux a JOIN Proprietaires p ON a.ProprietaireId = p.Id ORDER BY a.Nom");
                cboAnimalVaccin.DataSource = animaux.Select(a => new { Id = (int)a.Id, NomComplet = $"{a.Nom} (propriétaire: {a.PropNom})" }).ToList();
                cboAnimalVaccin.DisplayMember = "NomComplet";
                cboAnimalVaccin.ValueMember = "Id";
            }
        }

        private void ChargerComboFiltreVaccin(ComboBox cbo)
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                var animaux = db.Query("SELECT a.Id, a.Nom, p.Nom + ' ' + p.Prenom as PropNom FROM Animaux a JOIN Proprietaires p ON a.ProprietaireId = p.Id ORDER BY a.Nom");
                var liste = animaux.Select(a => new { Id = (int)a.Id, NomComplet = $"{a.Nom} (propriétaire: {a.PropNom})" }).ToList();
                liste.Insert(0, new { Id = 0, NomComplet = "-- Tous les animaux --" });
                cbo.DataSource = liste;
                cbo.DisplayMember = "NomComplet";
                cbo.ValueMember = "Id";
            }
        }

        private void BtnAjouterVaccin_Click(object sender, EventArgs e)
        {
            if (cboAnimalVaccin.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un animal.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNomVaccin.Text))
            {
                MessageBox.Show("Veuillez saisir le nom du vaccin.");
                return;
            }

            using (var db = DatabaseHelper.GetConnection())
            {
                db.Execute(@"
                    INSERT INTO Vaccinations (NomVaccin, DateAdministration, DateRappel, AnimalId) 
                    VALUES (@NomVaccin, @DateAdmin, @DateRappel, @AnimalId)",
                    new { NomVaccin = txtNomVaccin.Text, DateAdmin = dtpDateVaccin.Value, DateRappel = dtpDateRappel.Value, AnimalId = (int)cboAnimalVaccin.SelectedValue });

                MessageBox.Show("Vaccination enregistrée avec succès !");
                txtNomVaccin.Text = "";
                dtpDateVaccin.Value = DateTime.Now;
                dtpDateRappel.Value = DateTime.Now.AddYears(1);

                // Rafraîchir la liste
                var cboFiltre = FindControl<ComboBox>("cboFiltreVaccin");
                if (cboFiltre != null && cboFiltre.SelectedValue != null)
                    ChargerVaccinations((int)cboFiltre.SelectedValue);
            }
        }

        private void ChargerVaccinations(int animalId)
        {
            using (var db = DatabaseHelper.GetConnection())
            {
                string sql = @"
                    SELECT v.*, a.Nom as NomAnimal 
                    FROM Vaccinations v
                    JOIN Animaux a ON v.AnimalId = a.Id";

                if (animalId > 0)
                    sql += " WHERE v.AnimalId = @AnimalId";

                sql += " ORDER BY v.DateAdministration DESC";

                var result = db.Query(sql, new { AnimalId = animalId });
                dgvVaccinations.DataSource = result.ToList();

                // Colorer les rappels proches/expirés
                foreach (DataGridViewRow row in dgvVaccinations.Rows)
                {
                    if (row.Cells["DateRappel"].Value != null)
                    {
                        DateTime dateRappel = (DateTime)row.Cells["DateRappel"].Value;
                        if (dateRappel < DateTime.Now)
                            row.DefaultCellStyle.BackColor = Color.LightCoral;
                        else if (dateRappel < DateTime.Now.AddMonths(1))
                            row.DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                }
            }
        }

        // ==================== ONGLET 4 : MÉDICAMENTS ====================
        private TabPage CreerOngletMedicaments()
        {
            TabPage page = new TabPage("💊 Médicaments");
            Panel panel = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(20) };

            Label lblTitle = new Label() { Text = "Stock de médicaments", Font = new Font("Arial", 14, FontStyle.Bold), Location = new Point(20, 20), Size = new Size(300, 30) };

            Label lblRecherche = new Label() { Text = "Rechercher:", Location = new Point(20, 60), Size = new Size(80, 25) };
            txtRechercheMedoc = new TextBox() { Location = new Point(110, 60), Size = new Size(250, 25) };
            txtRechercheMedoc.TextChanged += (s, e) => ChargerMedicaments();

            Button btnRafraichir = new Button() { Text = "🔄 Rafraîchir", Location = new Point(380, 60), Size = new Size(100, 25) };
            btnRafraichir.Click += (s, e) => ChargerMedicaments();

            dgvMedicaments = new DataGridView() { Location = new Point(20, 100), Size = new Size(1100, 550), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblRecherche);
            panel.Controls.Add(txtRechercheMedoc);
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
                string sql = "SELECT * FROM Medicaments";
                if (!string.IsNullOrWhiteSpace(txtRechercheMedoc.Text))
                    sql += " WHERE NomCommercial LIKE @Search";
                sql += " ORDER BY DateExpiration";

                var result = db.Query(sql, new { Search = $"%{txtRechercheMedoc.Text}%" });
                dgvMedicaments.DataSource = result.ToList();

                // Colorer les médicaments
                foreach (DataGridViewRow row in dgvMedicaments.Rows)
                {
                    if (row.Cells["DateExpiration"].Value != null)
                    {
                        DateTime dateExp = (DateTime)row.Cells["DateExpiration"].Value;
                        if (dateExp < DateTime.Now)
                            row.DefaultCellStyle.BackColor = Color.LightCoral;
                        else if (dateExp < DateTime.Now.AddMonths(3))
                            row.DefaultCellStyle.BackColor = Color.LightYellow;
                    }

                    if (row.Cells["QuantiteStock"].Value != null && (int)row.Cells["QuantiteStock"].Value < 10)
                    {
                        if (row.DefaultCellStyle.BackColor != Color.LightCoral)
                            row.DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                }
            }
        }

        private T FindControl<T>(string name) where T : Control
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is TabControl tc)
                {
                    foreach (TabPage page in tc.TabPages)
                    {
                        T result = FindControlRecursive<T>(page, name);
                        if (result != null) return result;
                    }
                }
            }
            return null;
        }

        private T FindControlRecursive<T>(Control parent, string name) where T : Control
        {
            foreach (Control child in parent.Controls)
            {
                if (child.Name == name && child is T)
                    return (T)child;
                T result = FindControlRecursive<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}