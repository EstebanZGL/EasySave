using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace CryptoSoft
{
    // Main GUI for CryptoSoft
    public class CryptoSoftForm : Form
    {
        private TextBox txtSourcePath = null!;
        private TextBox txtTargetPath = null!;
        private Button btnBrowseSource = null!;
        private Button btnBrowseTarget = null!;
        private Button btnEncrypt = null!;
        private Button btnDecrypt = null!;
        private Label lblStatus = null!;
        private ProgressBar progressBar = null!;
        private Label lblProgress = null!;
        private Label lblPathType = null!;
        private Label lblPassword = null!;
        private TextBox txtPassword = null!;
        private Button btnSavePassword = null!;

        private int totalFiles = 0;
        private int processedFiles = 0;
        private bool isSourceFolder = false;

        private const string SUFFIX_ENCRYPTED = "_encrypted";
        private const string SUFFIX_DECRYPTED = "_decrypted";

        public CryptoSoftForm()
        {
            InitializeComponent();
            this.Text = "CryptoSoft v4.0";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Shield;
            
            this.txtPassword.Text = Program.LoadEncryptionKey();
        }

        private void InitializeComponent()
        {
            // Form size
            this.ClientSize = new Size(500, 280);

            // Source path controls
            Label lblSource = new Label
            {
                Text = "Source path:",
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
                Text = "Browse",
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
                Text = "Target path:",
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
                Text = "Browse",
                Location = new Point(410, 68),
                Width = 80
            };
            btnBrowseTarget.Click += BtnBrowseTarget_Click;
            this.Controls.Add(btnBrowseTarget);

            // Password controls
            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(10, 110),
                AutoSize = true
            };
            this.Controls.Add(lblPassword);

            txtPassword = new TextBox
            {
                Location = new Point(120, 110),
                Width = 280,
                UseSystemPasswordChar = true
            };
            this.Controls.Add(txtPassword);

            btnSavePassword = new Button
            {
                Text = "Save",
                Location = new Point(410, 108),
                Width = 80
            };
            btnSavePassword.Click += BtnSavePassword_Click;
            this.Controls.Add(btnSavePassword);

            // Action buttons
            btnEncrypt = new Button
            {
                Text = "Encrypt",
                Location = new Point(120, 150),
                Width = 120,
                Height = 30
            };
            btnEncrypt.Click += BtnEncrypt_Click;
            this.Controls.Add(btnEncrypt);

            btnDecrypt = new Button
            {
                Text = "Decrypt",
                Location = new Point(260, 150),
                Width = 120,
                Height = 30
            };
            btnDecrypt.Click += BtnDecrypt_Click;
            this.Controls.Add(btnDecrypt);

            // Status
            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(10, 200),
                AutoSize = true
            };
            this.Controls.Add(lblStatus);

            // Progress label
            lblProgress = new Label
            {
                Text = "",
                Location = new Point(10, 220),
                AutoSize = true
            };
            this.Controls.Add(lblProgress);

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(120, 200),
                Width = 370,
                Height = 20,
                Visible = false
            };
            this.Controls.Add(progressBar);
        }

        private void BtnSavePassword_Click(object? sender, EventArgs e)
        {
            string newPassword = txtPassword.Text.Trim();
            
            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Password cannot be empty.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            Program.SaveEncryptionKey(newPassword);
            MessageBox.Show("Password saved successfully.", "Success", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Detects if source path is a file or folder and updates the UI
        private void TxtSourcePath_TextChanged(object? sender, EventArgs e)
        {
            string path = txtSourcePath.Text.Trim();
            
            if (string.IsNullOrEmpty(path))
            {
                lblPathType.Text = "";
                isSourceFolder = false;
                return;
            }

            if (Directory.Exists(path))
            {
                lblPathType.Text = "Folder detected";
                isSourceFolder = true;
                
                string parentDir = Directory.GetParent(path)?.FullName ?? "";
                string folderName = new DirectoryInfo(path).Name;
                
                bool seemsEncrypted = folderName.EndsWith(SUFFIX_ENCRYPTED, StringComparison.OrdinalIgnoreCase);
                
                if (seemsEncrypted)
                {
                    string baseFolderName = folderName.Substring(0, folderName.Length - SUFFIX_ENCRYPTED.Length);
                    txtTargetPath.Text = Path.Combine(parentDir, baseFolderName + SUFFIX_DECRYPTED);
                }
                else
                {
                    txtTargetPath.Text = Path.Combine(parentDir, folderName + SUFFIX_ENCRYPTED);
                }
            }
            else if (File.Exists(path))
            {
                lblPathType.Text = "File detected";
                isSourceFolder = false;
                
                string fileName = Path.GetFileNameWithoutExtension(path);
                
                bool seemsEncrypted = fileName.EndsWith(SUFFIX_ENCRYPTED, StringComparison.OrdinalIgnoreCase);
                
                if (seemsEncrypted)
                {
                    SuggestTargetPath(path, false);
                }
                else
                {
                    SuggestTargetPath(path, true);
                }
            }
            else
            {
                lblPathType.Text = "Invalid path";
                isSourceFolder = false;
            }
        }

        private void BtnBrowseSource_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog fileDialog = new OpenFileDialog
            {
                Title = "Select a file or folder",
                Filter = "All files (*.*)|*.*",
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = false,
                FileName = "Select folder"
            };

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = fileDialog.FileName;
                
                if (selectedPath.EndsWith("Select folder"))
                {
                    selectedPath = Path.GetDirectoryName(selectedPath) ?? "";
                }
                
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    txtSourcePath.Text = selectedPath;
                }
            }
        }

        private void BtnBrowseTarget_Click(object? sender, EventArgs e)
        {
            if (isSourceFolder)
            {
                using FolderBrowserDialog folderDialog = new FolderBrowserDialog
                {
                    Description = "Select target folder",
                    UseDescriptionForTitle = true
                };

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtTargetPath.Text = folderDialog.SelectedPath;
                }
            }
            else
            {
                using SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Select target file",
                    Filter = "All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtTargetPath.Text = saveFileDialog.FileName;
                }
            }
        }

        private async void BtnEncrypt_Click(object? sender, EventArgs e)
        {
            await ProcessPathAsync(true);
        }

        private async void BtnDecrypt_Click(object? sender, EventArgs e)
        {
            await ProcessPathAsync(false);
        }

        // Suggests target path based on source path
        private void SuggestTargetPath(string sourcePath, bool isEncrypt)
        {
            string directory = Path.GetDirectoryName(sourcePath) ?? "";
            string fileName = Path.GetFileNameWithoutExtension(sourcePath);
            string extension = Path.GetExtension(sourcePath);
            
            if (fileName.EndsWith(SUFFIX_ENCRYPTED, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(0, fileName.Length - SUFFIX_ENCRYPTED.Length);
            }
            else if (fileName.EndsWith(SUFFIX_DECRYPTED, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(0, fileName.Length - SUFFIX_DECRYPTED.Length);
            }
            
            string newFileName = fileName + (isEncrypt ? SUFFIX_ENCRYPTED : SUFFIX_DECRYPTED) + extension;
            txtTargetPath.Text = Path.Combine(directory, newFileName);
        }

        private async Task ProcessPathAsync(bool isEncrypt)
        {
            string sourcePath = txtSourcePath.Text.Trim();
            string targetPath = txtTargetPath.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(targetPath))
            {
                MessageBox.Show("Please specify source and target paths.", "Input Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isSourceFolder)
            {
                if (!Directory.Exists(sourcePath))
                {
                    MessageBox.Show("Source folder does not exist.", "Path Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                if (!File.Exists(sourcePath))
                {
                    MessageBox.Show("Source file does not exist.", "File Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            SetControlsEnabled(false);
            progressBar.Visible = true;
            
            try
            {
                if (isSourceFolder)
                {
                    await ProcessFolderAsync(sourcePath, targetPath, isEncrypt, password);
                }
                else
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                    lblStatus.Text = isEncrypt ? "Encrypting..." : "Decrypting...";
                    
                    string? targetDir = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }
                    
                    await Task.Run(() =>
                    {
                        Program.EncryptFile(sourcePath, targetPath, password);
                    });
                    
                    lblStatus.Text = isEncrypt ? "Encryption complete." : "Decryption complete.";
                    MessageBox.Show(isEncrypt ? "File encrypted successfully!" : "File decrypted successfully!", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsEnabled(true);
                progressBar.Visible = false;
                lblProgress.Text = "";
            }
        }

        // Processes an entire folder recursively
        private async Task ProcessFolderAsync(string sourceFolder, string targetFolder, bool isEncrypt, string password)
        {
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            totalFiles = 0;
            processedFiles = 0;
            CountFilesRecursively(sourceFolder);
            
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Minimum = 0;
            progressBar.Maximum = totalFiles;
            progressBar.Value = 0;
            
            lblStatus.Text = isEncrypt ? "Encrypting folder..." : "Decrypting folder...";
            
            await ProcessFilesRecursivelyAsync(sourceFolder, targetFolder, isEncrypt, password);
            
            lblStatus.Text = isEncrypt ? "Folder encryption complete." : "Folder decryption complete.";
            MessageBox.Show($"Processing complete! {processedFiles} files processed.", 
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Counts total files in a folder and its subfolders
        private void CountFilesRecursively(string folder)
        {
            try
            {
                totalFiles += Directory.GetFiles(folder).Length;
                
                foreach (string subDir in Directory.GetDirectories(folder))
                {
                    CountFilesRecursively(subDir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting files: {ex.Message}");
            }
        }

        // Processes all files in a folder and its subfolders
        private async Task ProcessFilesRecursivelyAsync(string sourceFolder, string targetFolder, bool isEncrypt, string password)
        {
            try
            {
                foreach (string sourceFile in Directory.GetFiles(sourceFolder))
                {
                    string fileName = Path.GetFileName(sourceFile);
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    
                    if (fileNameWithoutExt.EndsWith(SUFFIX_ENCRYPTED, StringComparison.OrdinalIgnoreCase))
                    {
                        fileNameWithoutExt = fileNameWithoutExt.Substring(0, fileNameWithoutExt.Length - SUFFIX_ENCRYPTED.Length);
                    }
                    else if (fileNameWithoutExt.EndsWith(SUFFIX_DECRYPTED, StringComparison.OrdinalIgnoreCase))
                    {
                        fileNameWithoutExt = fileNameWithoutExt.Substring(0, fileNameWithoutExt.Length - SUFFIX_DECRYPTED.Length);
                    }
                    
                    string newFileName = fileNameWithoutExt + (isEncrypt ? SUFFIX_ENCRYPTED : SUFFIX_DECRYPTED) + extension;
                    string targetFile = Path.Combine(targetFolder, newFileName);
                    
                    UpdateProgressUI(fileName);
                    
                    await Task.Run(() =>
                    {
                        Program.EncryptFile(sourceFile, targetFile, password);
                    });
                    
                    processedFiles++;
                    this.Invoke((MethodInvoker)delegate {
                        progressBar.Value = processedFiles;
                    });
                }
                
                foreach (string sourceSubDir in Directory.GetDirectories(sourceFolder))
                {
                    string subDirName = new DirectoryInfo(sourceSubDir).Name;
                    string targetSubDir = Path.Combine(targetFolder, subDirName);
                    
                    if (!Directory.Exists(targetSubDir))
                    {
                        Directory.CreateDirectory(targetSubDir);
                    }
                    
                    await ProcessFilesRecursivelyAsync(sourceSubDir, targetSubDir, isEncrypt, password);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing files: {ex.Message}");
                throw;
            }
        }

        // Updates the UI with progress
        private void UpdateProgressUI(string currentFile)
        {
            this.Invoke((MethodInvoker)delegate {
                lblProgress.Text = $"Processing: {currentFile} ({processedFiles + 1}/{totalFiles})";
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
            txtPassword.Enabled = enabled;
            btnSavePassword.Enabled = enabled;
        }
    }
}