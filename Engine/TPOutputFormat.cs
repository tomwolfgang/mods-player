using System;

namespace TPlayer.Engine {
	/// <summary>
	/// 
	/// </summary>
	public struct TPOutputFormat {
		public TPOutputFormat(int sampleRate) {
			m_sampleRate		= sampleRate;
			m_bitsPerSample		= TPBitsPerSample.Bits8;
			m_stereo			= false;
			m_surround			= false;
			m_blockAlign		= (Convert.ToInt16(m_bitsPerSample) / 8) * (m_stereo ? 2 : 1);
      m_interpolationMethod = TPInterpolationMethod.LinearInterpolation;
			m_soundWindow		= null;
		}


		public enum TPBitsPerSample { Bits8 = 8, Bits16 = 16, Bits24 = 24, Bits32 = 32 };
		public enum TPInterpolationMethod { NoInterpolation = 0, LinearInterpolation, CubicInterpolation };
			
		private int m_sampleRate;
		public int SampleRate {
			get {
				return m_sampleRate;
			}
			set {
				if (8000 > value) 
					m_sampleRate = 8000;
				else if (88200 < value)
					m_sampleRate = 88200;
				else
					m_sampleRate = value;
			}
		}

		private TPBitsPerSample m_bitsPerSample;
		public TPBitsPerSample BitsPerSample {
			get {
				return m_bitsPerSample;
			}
			set	{
				if ((TPBitsPerSample.Bits8 != value) && 
            (TPBitsPerSample.Bits16 != value)) {
					m_bitsPerSample = TPBitsPerSample.Bits8;

					m_blockAlign = 1 * (m_stereo ? 2 : 1);
				} else {
					m_bitsPerSample = value;
					m_blockAlign = 2 * (m_stereo ? 2 : 1);
				}
			}
		}

		private int m_blockAlign; // bytes per sample
		public int BlockAlign {
			get {
				return m_blockAlign;
			}
		}

		private bool m_stereo;
		public bool Stereo {
			get {
				return m_stereo;
			}
			set {
				m_stereo = value;

				if (!m_stereo)
					m_surround = false;

				m_blockAlign = (Convert.ToInt16(m_bitsPerSample) / 8) * (m_stereo ? 2 : 1);
			}
		}

		private bool m_surround;
		public bool Surround {
			get {
				return m_surround;
			}
			set {
				if (m_stereo) {
					m_surround = value;
				} else {
					m_surround = false;
				}
			}
		}

		private TPInterpolationMethod m_interpolationMethod;
		public TPInterpolationMethod InterpolationMethod {
			get {
				return m_interpolationMethod;
			}
      set {
				m_interpolationMethod = value;
			}
		}

		private System.Windows.Forms.Control m_soundWindow;
		public System.Windows.Forms.Control SoundWindow	{
			get	{
				return m_soundWindow;
			}
      set	{
				m_soundWindow = value;
			}
		}
	}
}
