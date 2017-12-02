using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using HedgeLib.IO;
using System.Text;

namespace SonicLightEditor
{
    public partial class SonicLightEditor : Form
    {
        // Constructors
        public SonicLightEditor()
        {
            InitializeComponent();
        }

        // GUI Events
        private void SonicLightEditor_Load(object sender, EventArgs e)
        {
            RefreshUI(false, false);
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Title = "Open Light...",
                Filter = "Lights (*.light)|*.light"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadLight(ofd.FileName);
                LightName.Text = $"Light Name:\n{Path.GetFileName(ofd.FileName)}";
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog()
            {
                Title = "Save Light...",
                Filter = "Lights (*.light)|*.light"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SaveLight(sfd.FileName);
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Sonic Unleashed/Gens/LW/Forces .light edtior.");
            builder.AppendLine("Made by SWS90 with help from Radfordhound.");
            builder.AppendLine("Reads and writes the following for .light files:");
            builder.AppendLine("Directional: X,Y,Z Position, and RGB Color.");
            builder.AppendLine("Omni: X,Y,Z Position, RGB Color, Inner and Outer Range.");

            MessageBox.Show(builder.ToString(), "About Sonic Light Editor...", MessageBoxButtons.OK);
        }

        private void DarkThemeToggle_CheckedChanged(object sender, EventArgs e)
        {
            ToggleDarkTheme(DarkThemeToggle.Checked);
        }

        private void TxtBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            var txtBx = sender as TextBox;
            if (txtBx == null) return;

            // If the pressed key is enter, stop typing
            if (e.KeyChar == (char)Keys.Return)
            {
                ActiveControl = null;
                e.Handled = true;
                return;
            }

            // If the pressed key isn't a control key, digit, or
            // the first decimal point, don't accept it.
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                ((e.KeyChar != '.') || (txtBx.Text.IndexOf('.') > -1)))
            {
                e.Handled = true;
            }
        }

        private void TxtBox_Leave(object sender, EventArgs e)
        {
            var txtBx = sender as TextBox;
            if (txtBx == null) return;

            // When the text box looses focus, attempt to parse the text
            // as a float. If the text cannot be parsed, it's not a valid
            // number, so just set the text back to "0".
            txtBx.Text = (float.TryParse(txtBx.Text, out float f)) ?
                f.ToString() : "0";
        }

        // Methods
        public void RefreshUI(bool dirUIEnabled, bool omniUIEnabled)
        {
            // Enable/Disable Directional Light UI Elements
            TextBoxPosX.Enabled = TextBoxPosY.Enabled = TextBoxPosZ.Enabled =
            TextBoxColorR.Enabled = TextBoxColorG.Enabled = TextBoxColorB.Enabled =
                dirUIEnabled;

            // Enable/Disable Omni Light UI Elements
            TextBoxOmniLightInnerRange.Enabled = TextBoxOmniLightOuterRange.Enabled =
                omniUIEnabled;

            // Clear Omni TextBoxes if omni UI is disabled
            if (!omniUIEnabled)
            {
                TextBoxOmniLightInnerRange.Clear();
                TextBoxOmniLightOuterRange.Clear();
            }
        }

        public void ToggleDarkTheme(bool enabled)
        {
            // Set the color of all controls that use color 1
            AboutStripMenuItem.BackColor = ExitStripMenuItem.BackColor =
            SaveAsStripMenuItem.BackColor = OpenStripMenuItem.BackColor = (enabled) ? 
                Color.FromArgb(56, 56, 56) : SystemColors.Control;

            // Set the color of all controls that use color 2
            TopStrip.BackColor = HelpStripMenuItem.BackColor =
            FileStripMenuItem.BackColor = DarkThemeToggle.BackColor = (enabled) ?
                Color.FromArgb(59, 59, 59) : SystemColors.Control;

            // Set the color of all controls that use color 3
            BackColor = LightType.BackColor = LightName.BackColor =
            Label_PosX.BackColor = Label_PosY.BackColor = Label_PosZ.BackColor =
            Label_ColorR.BackColor = Label_ColorG.BackColor = Label_ColorB.BackColor =
            TextBoxPosX.BackColor = TextBoxPosY.BackColor = TextBoxPosZ.BackColor =
            TextBoxColorR.BackColor = TextBoxColorG.BackColor = TextBoxColorB.BackColor =
            Label_OmniLightInnerRange.BackColor = Label_OmniLightOuterRange.BackColor =
            TextBoxOmniLightOuterRange.BackColor = TextBoxOmniLightInnerRange.BackColor =
                (enabled) ? Color.FromArgb(64, 64, 64) : SystemColors.Control;

            // Set the color of all controls that use color 4
            ForeColor = TopStrip.ForeColor = DarkThemeToggle.ForeColor =
            FileStripMenuItem.ForeColor = OpenStripMenuItem.ForeColor =
            SaveAsStripMenuItem.ForeColor = ExitStripMenuItem.ForeColor =
            HelpStripMenuItem.ForeColor = AboutStripMenuItem.ForeColor =
            LightType.ForeColor = LightName.ForeColor = Label_PosX.ForeColor =
            Label_PosY.ForeColor = Label_PosZ.ForeColor = Label_ColorR.ForeColor =
            Label_ColorG.ForeColor = Label_ColorB.ForeColor = TextBoxPosX.ForeColor =
            TextBoxPosY.ForeColor = TextBoxPosZ.ForeColor = TextBoxColorR.ForeColor =
            TextBoxColorG.ForeColor = TextBoxColorB.ForeColor =
            Label_OmniLightInnerRange.ForeColor = Label_OmniLightOuterRange.ForeColor =
            TextBoxOmniLightInnerRange.ForeColor = TextBoxOmniLightOuterRange.ForeColor =
                (enabled) ? Color.FromArgb(164, 164, 164) : SystemColors.ControlText;
        }

