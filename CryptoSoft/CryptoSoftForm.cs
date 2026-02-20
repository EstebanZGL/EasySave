using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace CryptoSoft
{
    /// <summary>
    /// Interface graphique principale pour CryptoSoft
    /// </summary>
    public class CryptoSoftForm : Form
    {
        private TextBox txtSourceFile;
        private TextBox txtTargetFile;
        private Button btnBrowseSource;
        private Button btnBrowseTarget;
        private Button btnEncrypt;
        private Button btnDecrypt;
        private Label lblStatus;
        private ProgressBar progressBar;

        public CryptoSoftForm()
        {
            InitializeComponent();
            this.Text = "CryptoSoft v4.0";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Shield; // Utilise une icône de bouclier pour symboliser la sécurité
        }

        private void InitializeComponent()
        {
            // Form size
            this.ClientSize = new Size(500, 200);

            // Source file controls
            Label lblSource = new Label
            {
                Text = "Fichier source:",
                Location = new Point(10, 20),
                AutoSize = true
            };
            this.Controls.Add(lblSource);

            txtSourceFile = new TextBox
            {
                Location = new Point(120, 20),
                Width = 280
            };
            this.Controls.Add(txtSourceFile);

            btnBrowseSource = new Button
            {
                Text = "Parcourir",
                Location = new Point(410, 18),
                Width = 80
            };
            btnBrowseSource.Click += BtnBrowseSource_Click;
            this.Controls.Add(btnBrowseSource);

            // Target file controls
            Label lblTarget = new Label
            {
                Text = "Fichier cible:",
                Location = new Point(10, 50),
                AutoSize = true
            };
            this.Controls.Add(lblTarget);

            txtTargetFile = new TextBox
            {
                Location = new Point(120, 50),
                Width = 280
            };
            this.Controls.Add(txtTargetFile);

            btnBrowseTarget = new Button
            {
                Text = "Parcourir",
                Location = new Point(410, 48),
                Width = 80
            };
            btnBrowseTarget.Click += BtnBrowseTarget_Click;
            this.Controls.Add(btnBrowseTarget);

            // Action buttons
            btnEncrypt = new Button
            {
                Text = "Chiffrer",
                Location = new Point(120, 90),
                Width = 120,
                Height = 30
            };
            btnEncrypt.Click += BtnEncrypt_Click;
            this.Controls.Add(btnEncrypt);

            btnDecrypt = new Button
            {
                Text = "Déchiffrer",
                Location = new Point(260, 90),
                Width = 120,
                Height = 30
            };
            btnDecrypt.Click += BtnDecrypt_Click;
            this.Controls.Add(btnDecrypt);

            // Status
            lblStatus = new Label
            {
                Text = "Prêt",
                Location = new Point(10, 140),
                AutoSize = true
            };
            this.Controls.Add(lblStatus);

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(120, 140),
                Width = 370,
                Height = 20,
                Visible = false
            };
            this.Controls.Add(progressBar);
        }

        private void BtnBrowseSource_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Sélectionner le fichier source",
                Filter = "Tous les fichiers (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtSourceFile.Text = openFileDialog.FileName;
                
                // Auto-suggest target file name if empty
                if (string.IsNullOrWhiteSpace(txtTargetFile.Text))
                {
                    string sourceFile = openFileDialog.FileName;
                    string directory = Path.GetDirectoryName(sourceFile) ?? "";
                    string fileName = Path.GetFileNameWithoutExtension(sourceFile);
                    string extension = Path.GetExtension(sourceFile);
                    
                    // Pour le chiffrement, suggérer un nom avec "_chiffré"
                    txtTargetFile.Text = Path.Combine(directory, fileName + "_chiffré" + extension);
                }
            }
        }

        private void BtnBrowseTarget_Click(object sender, EventArgs e)
        {
            using SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Sélectionner le fichier cible",
                Filter = "Tous les fichiers (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtTargetFile.Text = saveFileDialog.FileName;
            }
        }

        private async void BtnEncrypt_Click(object sender, EventArgs e)
        {
            await ProcessFileAsync(true);
        }

        private async void BtnDecrypt_Click(object sender, EventArgs e)
        {
            await ProcessFileAsync(false);
        }

        private async Task ProcessFileAsync(bool isEncrypt)
        {
            string sourceFile = txtSourceFile.Text.Trim();
            string targetFile = txtTargetFile.Text.Trim();

            if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(targetFile))
            {
                MessageBox.Show("Veuillez spécifier les fichiers source et cible.", "Erreur de saisie", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(sourceFile))
            {
                MessageBox.Show("Le fichier source n'existe pas.", "Erreur de fichier", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Vérifier si le fichier cible existe déjà
            if (File.Exists(targetFile))
            {
                DialogResult result = MessageBox.Show(
                    "Le fichier cible existe déjà. Voulez-vous le remplacer ?",
                    "Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            // Disable controls during operation
            SetControlsEnabled(false);
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;
            lblStatus.Text = isEncrypt ? "Chiffrement en cours..." : "Déchiffrement en cours...";

            try
            {
                // Process file on a background thread
                await Task.Run(() =>
                {
                    // XOR encryption is symmetric, so we use the same method for both encrypt and decrypt
                    Program.EncryptFile(sourceFile, targetFile);
                });

                lblStatus.Text = isEncrypt ? "Chiffrement terminé." : "Déchiffrement terminé.";
                MessageBox.Show(isEncrypt ? "Fichier chiffré avec succès !" : "Fichier déchiffré avec succès !", 
                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erreur: " + ex.Message;
                MessageBox.Show($"Une erreur s'est produite: {ex.Message}", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable controls
                SetControlsEnabled(true);
                progressBar.Visible = false;
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            txtSourceFile.Enabled = enabled;
            txtTargetFile.Enabled = enabled;
            btnBrowseSource.Enabled = enabled;
            btnBrowseTarget.Enabled = enabled;
            btnEncrypt.Enabled = enabled;
            btnDecrypt.Enabled = enabled;
        }
    }
}