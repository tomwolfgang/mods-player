using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using TPlayer.Model;
using TPlayer.Engine;
using System.IO;

namespace TPlayer
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnPlay;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.OpenFileDialog _openFileDialog;
		private System.Windows.Forms.Button btnIncreaseSpeed;
		private System.Windows.Forms.Button btnLowerSpeed;
		private System.Windows.Forms.Label lblSpeed;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ComboBox _comboBoxInterpolation;
		private System.Windows.Forms.TextBox txtBoxStatus;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ListView listViewSamples;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ListView listViewPatterns;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.Label lblBuffers;
		private System.Windows.Forms.TextBox txtBoxNumOfBuffers;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtBoxBufferSize;
		//private System.ComponentModel.IContainer components;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				//if (components != null) 
				//{
				//	components.Dispose();
				//}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.btnPlay = new System.Windows.Forms.Button();
      this.btnStop = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.btnBrowse = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this._openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.btnIncreaseSpeed = new System.Windows.Forms.Button();
      this.btnLowerSpeed = new System.Windows.Forms.Button();
      this.lblSpeed = new System.Windows.Forms.Label();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.checkBox2 = new System.Windows.Forms.CheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this._comboBoxInterpolation = new System.Windows.Forms.ComboBox();
      this.txtBoxStatus = new System.Windows.Forms.TextBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.m_patternView = new TPlayer.PatternView();
      this.listViewSamples = new System.Windows.Forms.ListView();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.listViewPatterns = new System.Windows.Forms.ListView();
      this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.lblBuffers = new System.Windows.Forms.Label();
      this.txtBoxNumOfBuffers = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.txtBoxBufferSize = new System.Windows.Forms.TextBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnPlay
      // 
      this.btnPlay.Location = new System.Drawing.Point(10, 83);
      this.btnPlay.Name = "btnPlay";
      this.btnPlay.Size = new System.Drawing.Size(67, 28);
      this.btnPlay.TabIndex = 0;
      this.btnPlay.Text = "Play";
      this.btnPlay.Click += new System.EventHandler(this.button1_Click);
      // 
      // btnStop
      // 
      this.btnStop.Location = new System.Drawing.Point(86, 83);
      this.btnStop.Name = "btnStop";
      this.btnStop.Size = new System.Drawing.Size(68, 28);
      this.btnStop.TabIndex = 1;
      this.btnStop.Text = "Stop";
      this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.btnBrowse);
      this.groupBox1.Controls.Add(this.textBox1);
      this.groupBox1.Location = new System.Drawing.Point(10, 9);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(326, 65);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Filename:";
      // 
      // btnBrowse
      // 
      this.btnBrowse.Location = new System.Drawing.Point(240, 26);
      this.btnBrowse.Name = "btnBrowse";
      this.btnBrowse.Size = new System.Drawing.Size(77, 27);
      this.btnBrowse.TabIndex = 1;
      this.btnBrowse.Text = "Browse...";
      this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(10, 28);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(220, 22);
      this.textBox1.TabIndex = 0;
      // 
      // btnIncreaseSpeed
      // 
      this.btnIncreaseSpeed.Location = new System.Drawing.Point(10, 157);
      this.btnIncreaseSpeed.Name = "btnIncreaseSpeed";
      this.btnIncreaseSpeed.Size = new System.Drawing.Size(76, 28);
      this.btnIncreaseSpeed.TabIndex = 3;
      this.btnIncreaseSpeed.Text = "Speed++";
      this.btnIncreaseSpeed.Click += new System.EventHandler(this.button4_Click);
      // 
      // btnLowerSpeed
      // 
      this.btnLowerSpeed.Location = new System.Drawing.Point(96, 157);
      this.btnLowerSpeed.Name = "btnLowerSpeed";
      this.btnLowerSpeed.Size = new System.Drawing.Size(77, 28);
      this.btnLowerSpeed.TabIndex = 4;
      this.btnLowerSpeed.Text = "Speed--";
      this.btnLowerSpeed.Click += new System.EventHandler(this.button5_Click);
      // 
      // lblSpeed
      // 
      this.lblSpeed.Location = new System.Drawing.Point(182, 163);
      this.lblSpeed.Name = "lblSpeed";
      this.lblSpeed.Size = new System.Drawing.Size(23, 18);
      this.lblSpeed.TabIndex = 5;
      this.lblSpeed.Text = "6";
      // 
      // checkBox1
      // 
      this.checkBox1.Location = new System.Drawing.Point(10, 120);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(76, 18);
      this.checkBox1.TabIndex = 6;
      this.checkBox1.Text = "Stereo";
      this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
      // 
      // checkBox2
      // 
      this.checkBox2.Checked = true;
      this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBox2.Location = new System.Drawing.Point(96, 120);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(86, 18);
      this.checkBox2.TabIndex = 7;
      this.checkBox2.Text = "16 Bit";
      this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this._comboBoxInterpolation);
      this.groupBox2.Location = new System.Drawing.Point(173, 83);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(153, 55);
      this.groupBox2.TabIndex = 8;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Interpolation:";
      // 
      // _comboBoxInterpolation
      // 
      this._comboBoxInterpolation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._comboBoxInterpolation.Items.AddRange(new object[] {
            "None",
            "Linear",
            "Cubic"});
      this._comboBoxInterpolation.Location = new System.Drawing.Point(10, 18);
      this._comboBoxInterpolation.Name = "_comboBoxInterpolation";
      this._comboBoxInterpolation.Size = new System.Drawing.Size(134, 24);
      this._comboBoxInterpolation.TabIndex = 0;
      this._comboBoxInterpolation.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
      // 
      // txtBoxStatus
      // 
      this.txtBoxStatus.Location = new System.Drawing.Point(346, 9);
      this.txtBoxStatus.Multiline = true;
      this.txtBoxStatus.Name = "txtBoxStatus";
      this.txtBoxStatus.Size = new System.Drawing.Size(172, 74);
      this.txtBoxStatus.TabIndex = 9;
      // 
      // panel1
      // 
      this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panel1.Controls.Add(this.m_patternView);
      this.panel1.Location = new System.Drawing.Point(10, 203);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(816, 315);
      this.panel1.TabIndex = 10;
      // 
      // m_patternView
      // 
      this.m_patternView.BackColor = System.Drawing.Color.White;
      this.m_patternView.CurrentPattern = 0;
      this.m_patternView.CurrentRow = 0;
      this.m_patternView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_patternView.ImeMode = System.Windows.Forms.ImeMode.NoControl;
      this.m_patternView.Location = new System.Drawing.Point(0, 0);
      this.m_patternView.Name = "m_patternView";
      this.m_patternView.Size = new System.Drawing.Size(812, 311);
      this.m_patternView.TabIndex = 0;
      // 
      // listViewSamples
      // 
      this.listViewSamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewSamples.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
      this.listViewSamples.FullRowSelect = true;
      this.listViewSamples.HideSelection = false;
      this.listViewSamples.Location = new System.Drawing.Point(528, 9);
      this.listViewSamples.Name = "listViewSamples";
      this.listViewSamples.Size = new System.Drawing.Size(298, 176);
      this.listViewSamples.TabIndex = 11;
      this.listViewSamples.UseCompatibleStateImageBehavior = false;
      this.listViewSamples.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "#";
      this.columnHeader1.Width = 20;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Name";
      this.columnHeader2.Width = 227;
      // 
      // listViewPatterns
      // 
      this.listViewPatterns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
      this.listViewPatterns.FullRowSelect = true;
      this.listViewPatterns.HideSelection = false;
      this.listViewPatterns.Location = new System.Drawing.Point(346, 92);
      this.listViewPatterns.Name = "listViewPatterns";
      this.listViewPatterns.Size = new System.Drawing.Size(172, 93);
      this.listViewPatterns.TabIndex = 12;
      this.listViewPatterns.UseCompatibleStateImageBehavior = false;
      this.listViewPatterns.View = System.Windows.Forms.View.Details;
      this.listViewPatterns.DoubleClick += new System.EventHandler(this.listView2_DoubleClick);
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "#";
      this.columnHeader3.Width = 20;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Pattern";
      this.columnHeader4.Width = 100;
      // 
      // lblBuffers
      // 
      this.lblBuffers.Location = new System.Drawing.Point(211, 146);
      this.lblBuffers.Name = "lblBuffers";
      this.lblBuffers.Size = new System.Drawing.Size(58, 18);
      this.lblBuffers.TabIndex = 13;
      this.lblBuffers.Text = "Buffers:";
      // 
      // txtBoxNumOfBuffers
      // 
      this.txtBoxNumOfBuffers.Location = new System.Drawing.Point(269, 144);
      this.txtBoxNumOfBuffers.Name = "txtBoxNumOfBuffers";
      this.txtBoxNumOfBuffers.Size = new System.Drawing.Size(57, 22);
      this.txtBoxNumOfBuffers.TabIndex = 14;
      this.txtBoxNumOfBuffers.Text = "4";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(211, 175);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(58, 19);
      this.label3.TabIndex = 15;
      this.label3.Text = "Size:";
      // 
      // txtBoxBufferSize
      // 
      this.txtBoxBufferSize.Location = new System.Drawing.Point(269, 173);
      this.txtBoxBufferSize.Name = "txtBoxBufferSize";
      this.txtBoxBufferSize.Size = new System.Drawing.Size(57, 22);
      this.txtBoxBufferSize.TabIndex = 16;
      this.txtBoxBufferSize.Text = "20";
      // 
      // Form1
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
      this.ClientSize = new System.Drawing.Size(836, 525);
      this.Controls.Add(this.txtBoxBufferSize);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.txtBoxNumOfBuffers);
      this.Controls.Add(this.lblBuffers);
      this.Controls.Add(this.listViewPatterns);
      this.Controls.Add(this.listViewSamples);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.txtBoxStatus);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.checkBox2);
      this.Controls.Add(this.checkBox1);
      this.Controls.Add(this.lblSpeed);
      this.Controls.Add(this.btnLowerSpeed);
      this.Controls.Add(this.btnIncreaseSpeed);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.btnStop);
      this.Controls.Add(this.btnPlay);
      this.Name = "Form1";
      this.Text = "TPlayer";
      this.Load += new System.EventHandler(this.Form1_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}
		
		private PatternView m_patternView;
		private TPParserMOD m_parser = new TPParserMOD();
		private TPPlayer m_player = new TPPlayer();

		private void button1_Click(object sender, EventArgs e) {
			if ((null != m_player) && (null != m_player.Module)) {
				TPOutputFormat outputFormat = new TPOutputFormat();
				outputFormat.BitsPerSample	= checkBox2.Checked ? TPOutputFormat.TPBitsPerSample.Bits16 : TPOutputFormat.TPBitsPerSample.Bits8;
				outputFormat.SampleRate		= 44100;
				outputFormat.Stereo			= checkBox1.Checked;
				outputFormat.InterpolationMethod = (TPOutputFormat.TPInterpolationMethod)_comboBoxInterpolation.SelectedIndex;
				outputFormat.SoundWindow = this;

				try {
					TPOutputDeviceDirectSound dsOutput = new TPOutputDeviceDirectSound(outputFormat, Convert.ToInt32(txtBoxNumOfBuffers.Text), Convert.ToInt32(txtBoxBufferSize.Text));
					m_player.OutputDevice = dsOutput;

					m_player.OnUpdateHandler += new TPPlayer.UpdateEventHandler(player_OnUpdateHandler);

          if ((m_player.CurrentPattern != 0) || (m_player.CurrentRow != 0)) {
						if (m_player.CurrentRow != 0)	{
							m_player.CurrentRow -= 1;
						}	else {
							m_player.CurrentPattern -= 1;
							m_player.CurrentRow = m_player.Module.m_moduleTypeInfo.m_rowsPerPattern -1;
						}
					}

					m_player.Play();
				} catch(Exception ex) {
					MessageBox.Show(ex.Message);
				}
			}
		}


		private void btnStop_Click(object sender, EventArgs e) {
			if (null != m_player) {
				if (m_player.Playing) {
					if (!m_player.Stop()) {
						MessageBox.Show("Unable to stop...(?)");
					}
				}
			}
		}

		private void btnBrowse_Click(object sender, EventArgs e) {
			if (m_player.Playing) {
				MessageBox.Show("Currently playing!");
				return;
			}

			_openFileDialog.Reset();
			if (DialogResult.OK == _openFileDialog.ShowDialog(this)) {
        Cursor.Current = Cursors.WaitCursor;
        textBox1.Text = _openFileDialog.FileName;

				FileStream fs = new FileStream(_openFileDialog.FileName, 
													             FileMode.Open, 
                                       FileAccess.Read);

				m_player.Module = m_parser.LoadModule(fs);

				if (m_player.Module != null) { 
          _comboBoxInterpolation.SelectedIndex = 1; // Linear
					m_player.CurrentPattern = 0;
					m_player.CurrentRow = 0;
					m_player.BPM = 125;
					m_player.Speed = 6;

					this.Text = "TPlayer - " + m_player.Module.m_moduleName;

					m_patternView.LoadModule(m_player.Module);

					m_patternView.CurrentPattern = (int)m_player.Module.m_patternsInfo.m_orderArray[0];

					fs.Close();

					this.listViewSamples.Items.Clear();

					for (int i = 0; i < m_player.Module.m_samples.Length; i++) {
						ListViewItem item = this.listViewSamples.Items.Add(string.Format("{0:0#}", i+1));
						item.SubItems.Add(m_player.Module.m_samples[i].m_sampleName);
					}

					this.listViewPatterns.Items.Clear();

					for (int i = 0; i < m_player.Module.m_patternsInfo.m_songLengthInPatterns; i++)	{
						ListViewItem item = this.listViewPatterns.Items.Add(string.Format("{0:0#}", i));
						item.SubItems.Add(string.Format("Pattern: {0:0#}", (int)m_player.Module.m_patternsInfo.m_orderArray[i]));
					}
				} else {
					MessageBox.Show("Unable to load this module!");
				}

        Cursor.Current = Cursors.Arrow;
			}
		}

		private void button4_Click(object sender, EventArgs e) {
			if (m_player.Speed > 1) {
				m_player.Speed -= 1;
				lblSpeed.Text = m_player.Speed.ToString();
			}
		}

		private void button5_Click(object sender, EventArgs e) {
			if (m_player.Speed < 30) {
				m_player.Speed += 1;
				lblSpeed.Text = m_player.Speed.ToString();
			}
		}

		void updateUIOnMainThread()	{
			m_patternView.CurrentPattern = (int)m_player.Module.m_patternsInfo.m_orderArray[m_player.CurrentPattern];
			m_patternView.CurrentRow = m_player.CurrentRow;

			string info = String.Format(
        "Pattern: {0}/{1}\r\n" +
        "Row: {2}/{3}\r\n" +
        "BPM: {4}\r\n" +
        "Speed: {5}\r\n", 
        m_player.CurrentPattern, 
				m_player.Module.m_patternsInfo.m_songLengthInPatterns-1, m_player.CurrentRow+1, 
				m_player.Module.m_moduleTypeInfo.m_rowsPerPattern, m_player.BPM, m_player.Speed);
			
			txtBoxStatus.Text = info;
			lblSpeed.Text = m_player.Speed.ToString();

			Application.DoEvents();
		}

		private delegate void UpdateUI();
		private UpdateUI _updatedUIFunc = null;

		private void player_OnUpdateHandler(object source, EventArgs e) {
			if (!m_patternView.Created) {
				return;
			}

			if (null == _updatedUIFunc) {
        _updatedUIFunc = new UpdateUI(updateUIOnMainThread);
      }

			BeginInvoke(_updatedUIFunc);
		}

		
		private void Form1_Load(object sender, System.EventArgs e)
		{
			_comboBoxInterpolation.SelectedIndex = 1;
		}

		private void listView2_DoubleClick(object sender, System.EventArgs e)
		{
			if (listViewPatterns.SelectedIndices.Count > 0)
			{
				int index = listViewPatterns.SelectedIndices[0];
				int pattern = (int)m_player.Module.m_patternsInfo.m_orderArray[index];

				if (m_patternView.CurrentPattern != index)
				{
					m_player.CurrentPattern = index;
					m_player.CurrentRow = 0;
					m_patternView.CurrentPattern = pattern;
					m_patternView.CurrentRow = 0;
				}
			}
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e) {
			if (m_player.Playing) {
				m_player.Stop();
				button1_Click(null, null);
			}
		}

		private void checkBox2_CheckedChanged(object sender, EventArgs e) {
			if (m_player.Playing) {
				m_player.Stop();
				button1_Click(null, null);
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)	{
			if (m_player.Playing) {
				m_player.Stop();
				button1_Click(null, null);
			}
		}

	}
}