        public void LoadLight(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                var reader = new ExtendedBinaryReader(fileStream, true);
                uint fileSize = reader.ReadUInt32();
                uint rootNodeType = reader.ReadUInt32();
                uint finalTableOffset = reader.ReadUInt32();
                uint rootNodeOffset = reader.ReadUInt32();
                uint finalTableOffsetAbs = reader.ReadUInt32();
                uint padding = reader.ReadUInt32();
                uint value_LightType = reader.ReadUInt32();
                float XPos = reader.ReadSingle();
                float YPos = reader.ReadSingle();
                float ZPos = reader.ReadSingle();
                float ColorR = reader.ReadSingle();
                float ColorG = reader.ReadSingle();
                float ColorB = reader.ReadSingle();

                // Read Omni-Specific Values
                bool isOmniLight = (value_LightType == 1);
                if (isOmniLight)
                {
                    uint Unknown1 = reader.ReadUInt32();
                    uint Unknown2 = reader.ReadUInt32();
                    uint Unknown3 = reader.ReadUInt32();
                    float OmniInnerRange = reader.ReadSingle();
                    float OmniOuterRange = reader.ReadSingle();

                    TextBoxOmniLightInnerRange.Text = OmniInnerRange.ToString();
                    TextBoxOmniLightOuterRange.Text = OmniOuterRange.ToString();
                }

                // Update UI Elements
                TextBoxPosX.Text = XPos.ToString();
                TextBoxPosY.Text = YPos.ToString();
                TextBoxPosZ.Text = ZPos.ToString();
                TextBoxColorR.Text = ColorR.ToString();
                TextBoxColorG.Text = ColorG.ToString();
                TextBoxColorB.Text = ColorB.ToString();

                LightType.Text = string.Format("Light Type: {0}",
                    (isOmniLight) ? "Omni" : "Directional");

                RefreshUI(true, isOmniLight);
            }
        }

        public void SaveLight(string filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                var writer = new ExtendedBinaryWriter(fileStream, true);

                // Write Header
                writer.Write(0);
                writer.Write(1);
                writer.Write(0);
                writer.Write(24);
                writer.Write(0);
                writer.Write(0);

                // Write Light Type
                bool isOmniLight = (LightType.Text == "Light Type: Omni");
                writer.Write((isOmniLight) ? 1 : 0);

                // Write Light XYZ Position and RGB Color
                writer.Write(float.Parse(TextBoxPosX.Text));
                writer.Write(float.Parse(TextBoxPosY.Text));
                writer.Write(float.Parse(TextBoxPosZ.Text));
                writer.Write(float.Parse(TextBoxColorR.Text));
                writer.Write(float.Parse(TextBoxColorG.Text));
                writer.Write(float.Parse(TextBoxColorB.Text));

                // Write Omni-Specific Values
                if (isOmniLight)
                {
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(float.Parse(TextBoxOmniLightInnerRange.Text));
                    writer.Write(float.Parse(TextBoxOmniLightOuterRange.Text));
                }

                // Write Offset Table
                writer.Write(0);

                // Fill-In Header Values
                fileStream.Position = 0;
                writer.Write((uint)fileStream.Length);

                uint finalTablePosition = (uint)fileStream.Length - 4;
                fileStream.Position = 8;
                writer.Write(finalTablePosition - 0x18);

                fileStream.Position = 16;
                writer.Write(finalTablePosition);
            }
        }
    }
}
