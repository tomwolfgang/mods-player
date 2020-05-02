using System;

namespace TPlayer.Model {
	/// <summary>
	/// 
	/// </summary>
	public class TPTrack {
		public TPTrack() {
			Clear();
		}

    public void Clear() {
			m_currentPosition	= 0;
			m_effect			= 0;
			m_effectParam		= 0;
			m_frequency			= 0;
			m_mute				= false;
			m_periodIndex		= -1;
			m_periodValue		= -1;
			m_portaPeriod		= -1;
			m_portaSpeed		= 0;
			m_sample			= -1;
			m_volume			= 0;
			m_vibratoPos		= 0;
			m_vibratoSpeed		= 0;
			m_vibratoDepth		= 0;
			m_tremoloPos		= 0;
			m_tremoloSpeed		= 0;
			m_tremoloDepth		= 0;
			m_volSlideDelta		= 0;
			m_hasNewNote		= false;
			m_forceLeftPan		= false;
			m_forceRightPan		= false;
		}

		private bool m_mute;
		public bool Mute {
			get	{
				return m_mute;
			}
			set {
				m_mute = value;
			}
		}

		private int m_currentPosition;
		public int CurrentPosition {
			get {
				return m_currentPosition;
			}
			set {
				m_currentPosition = value;
			}
		}

		private int m_volume;
		public int Volume	{
			get	{
				return m_volume;
			}
			set {
				m_volume = value;
			}
		}

		private int m_periodValue;
		public int PeriodValue {
			get {
				return m_periodValue;
			}
			set {
				m_periodValue = value;
			}
		}

    private int m_periodIndex;
		public int PeriodIndex {
			get {
				return m_periodIndex;
			}
			set	{
				m_periodIndex = value;
			}
		}

		private int m_frequency;
		public int Frequency {
			get {
				return m_frequency;
			}
			set {
				m_frequency = value;
			}
		}

		private int m_sample;
		public int Sample	{
			get	{
				return m_sample;
			}
			set	{
				m_sample = value;
			}
		}
		
		private int m_effect;
		public int Effect	{
			get {
				return m_effect;
			}
			set {
				m_effect = value;
			}
		}

    private int m_effectParam;
		public int EffectParam {
			get {
				return m_effectParam;
			}
			set {
				m_effectParam = value;
			}
		}

		// Specific effects fields
		private int m_portaPeriod;
		public int PortaPeriod {
			get {
				return m_portaPeriod;
			}
			set {
				m_portaPeriod = value;
			}
		}
	
		private int m_portaSpeed;
		public int PortaSpeed {
			get {
				return m_portaSpeed;
			}
			set {
				m_portaSpeed = value;
			}
		}

		private int m_vibratoPos;
		public int VibratoPos {
			get {
				return m_vibratoPos;
			}
			set {
				m_vibratoPos = value;
			}
		}

    private int m_vibratoSpeed;
		public int VibratoSpeed {
			get	{
				return m_vibratoSpeed;
			}
			set {
				m_vibratoSpeed = value;
			}
		}

		private int m_vibratoDepth;
		public int VibratoDepth	{
			get	{
				return m_vibratoDepth;
			}
			set	{
				m_vibratoDepth = value;
			}
		}

		private int m_tremoloPos;
		public int TremoloPos	{
			get	{
				return m_tremoloPos;
			}
			set {
				m_tremoloPos = value;
			}
		}

		private int m_tremoloSpeed;
		public int TremoloSpeed	{
			get	{
				return m_tremoloSpeed;
			}
			set {
				m_tremoloSpeed = value;
			}
		}

		private int m_tremoloDepth;
		public int TremoloDepth	{
			get {
				return m_tremoloDepth;
			}
			set	{
				m_tremoloDepth = value;
			}
		}

		private int m_volSlideDelta;
		public int VolumeSlideDelta	{
			get {
				return m_volSlideDelta;
			}
			set {
				m_volSlideDelta = value;
			}
		}

    private bool m_hasNewNote;
		public bool HasNewNote {
			get {
				return m_hasNewNote;
			}
			set {
				m_hasNewNote = value;
			}
		}

		private bool m_forceLeftPan;
		public bool ForceLeftPanning {
			get {
				return m_forceLeftPan;
			}
			set {
				m_forceLeftPan = value;
			}
		}

    private bool m_forceRightPan;
		public bool ForceRightPanning {
			get {
				return m_forceRightPan;
			}
			set {
				m_forceRightPan = value;
			}
		}
	}
}
