using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using TPlayer.Model;

namespace TPlayer
{
	/// <summary>
	/// Summary description for PatternView.
	/// </summary>
	public class PatternView : System.Windows.Forms.Control {
		#region Constants
		
		private const int FONT_CHAR_HEIGHT = 13;
		private const int FONT_CHAR_WIDTH = 6;
		private const int GRID_BORDER_WIDTH = 4;

		#endregion

		public PatternView() {
			InitializeComponent();
			Init();
		}
		
		private void Init()	{
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.DoubleBuffer, true);
			this.SetStyle(ControlStyles.ResizeRedraw, true);
			this.SetStyle(ControlStyles.Selectable, true);
			this.TabStop = true;

			// create column header font
			m_font = new Font("Comic Sans MS", 9);

			if (m_font.Name.CompareTo("Comic Sans MS") != 0) {
				m_font = new Font("MS Sans Serif", 9);
			}

			// initialize size variables
			
			// a column is built of: NNN_SS__EEE (11 characters)
			// NNN = note
			// SS = sample
			// EEE = effect
			m_columnWidth = FONT_CHAR_WIDTH*11;

			// the number column is built of 3 characters
			m_numberColumnWidth = FONT_CHAR_WIDTH*3;
			
			//m_rowHeight = FONT_CHAR_HEIGHT;

			System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

			Image img = Image.FromStream(asm.GetManifestResourceStream("TPlayer.font.bmp"));
			
			int numberOfPics = 20;

