using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace CryptoSoft
{
    /// <summary>
    /// Interface graphique principale pour CryptoSoft
    /// </summary>
    public class CryptoSoftForm : Form
    {
        private TextBox txtSourcePath;
        private TextBox txtTargetPath;
        private Button btnBrowseSource;
        private Button btnBrowseTarget;
        private Button btnEncrypt;
        private Button btnDecrypt;
        private Label lblStatus;
        private ProgressBar progressBar;
        private Label lblProgress;
        private Label lblPathType;

        // Compteurs pour le traitement par lot
        private int totalFiles = 0;
        private int processedFiles = 0;
        
        // Indique si le chemin source est un dossier
        private bool isSourceFolder = false;

        // Constantes pour les suffixes
        private const string SUFFIX_ENCRYPTED = "_chiffre";
        private const string SUFFIX_DECRYPTED = "_dechiffre";

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
            this.ClientSize = new Size(500, 240);

            // Source path controls
            Label lblSource = new Label
            {
                Text = "Chemin source:",
                Location = new Point(10, 20),
                AutoSize = true
            };
            this.Controls.Add(lblSource);

            txtSourcePath = new TextBox
            {
                Location = new Point(120, 20),
                Width = 280
            };
            txtSourcePath.TextChanged += TxtSourcePath_TextChanged;
            this.Controls.Add(txtSourcePath);

            btnBrowseSource = new Button
            {
                Text = "Parcourir",
                Location = new Point(410, 18),
                Width = 80
            };
            btnBrowseSource.Click += BtnBrowseSource_Click;
            this.Controls.Add(btnBrowseSource);

            // Path type indicator
            lblPathType = new Label
            {
                Text = "",
                Location = new Point(120, 45),
                AutoSize = true,
                ForeColor = Color.Blue
            };
            this.Controls.Add(lblPathType);

            // Target path controls
            Label lblTarget = new Label
            {
                Text = "Chemin cible:",
                Location = new Point(10, 70),
                AutoSize = true
            };
            this.Controls.Add(lblTarget);

            txtTargetPath = new TextBox
            {
                Location = new Point(120, 70),
                Width = 280
            };
            this.Controls.Add(txtTargetPath);

            btnBrowseTarget = new Button
            {
                Text = "Parcourir",
                Location = new Point(410, 68),
                Width = 80
            };
            btnBrowseTarget.Click += BtnBrowseTarget_Click;
            this.Controls.Add(btnBrowseTarget);

            // Action buttons
            btnEncrypt = new Button
            {
                Text = "Chiffrer",
                Location = new Point(120, 110),
                Width = 120,
                Height = 30
            };
            btnEncrypt.Click += BtnEncrypt_Click;
            this.Controls.Add(btnEncrypt);

            btnDecrypt = new Button
            {
                Text = "Déchiffrer",
                Location = new Point(260, 110),
                Width = 120,
                Height = 30
            };
            btnDecrypt.Click += BtnDecrypt_Click;
            this.Controls.Add(btnDecrypt);

            // Status
            lblStatus = new Label
            {
                Text = "Prêt",
                Location = new Point(10, 160),
                AutoSize = true
            };
            this.Controls.Add(lblStatus);

            // Progress label
            lblProgress = new Label
            {
                Text = "",
                Location = new Point(10, 180),
                AutoSize = true
            };
            this.Controls.Add(lblProgress);

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(120, 160),
                Width = 370,
                Height = 20,
                Visible = false
            };
            this.Controls.Add(progressBar);
        }

        /// <summary>
        /// Détecte automatiquement si le chemin source est un fichier ou un dossier
        /// et met à jour l'interface en conséquence
        /// </summary>
        private void TxtSourcePath_TextChanged(object sender, EventArgs e)
        {
            string path = txtSourcePath.Text.Trim();
            
            if (string.IsNullOrEmpty(path))
            {
                lblPathType.Text = "";
                isSourceFolder = false;
                return;
            }

            // Détecter si c'est un fichier ou un dossier
            if (Directory.Exists(path))
            {
                lblPathType.Text = "Dossier détecté";
                isSourceFolder = true;
                
                // Suggérer un chemin cible pour le dossier
                string parentDir = Directory.GetParent(path)?.FullName ?? "";
                string folderName = new DirectoryInfo(path).Name;
                
                // Déterminer si le dossier semble être chiffré ou non
                bool seemsEncrypted = folderName.EndsWith(SUFFIX_ENCRYPTED, StringComparison.OrdinalIgnoreCase);
                
                if (seemsEncrypted)
                {
                    // Si le dossier semble chiffré, suggérer un dossier déchiffré
                    string baseFolderName = folderName.Substring(0, folderName.Length - SUFFIX_ENCRYPTED.Length);
                    txtTargetPath.Text = Path.Combine(parentDir, baseFolderName + SUFFIX_DECRYPTED);
                }
                else
                {
                    // Sinon, suggérer un dossier chiffré
                    txtTargetPath.Text = Path.Combine(parentDir, folderName + SUFFIX_ENCRYPTED);
                }
            }
            else if (File.Exists(path))
            {
                lblPathType.Text = "Fichier détecté";
                isSourceFolder = false;
                
                // Suggérer un chemin cible pour le fichier
                string fileName = Path.GetFileNameWithoutExtension(path);
                
                // Déterminer si le fichier semble être chiffré ou non
                bool seemsEncrypted = fileName.EndsWith(SUFFIX_ENCRYPTED, StringComparison.OrdinalIgnoreCase);
                
                if (seemsEncrypted)
                {
                    // Si le fichier semble chiffré, suggérer un déchiffrement
                    SuggestTargetPath(path, false);
                }
                else
                {
                    // Sinon, suggérer un chiffrement
                    SuggestTargetPath(path, true);
                }
            }
            else
            {
                lblPathType.Text = "Chemin invalide";
                isSourceFolder = false;
            }
        }

        private void BtnBrowseSource_Click(object sender, EventArgs e)
        {
            // Dialogue pour sélectionner un fichier ou un dossier
            using OpenFileDialog fileDialog = new OpenFileDialog
            {
                Title = "Sélectionner un fichier ou un dossier",
                Filter = "Tous les fichiers (*.*)|*.*",
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = false,
                FileName = "Sélectionner un dossier"
            };

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = fileDialog.FileName;
                
                // Si "Sélectionner un dossier" est choisi, extraire le chemin du dossier
                if (selectedPath.EndsWith("Sélectionner un dossier"))
                {
                    selectedPath = Path.GetDirectoryName(selectedPath) ?? "";
                }
                
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    txtSourcePath.Text = selectedPath;
                    // Le changement de texte va déclencher TxtSourcePath_TextChanged
                }
            }
        }

        private void BtnBrowseTarget_Click(object sender, EventArgs e)
        {
            if (isSourceFolder)
            {
                // Sélectionner un dossier cible
                using FolderBrowserDialog folderDialog = new FolderBrowserDialog
                {
                    Description = "Sélectionner le dossier cible",
                    UseDescriptionForTitle = true
                };

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtTargetPath.Text = folderDialog.SelectedPath;
                }
            }
            else
            {
                // Sélectionner un fichier cible
                using SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Sélectionner le fichier cible",
                    Filter = "Tous les fichiers (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtTargetPath.Text = saveFileDialog.FileName;
                }
            }
        }

        private async void BtnEncrypt_Click(object sender, EventArgs e)
        {
            // Force le mode chiffrement
            await ProcessPathAsync(true);
        }

        private async void BtnDecrypt_Click(object sender, EventArgs e)
        {
            // Force le mode déchiffrement
            await ProcessPathAsync(false);
        }

        /// <summary>
        /// Suggère un chemin cible basé sur le chemin source et l'opération
        /// </summary>
        /// <param name="sourcePath">Chemin source</param>
        /// <param name="isEncrypt">True pour chiffrement, False pour déchiffrement</param>
        private void SuggestTargetPath(string sourcePath, bool isEncrypt)
        {
            string directory = Path.GetDirectoryName(sourcePath) ?? "";
            string fileName = Path.GetFileNameWithoutExtension(sourcePath);
            string extension = Path.GetExtension(sourcePath);
            
            // Supprimer les suffixes existants si présents
            if (fileName.EndsWith(SUFFIX_ENCRYPTED, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(0, fileName.Length - SUFFIX_ENCRYPTED.Length);
            }
            else if (fileName.EndsWith(SUFFIX_DECRYPTED, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(0, fileName.Length - SUFFIX_DECRYPTED.Length);
            }
            
            // Ajouter le suffixe approprié
            string newFileName = fileName + (isEncrypt ? SUFFIX_ENCRYPTED : SUFFIX_DECRYPTED) + extension;
            txtTargetPath.Text = Path.Combine(directory, newFileName);
        }

        private async Task ProcessPathAsync(bool isEncrypt)
        {
            string sourcePath = txtSourcePath.Text.Trim();
            string targetPath = txtTargetPath.Text.Trim();

            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(targetPath))
            {
                MessageBox.Show("Veuillez spécifier les chemins source et cible.", "Erreur de saisie", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Vérifier si le chemin source existe
            if (isSourceFolder)
            {
                if (!Directory.Exists(sourcePath))
                {
                    MessageBox.Show("Le dossier source n'existe pas.", "Erreur de chemin", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                if (!File.Exists(sourcePath))
                {
                    MessageBox.Show("Le fichier source n'existe pas.", "Erreur de fichier", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Disable controls during operation
            SetControlsEnabled(false);
            progressBar.Visible = true;
            
            try
            {
                if (isSourceFolder)
                {
                    // Traitement de dossier
                    await ProcessFolderAsync(sourcePath, targetPath, isEncrypt);
                }
                else
                {
                    // Traitement de fichier unique
                    progressBar.Style = ProgressBarStyle.Marquee;
                    lblStatus.Text = isEncrypt ? "Chiffrement en cours..." : "Déchiffrement en cours...";
                    
                    // Créer le dossier cible si nécessaire
                    string? targetDir = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }
                    
                    await Task.Run(() =>
                    {
                        Program.EncryptFile(sourcePath, targetPath);
                    });
                    
                    lblStatus.Text = isEncrypt ? "Chiffrement terminé." : "Déchiffrement terminé.";
                    MessageBox.Show(isEncrypt ? "Fichier chiffré avec succès !" : "Fichier déchiffré avec succès !", 
                        "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
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
                lblProgress.Text = "";
            }
        }

        /// <summary>
        /// Traite un dossier entier de manière récursive
        /// </summary>
        private async Task ProcessFolderAsync(string sourceFolder, string targetFolder, bool isEncrypt)
        {
            // Créer le dossier cible s'il n'existe pas
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            // Compter le nombre total de fichiers à traiter
            totalFiles = 0;
            processedFiles = 0;
            CountFilesRecursively(sourceFolder);
            
            // Configurer la barre de progression
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Minimum = 0;
            progressBar.Maximum = totalFiles;
            progressBar.Value = 0;
            
            lblStatus.Text = isEncrypt ? "Chiffrement du dossier en cours..." : "Déchiffrement du dossier en cours...";
            
            // Traiter tous les fichiers de manière récursive
            await ProcessFilesRecursivelyAsync(sourceFolder, targetFolder, isEncrypt);
            
            lblStatus.Text = isEncrypt ? "Chiffrement du dossier terminé." : "Déchiffrement du dossier terminé.";
            MessageBox.Show($"Traitement terminé ! {processedFiles} fichiers traités.", 
                "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Compte le nombre total de fichiers dans un dossier et ses sous-dossiers
        /// </summary>
        private void CountFilesRecursively(string folder)
        {
            try
            {
                // Compter les fichiers dans le dossier actuel
                totalFiles += Directory.GetFiles(folder).Length;
                
                // Compter récursivement dans les sous-dossiers
                foreach (string subDir in Directory.GetDirectories(folder))
                {
                    CountFilesRecursively(subDir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du comptage des fichiers: {ex.Message}");
            }
        }

        /// <summary>
        /// Traite tous les fichiers d'un dossier et de ses sous-dossiers
        /// </summary>
        private async Task ProcessFilesRecursivelyAsync(string sourceFolder, string targetFolder, bool isEncrypt)
        {
            try
            {
                // Traiter tous les fichiers du dossier actuel
                foreach (string sourceFile in Directory.GetFiles(sourceFolder))
                {
                    // Obtenir juste le nom du fichier sans le chemin
                    string fileName = Path.GetFileName(sourceFile);
                    
                    // Générer le nom du fichier cible avec le suffixe approprié
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    
                    // Supprimer les suffixes existants si présents
                    if (fileNameWithoutExt.EndsWith(SUFFIX_ENCRYPTED, StringComparison.OrdinalIgnoreCase))
                    {
                        fileNameWithoutExt = fileNameWithoutExt.Substring(0, fileNameWithoutExt.Length - SUFFIX_ENCRYPTED.Length);
                    }
                    else if (fileNameWithoutExt.EndsWith(SUFFIX_DECRYPTED, StringComparison.OrdinalIgnoreCase))
                    {
                        fileNameWithoutExt = fileNameWithoutExt.Substring(0, fileNameWithoutExt.Length - SUFFIX_DECRYPTED.Length);
                    }
                    
                    // Créer le nouveau nom de fichier avec le suffixe
                    string newFileName = fileNameWithoutExt + (isEncrypt ? SUFFIX_ENCRYPTED : SUFFIX_DECRYPTED) + extension;
                    
                    // Chemin complet du fichier cible
                    string targetFile = Path.Combine(targetFolder, newFileName);
                    
                    // Mettre à jour l'interface utilisateur
                    UpdateProgressUI(fileName);
                    
                    // Traiter le fichier
                    await Task.Run(() =>
                    {
                        Program.EncryptFile(sourceFile, targetFile);
                    });
                    
                    // Incrémenter le compteur de fichiers traités
                    processedFiles++;
                    this.Invoke((MethodInvoker)delegate {
                        progressBar.Value = processedFiles;
                    });
                }
                
                // Traiter récursivement tous les sous-dossiers
                foreach (string sourceSubDir in Directory.GetDirectories(sourceFolder))
                {
                    string subDirName = new DirectoryInfo(sourceSubDir).Name;
                    string targetSubDir = Path.Combine(targetFolder, subDirName);
                    
                    // Créer le sous-dossier cible s'il n'existe pas
                    if (!Directory.Exists(targetSubDir))
                    {
                        Directory.CreateDirectory(targetSubDir);
                    }
                    
                    // Traiter le sous-dossier récursivement
                    await ProcessFilesRecursivelyAsync(sourceSubDir, targetSubDir, isEncrypt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du traitement des fichiers: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Met à jour l'interface utilisateur avec la progression
        /// </summary>
        private void UpdateProgressUI(string currentFile)
        {
            this.Invoke((MethodInvoker)delegate {
                lblProgress.Text = $"Traitement de: {currentFile} ({processedFiles + 1}/{totalFiles})";
            });
        }

        private void SetControlsEnabled(bool enabled)
        {
            txtSourcePath.Enabled = enabled;
            txtTargetPath.Enabled = enabled;
            btnBrowseSource.Enabled = enabled;
            btnBrowseTarget.Enabled = enabled;
            btnEncrypt.Enabled = enabled;
            btnDecrypt.Enabled = enabled;
        }
    }
}