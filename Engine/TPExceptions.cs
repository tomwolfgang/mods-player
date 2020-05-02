using System;

namespace TPlayer.Engine {
	public class TPFieldChangeException : Exception	{
		public TPFieldChangeException() : base() {
    }

		public TPFieldChangeException(String message)	: base(message) {
    }

		public TPFieldChangeException(String message, Exception innerException)	:
      base(message, innerException) {
    }
	}
}
