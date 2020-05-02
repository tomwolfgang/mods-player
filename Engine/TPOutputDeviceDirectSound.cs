using System;
using Microsoft.DirectX;
using System.Threading;

namespace TPlayer.Engine {
	/// <summary>
	/// This class is a DirectSound output device implementation
	/// </summary>
	public class TPOutputDeviceDirectSound {
		#region Private Members

		// our direct sound buffer
    
		private Microsoft.DirectX.DirectSound.Buffer m_dsBuffer = null;

		// a temporary window control that will be passed to the direct sound device
		//private System.Windows.Forms.Control m_dsTempControl	= null;

		// event management members
		private AutoResetEvent[] m_dsNotifyEvents = null;
		private Microsoft.DirectX.DirectSound.BufferPositionNotify[] m_dsBufPosNotifiers = null;

		// threading members
		private Thread m_playThread = null;
		private ThreadStart m_playThreadStart = null;

		// holds the direct sound buffer's real size (samples * sample-block-size)
		private int m_bufferSize	= 0;

		// the current buffer out of the number of DS buffers
		private int m_currentBuffer = 0;

		#endregion

		// event for next buffer calls
		public delegate void NextBufferEventHandler(object source, EventArgs e);
		public event NextBufferEventHandler OnNextBufferHandler = null;

		private TPOutputFormat m_outputFormat;
		public TPOutputFormat OutputFormat {
			get	{
				return m_outputFormat;
			}
		}

		private int m_samplesPerMilliseconds;
		public int SamplesPerMilliseconds	{
			get {
				return m_samplesPerMilliseconds;
			}
		}

		private int m_numberOfBuffers;
		public int NumberOfBuffers {
			get {
				return m_numberOfBuffers;
			}
		}

		/// <summary>
		/// calculate, given a milliseconds value, how many samples are to be played according to the
		/// current output format 
		/// </summary>
		/// <returns></returns>
		public static int CalculateSamplesPerMilliseconds(int milliseconds, int sampleRate)	{
			// We will use a simple cross multiplication equation:
			//
			// samples			milliseconds
			// =======			============
			//	44100			    1000
			//    X					  30
			//
			// X = (30 * 44100) / 1000 = 1323
			//
			// the above example is in case our sample rate is 44100 - this
			// means we play 44100 samples per second (or 1000 milliseconds)

			int samplesPerMilliseconds = (milliseconds * sampleRate) / 1000;
			return samplesPerMilliseconds;
		}

