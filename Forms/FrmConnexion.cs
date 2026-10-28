using System;
using System.Drawing;
using System.Windows.Forms;
using Dapper;
using CliniqueVeterinaire.Helpers;
using CliniqueVeterinaire.Models;

namespace CliniqueVeterinaire.Forms
{
    public class FrmConnexion : Form
    {
        private TextBox txtEmail;
        private TextBox txtMotDePasse;
        private Button btnConnexion;

        public FrmConnexion()
        {
            this.Text = "Connexion Clinique Vétérinaire";
            this.Size = new Size(450, 250);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Titre
            Label lblTitre = new Label();
            lblTitre.Text = "Clinique Vétérinaire";
            lblTitre.Font = new Font("Arial", 14, FontStyle.Bold);
            lblTitre.Location = new Point(130, 20);
            lblTitre.Size = new Size(200, 30);

            // Email
            Label lblEmail = new Label();
            lblEmail.Text = "Email:";
            lblEmail.Location = new Point(50, 70);
            lblEmail.Size = new Size(80, 25);

            txtEmail = new TextBox();
            txtEmail.Location = new Point(140, 70);
            txtEmail.Size = new Size(200, 25);

            // Mot de passe
            Label lblMdp = new Label();
            lblMdp.Text = "Mot de passe:";
            lblMdp.Location = new Point(50, 110);
            lblMdp.Size = new Size(80, 25);

            txtMotDePasse = new TextBox();
            txtMotDePasse.Location = new Point(140, 110);
            txtMotDePasse.Size = new Size(200, 25);
            txtMotDePasse.PasswordChar = '*';

            // Bouton
            btnConnexion = new Button();
            btnConnexion.Text = "Se connecter";
            btnConnexion.Location = new Point(160, 160);
            btnConnexion.Size = new Size(100, 35);
            btnConnexion.Click += BtnConnexion_Click;

            // Ajouter tous les contrôles
            this.Controls.Add(lblTitre);
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblMdp);
            this.Controls.Add(txtMotDePasse);
            this.Controls.Add(btnConnexion);
        }

        private void BtnConnexion_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtMotDePasse.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs");
                return;
            }

            using (var db = DatabaseHelper.GetConnection())
            {
                var user = db.QueryFirstOrDefault<Utilisateur>(
                    "SELECT * FROM Utilisateurs WHERE Email = @Email",
                    new { Email = txtEmail.Text });

                if (user != null && PasswordHasher.Verify(txtMotDePasse.Text, user.MotDePasse))
                {
                    SessionUtilisateur.Current = user;
                    this.Hide();

                    if (user.Role == "Admin")
                    {
                        new FrmAdminDashboard().ShowDialog();
                    }
                    else if (user.Role == "Secretaire")
                    {
                        new FrmSecretaire().ShowDialog();
                    }
                    else if (user.Role == "Veterinaire")
                    {
                        new FrmVeterinaire().ShowDialog();
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Email ou mot de passe incorrect");
                }
            }
        }
    }
}