			m_imgParserFont = new ImageParser(img, numberOfPics, new System.Drawing.Size(FONT_CHAR_WIDTH, FONT_CHAR_HEIGHT), System.Drawing.Color.FromArgb(255,0,255));
		}


		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )	{
			if( disposing )	{
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			components = new System.ComponentModel.Container();
		}
		#endregion

		#region Private Members

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private ImageParser m_imgParserFont = null;
		
		/// <summary>
		/// Stores memory bitmaps that will hold the patterns, this way we will be able to
		/// update the pattern view very fast and won't delay the playing of the MODs
		/// </summary>
		private ArrayList m_patternBitmapList = null;

		// this will be the background image - columns and grid
		private Bitmap m_backgroundImage = null;

		private TPModule m_module = null;

		private int m_columnWidth = 0;
		private int m_numberColumnWidth = 0;
		private int m_bitmapWidth = 0;
		private int m_bitmapHeight = 0;

		private int m_rowCount = 64;
		private int m_channels = 4; // number of channels
		private System.Drawing.Font m_font = null;

		private int m_currentRow = 0;
		private int m_currentPattern = 0;

		#endregion

		#region Properties
		
		[System.ComponentModel.Browsable(false)]
		public int CurrentRow {
			get {
				return m_currentRow;
			}
			set {
				int diff = value%m_rowCount;

				if (diff != m_currentRow) {
					m_currentRow = diff;
					Invalidate();
					Update();
				}
			}
		}

		[System.ComponentModel.Browsable(false)]
		public int CurrentPattern	{
			get	{
				return m_currentPattern;
			}
			set	{
				if (value != m_currentPattern) {
					m_currentPattern = value;
					Invalidate();
					Update();
				}
			}
		}

		
		// hide unused properties
		[System.ComponentModel.Browsable(false)]
		public override string Text {
			get	{
				return base.Text;
			}
			set {
				//base.Text = value;
			}
		}
	
		[System.ComponentModel.Browsable(false)]
		public override bool AllowDrop {
			get	{
				return base.AllowDrop;
			}
			set	{
				base.AllowDrop = value;
			}
		}


		[System.ComponentModel.Browsable(false)]
		public override RightToLeft RightToLeft	{
			get	{
				return base.RightToLeft;
			}
			set {
				base.RightToLeft = value;
			}
		}

		[System.ComponentModel.Browsable(false)]
		public override Font Font {
			get {
				return base.Font;
			}
			set	{
				base.Font = value;
			}
		}
		
		#endregion

		public bool LoadModule(TPModule module)	{
			m_module = module;
			m_currentRow = 0;
			m_currentPattern = 0;

			if (m_module.m_patterns.Length > 0) {
				m_channels = m_module.m_moduleTypeInfo.m_channels;
				m_rowCount = m_module.m_moduleTypeInfo.m_rowsPerPattern;

				// calculate the bitmaps width and height
				m_bitmapWidth = (m_numberColumnWidth + GRID_BORDER_WIDTH) + ((m_columnWidth + GRID_BORDER_WIDTH)*m_channels);
				m_bitmapHeight = m_rowCount * FONT_CHAR_HEIGHT;

				if (CreateBackgroundBitmap())	{
					m_patternBitmapList = new ArrayList();

					for (int i = 0; i < m_module.m_patterns.Length; i++) {
						m_patternBitmapList.Add(CreateBitmapFromPattern(m_module.m_patterns[i], m_channels, m_rowCount));
					}

					Invalidate();
					Update();

					return true;
				}
			}

			return false;
		}

		private bool CreateBackgroundBitmap() {
			if (null != m_backgroundImage) {
				m_backgroundImage.Dispose();
				m_backgroundImage = null;
			}

			m_backgroundImage = new Bitmap(m_bitmapWidth, m_bitmapHeight);

			Graphics graphics = Graphics.FromImage(m_backgroundImage);

			DrawGridLines(graphics, m_channels);
			DrawColumns(graphics, m_channels);

			return true;
		}

		private void DrawNumber(Graphics grph, int number, int dstX, int dstY) {
			string numberString = number.ToString();
			DrawNumber(grph, numberString, dstX, dstY);
		}

		private void DrawNumber(Graphics grph, string number, int dstX, int dstY)	{
			int currentXPos = dstX;

			int offset = 7;

			for (int i = 0; i < number.Length; i++)	{
				int currentNumber = Int32.Parse(number.Substring(i,1));

				m_imgParserFont.Draw(grph, currentXPos, dstY, offset + currentNumber);
				currentXPos += FONT_CHAR_WIDTH;
			}
		}

		private void DrawEffect(Graphics grph, TPNote note, int dstX, int dstY)	{
			if (note.m_effect < 0)
				return;

			int currentXPos = dstX;

			int effectIndex = (7 + note.m_effect) % 17;
			//bool splitEffectParam = false;

			#region Switch Effects

//			switch(note.m_effect)
//			{
//				case TP_EFFECT_ARPEGGIO:
//					splitEffectParam = true;
//					break;
//				case TP_EFFECT_PORTAMENTO_UP:
//				case TP_EFFECT_PORTAMENTO_DOWN:
//					splitEffectParam = false;
//					break;
//				case TP_EFFECT_VIBRATO:
//					break;
//				case TP_EFFECT_VOLUME_SLIDE_AND_TONE_PORTAMENTO:
//					break;
//				case TP_EFFECT_VOLUME_SLIDE_AND_VIBRATO:
//					break;
//				case TP_EFFECT_TREMOLO:
//					break;
//				case TP_EFFECT_SET_PANNING:
//					break;
//				case TP_EFFECT_SET_OFFSET:
//					break;
//				case TP_EFFECT_VOLUME_SLIDE:
//					break;
//				case TP_EFFECT_POSITION_JUMP:
//					break;
//				case TP_EFFECT_SET_VOLUME:
//					break;
//				case TP_EFFECT_PATTERN_BREAK:
//					break;
//				case TP_EFFECT_SET_SPEED:
//					break;
//			}

			#endregion
			
			m_imgParserFont.Draw(grph, currentXPos, dstY, effectIndex);
			currentXPos += FONT_CHAR_WIDTH;

			string ret = string.Format("{0:0X}", note.m_effectParams);
			

		}

		private void DrawNote(Graphics grph, TPNote note, int dstX, int dstY)	{
			int currentXPos = dstX;

			bool sharp = false;
			int letterIndex = 0;         			

			if (note.m_note != TPNote.TPNotes.None)	{
				#region Switch Note

				switch(note.m_note) {
					case TPNote.TPNotes.A:
						sharp = false;
						letterIndex = 0;
						break;
					case TPNote.TPNotes.ASharp:
						sharp = true;
						letterIndex = 0;
						break;
					case TPNote.TPNotes.B:
						sharp = false;
						letterIndex = 1;
						break;
					case TPNote.TPNotes.C:
						sharp = false;
						letterIndex = 2;
						break;
					case TPNote.TPNotes.CSharp:
						sharp = true;
						letterIndex = 2;
						break;
					case TPNote.TPNotes.D:
						sharp = false;
						letterIndex = 3;
						break;
					case TPNote.TPNotes.DSharp:
						sharp = true;
						letterIndex = 3;
						break;
					case TPNote.TPNotes.E:
						sharp = false;
						letterIndex = 4;
						break;
					case TPNote.TPNotes.F:
						sharp = false;
						letterIndex = 5;
						break;
					case TPNote.TPNotes.FSharp:
						sharp = true;
						letterIndex = 5;
						break;
					case TPNote.TPNotes.G:
						sharp = false;
						letterIndex = 6;
						break;
					case TPNote.TPNotes.GSharp:
						sharp = true;
						letterIndex = 6;
						break;
				}

				#endregion

				m_imgParserFont.Draw(grph, currentXPos, dstY, letterIndex);

				currentXPos += FONT_CHAR_WIDTH;
				m_imgParserFont.Draw(grph, currentXPos, dstY, sharp ? 18 : 17);
				currentXPos += FONT_CHAR_WIDTH;
				m_imgParserFont.Draw(grph, currentXPos, dstY, 7 + note.m_octave); // offset = 7
			}	else {
				m_imgParserFont.Draw(grph, currentXPos, dstY, 19);
				currentXPos += FONT_CHAR_WIDTH;
				m_imgParserFont.Draw(grph, currentXPos, dstY, 19);
				currentXPos += FONT_CHAR_WIDTH;
				m_imgParserFont.Draw(grph, currentXPos, dstY, 19);
			}
			
			currentXPos += FONT_CHAR_WIDTH*2;
			if (note.m_sampleNumber > 0) {
				DrawNumber(grph, string.Format("{0:0#}", note.m_sampleNumber), currentXPos, dstY);
				currentXPos += FONT_CHAR_WIDTH;
			} else {
				m_imgParserFont.Draw(grph, currentXPos, dstY, 19);
				currentXPos += FONT_CHAR_WIDTH;
				m_imgParserFont.Draw(grph, currentXPos, dstY, 19);
			}

			currentXPos += FONT_CHAR_WIDTH*3;

			if (note.m_effect > 0) {
				DrawEffect(grph, note, currentXPos, dstY);
			} else {
				m_imgParserFont.Draw(grph, currentXPos, dstY, 19);
			}
			
			currentXPos += FONT_CHAR_WIDTH;
			m_imgParserFont.Draw(grph, currentXPos, dstY, 19);
			currentXPos += FONT_CHAR_WIDTH;
			m_imgParserFont.Draw(grph, currentXPos, dstY, 19);
		}

		/// <summary>
		/// This method will create a memory bitmap from a given pattern
		/// </summary>
		private Bitmap CreateBitmapFromPattern(TPPattern pattern, int channels, int rows)	{
			try	{
				Bitmap bmp = new Bitmap(m_bitmapWidth, m_bitmapHeight);

				Graphics graphics = Graphics.FromImage(bmp);

				int currentYPos = 0;

				for (int i = 0; i < rows; i++) {
					DrawNumber(graphics, string.Format("{0:0#}", i), 1, currentYPos);

					int currentXPos = this.m_numberColumnWidth + GRID_BORDER_WIDTH;

					for (int j = 0; j < channels; j++) {
						TPNote note = pattern.m_rows[i,j];
						
						DrawNote(graphics, note, currentXPos, currentYPos);

						currentXPos += this.m_columnWidth + GRID_BORDER_WIDTH;
					}

					currentYPos += FONT_CHAR_HEIGHT;
				}

				return bmp;
			}	catch(Exception) {
			}

			return null;
		}

		private void DrawGridLines(Graphics graphics, int channels) {
			int currentXPos = m_numberColumnWidth;

			for (int i = 0; i < channels; i++) {
				// we will draw each grid line from the end of the column header till the height of the bitmap
				ControlPaint.DrawBorder3D(graphics, currentXPos, FONT_CHAR_HEIGHT, GRID_BORDER_WIDTH, 
										  m_bitmapHeight, Border3DStyle.Raised, System.Windows.Forms.Border3DSide.All);
				currentXPos += (GRID_BORDER_WIDTH + m_columnWidth);
			}
		}

		private void DrawColumns(Graphics graphics, int channels)	{
			Pen pen1 = new Pen(Color.FromArgb(226, 222, 205), 2);
			Pen pen2 = new Pen(Color.FromArgb(214, 210, 194), 2);
			Pen pen3 = new Pen(Color.FromArgb(203, 199, 184), 2);

			graphics.FillRectangle(SystemBrushes.Control, 0, 0,m_bitmapWidth, FONT_CHAR_HEIGHT - 3);
			
			graphics.DrawLine(pen1, 0, FONT_CHAR_HEIGHT-2, m_bitmapWidth, FONT_CHAR_HEIGHT-2);
			graphics.DrawLine(pen2, 0, FONT_CHAR_HEIGHT-1, m_bitmapWidth, FONT_CHAR_HEIGHT-1);
			graphics.DrawLine(pen3, 0, FONT_CHAR_HEIGHT, m_bitmapWidth, FONT_CHAR_HEIGHT);

			pen1.Dispose();
			pen2.Dispose();
			pen3.Dispose();

			int currentXPos = m_numberColumnWidth;

			int middleGrid = GRID_BORDER_WIDTH /2;

			// draw grid highlight
			graphics.DrawLine(SystemPens.ControlDark, currentXPos+middleGrid, 1, currentXPos+middleGrid, FONT_CHAR_HEIGHT-3);
			graphics.DrawLine(Pens.White, currentXPos+middleGrid+1, 1, currentXPos+middleGrid+1, FONT_CHAR_HEIGHT-3);
			
			// draw number column title
			graphics.DrawString("#", m_font, Brushes.Black, (m_numberColumnWidth/2) - (graphics.MeasureString("#", m_font).Width / 2),-2);
			
			// draw the rest of the column's title
			for (int i = 1; i <= channels; i++)	{
				graphics.DrawString("Channel " + i.ToString(), m_font, 
					Brushes.Black, currentXPos + GRID_BORDER_WIDTH,-2);

				currentXPos += (m_columnWidth + GRID_BORDER_WIDTH);
				graphics.DrawLine(SystemPens.ControlDark, currentXPos+middleGrid, 1, currentXPos+middleGrid, FONT_CHAR_HEIGHT-3);
				graphics.DrawLine(Pens.White, currentXPos+middleGrid+1, 1, currentXPos+middleGrid+1, FONT_CHAR_HEIGHT-3);
			}
		}

		private void DrawHeader(Graphics graphics) {
			Pen pen1 = new Pen(Color.FromArgb(226, 222, 205), 2);
			Pen pen2 = new Pen(Color.FromArgb(214, 210, 194), 2);
			Pen pen3 = new Pen(Color.FromArgb(203, 199, 184), 2);

			graphics.FillRectangle(SystemBrushes.Control, 0, 0,this.Width, FONT_CHAR_HEIGHT - 3);
			
			graphics.DrawLine(pen1, 0, FONT_CHAR_HEIGHT-2, this.Width, FONT_CHAR_HEIGHT-2);
			graphics.DrawLine(pen2, 0, FONT_CHAR_HEIGHT-1, this.Width, FONT_CHAR_HEIGHT-1);
			graphics.DrawLine(pen3, 0, FONT_CHAR_HEIGHT, this.Width, FONT_CHAR_HEIGHT);

			pen1.Dispose();
			pen2.Dispose();
			pen3.Dispose();
		}
		
		private void HighlightCurrentRow(Graphics graphics)	{
			// calculate how many rows can we draw on (we don't count the columns as a row 
			//										   to draw on - thus the -FONT_CHAR_HEIGHT)
			int numberOfRows = ((this.Height-FONT_CHAR_HEIGHT) / FONT_CHAR_HEIGHT);

			// get the middle row on which we will draw the current selected row
			int middleRow = (int)((numberOfRows / 2)) + 1; // +1 'cause we are skipping the column headers

			int rowWidth = m_numberColumnWidth;
			rowWidth += m_channels*(GRID_BORDER_WIDTH + m_columnWidth);

			// highlight the selected row
			graphics.FillRectangle(Brushes.LightBlue, 
								   0, middleRow*FONT_CHAR_HEIGHT,
								   rowWidth, FONT_CHAR_HEIGHT);
		}

		private void DrawCurrentPattern(Graphics graphics) {
			// draw rows
			if ((null != m_patternBitmapList) && (0 < m_patternBitmapList.Count)) {
				if ((m_currentPattern < m_patternBitmapList.Count) && (m_currentRow < m_rowCount)) {
					// calculate how many rows can we draw on (we don't count the columns as a row 
					//										   to draw on - thus the -FONT_CHAR_HEIGHT)
					int numberOfRows = ((this.Height-FONT_CHAR_HEIGHT) / FONT_CHAR_HEIGHT);

					// get the middle row on which we will draw the current selected row
					int middleRow = (int)((numberOfRows / 2)) + 1; // +1 'cause we are skipping the column headers

					middleRow -= m_currentRow;

					graphics.DrawImage((Image)m_patternBitmapList[m_currentPattern], 0, middleRow*FONT_CHAR_HEIGHT);
				}
			}
		}

		protected override void OnPaint(PaintEventArgs e) {
			if (null != m_backgroundImage) {
				HighlightCurrentRow(e.Graphics);
		
				DrawCurrentPattern(e.Graphics);

				DrawHeader(e.Graphics);

				// draw background
				e.Graphics.DrawImage((Image)m_backgroundImage, 0, 0);
			}	else {
				DrawHeader(e.Graphics);
			}
		}

		protected override bool IsInputKey(Keys keyData) {
			if ((keyData & Keys.Alt) != Keys.Alt) {
				Keys key = keyData & Keys.KeyCode;
				
				switch (key) {
					case Keys.Up:
					case Keys.Down:
					case Keys.Left:
					case Keys.Right:
					case Keys.Prior:
					case Keys.Next:
					case Keys.End:
					case Keys.Home:
					{
						return true;
					}
				}

				if (base.IsInputKey(keyData))	{
					return true;
				}
			}

			return false;
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			Keys key = e.KeyData & Keys.KeyCode;
			bool redraw = false;

			if (Keys.Down == key)	{
				if (m_currentRow < (m_rowCount-1)) {
					m_currentRow++;
				} else {
					m_currentRow = 0;
				}

				redraw = true;
			}	else if (Keys.Up == key) {
				if (m_currentRow > 0) {
					m_currentRow--;
				} else {
					m_currentRow = m_rowCount-1;
				}

				redraw = true;
			}

			if (redraw) {
				Invalidate();
				Update();
			}
		}

		protected override void OnClick(EventArgs e) {
			this.Focus();
		}
	}
}