		public void PlayEventListener() {
			while (m_dsBuffer.Status.Playing) {
				WaitHandle.WaitAny(m_dsNotifyEvents); // wait half way

				// throw event
				if (null != OnNextBufferHandler)
					OnNextBufferHandler(this, null);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="outputFormat"></param>
		/// <param name="numberOfBuffers"></param>
		/// <param name="bufferSizeInMilliseconds"></param>
		public TPOutputDeviceDirectSound(TPOutputFormat outputFormat,
                                     int numberOfBuffers,
										                 int bufferSizeInMilliseconds) {
			if ((numberOfBuffers < 1) || (numberOfBuffers > 10)) {
				throw new ArgumentOutOfRangeException("numberOfBuffers Must Range Between 1 And 10!");
			}

			if ((bufferSizeInMilliseconds < 20) || (bufferSizeInMilliseconds > 300)) {
				throw new ArgumentOutOfRangeException("bufferSizeInMilliseconds Must Range Between 20 And 300!");
			}

			m_outputFormat				= outputFormat;
			m_numberOfBuffers			= numberOfBuffers;
			m_currentBuffer				= 0;
			m_samplesPerMilliseconds	= CalculateSamplesPerMilliseconds(bufferSizeInMilliseconds,
																		  m_outputFormat.SampleRate);
			m_bufferSize				= m_samplesPerMilliseconds * m_outputFormat.BlockAlign;

			// set cooperative level
			if (null == outputFormat.SoundWindow)
				outputFormat.SoundWindow = new System.Windows.Forms.Control();

			Microsoft.DirectX.DirectSound.Device dsDevice = null;
			dsDevice = new Microsoft.DirectX.DirectSound.Device();
			dsDevice.SetCooperativeLevel(outputFormat.SoundWindow, Microsoft.DirectX.DirectSound.CooperativeLevel.Priority);

			#region Primary Buffer Setup

			// set the primary buffer's output information
			Microsoft.DirectX.DirectSound.BufferDescription bufferDesc = null;
			bufferDesc = new Microsoft.DirectX.DirectSound.BufferDescription();

			bufferDesc.PrimaryBuffer = true;       

			Microsoft.DirectX.DirectSound.WaveFormat wf = new Microsoft.DirectX.DirectSound.WaveFormat();
			wf.FormatTag				= Microsoft.DirectX.DirectSound.WaveFormatTag.Pcm;
			wf.Channels					= outputFormat.Stereo ? (short)2 : (short)1;
			wf.SamplesPerSecond			= outputFormat.SampleRate;
			wf.BitsPerSample			= (outputFormat.BitsPerSample == TPOutputFormat.TPBitsPerSample.Bits8) ? (short)8 : (short)16;
			wf.BlockAlign				= (short)((wf.BitsPerSample / 8) * wf.Channels);
			wf.AverageBytesPerSecond	= wf.SamplesPerSecond * wf.BlockAlign;

			bufferDesc.BufferBytes = 0;

			Microsoft.DirectX.DirectSound.Buffer dsBufferPrimary = new Microsoft.DirectX.DirectSound.Buffer(bufferDesc, dsDevice);
			dsBufferPrimary.Format = wf;

			#endregion

			// create our direct sound secondary buffer - the actual buffer to be used
			// to write our sound data
			Microsoft.DirectX.DirectSound.BufferDescription bufferDesc2 = null;
			bufferDesc2 = new Microsoft.DirectX.DirectSound.BufferDescription();

			bufferDesc2.Flags			= Microsoft.DirectX.DirectSound.BufferDescriptionFlags.ControlPositionNotify;
			bufferDesc2.DeferLocation	= true;
			bufferDesc2.GlobalFocus		= true;
			
			Microsoft.DirectX.DirectSound.WaveFormat wf2 = new Microsoft.DirectX.DirectSound.WaveFormat();
			wf2.FormatTag				= Microsoft.DirectX.DirectSound.WaveFormatTag.Pcm;
			wf2.Channels				= outputFormat.Stereo ? (short)2 : (short)1;
			wf2.SamplesPerSecond		= outputFormat.SampleRate;
			wf2.BitsPerSample			= (outputFormat.BitsPerSample == TPOutputFormat.TPBitsPerSample.Bits8) ? (short)8 : (short)16;
			wf2.BlockAlign				= (short)((wf2.BitsPerSample / 8) * wf2.Channels);
			wf2.AverageBytesPerSecond	= wf2.SamplesPerSecond * wf2.BlockAlign;

			bufferDesc2.Format = wf2;

			// set the buffer's real size = number of buffer * size of buffer
			bufferDesc2.BufferBytes = m_numberOfBuffers * m_bufferSize;

			m_dsBuffer = new Microsoft.DirectX.DirectSound.SecondaryBuffer(bufferDesc2, dsDevice);

			// create notification events
			m_dsNotifyEvents	= new AutoResetEvent[m_numberOfBuffers];
			m_dsBufPosNotifiers = new Microsoft.DirectX.DirectSound.BufferPositionNotify[m_numberOfBuffers];

			for (int i = 0; i < m_numberOfBuffers; i++) {
				m_dsNotifyEvents[i] = new AutoResetEvent(false);
				m_dsBufPosNotifiers[i] = new Microsoft.DirectX.DirectSound.BufferPositionNotify();

				m_dsBufPosNotifiers[i].Offset			 = ((i + 1) * m_bufferSize) - 1;
				m_dsBufPosNotifiers[i].EventNotifyHandle = m_dsNotifyEvents[i].Handle;
			}
            
			Microsoft.DirectX.DirectSound.Notify notify = 
									new Microsoft.DirectX.DirectSound.Notify(m_dsBuffer);

			notify.SetNotificationPositions(m_dsBufPosNotifiers, m_numberOfBuffers);

			// initialize our play event thread
			m_playThreadStart = new ThreadStart(PlayEventListener);
		}

		public bool Write(byte[] pcmBuffer)	{
			if (null != m_dsBuffer) {
				if (m_currentBuffer >= m_numberOfBuffers)
					m_currentBuffer = 0;
				
				int curOffset = m_currentBuffer * m_bufferSize;

				m_dsBuffer.Write(curOffset, pcmBuffer, Microsoft.DirectX.DirectSound.LockFlag.None);

				m_currentBuffer++;

				return true;
			}

			return false;
		}

		public void Play() {
			if (null != m_dsBuffer) {
				if (!m_dsBuffer.Status.Playing) {
					m_dsBuffer.Play(0, Microsoft.DirectX.DirectSound.BufferPlayFlags.Looping);
				
					if (null != m_playThread) {
						m_playThread.Abort();
						m_playThread.Join();
					}

					m_playThread = new Thread(m_playThreadStart);
					m_playThread.IsBackground = true;
					m_playThread.Start();
				}	else {
					throw new ApplicationException("The Device Is Already In Playing Mode!");
				}
			}	else {
				throw new ApplicationException("The Device Wasn't Initialized Properly");
			}
		}

		public void Stop() {
			if (null != m_dsBuffer)	{
				m_dsBuffer.Stop();
			}
		}

	}
}
