// SharpStar
// Copyright (C) 2014 Mitchell Kutchuk
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Net.Sockets;
using SharpStar.Lib.Logging;

namespace SharpStar.Lib.Extensions
{
	public static class SocketExtensions
	{
		public static bool IsConnected(this Socket socket)
		{
			try
			{
				return !(socket.Poll(1, SelectMode.SelectRead) && (socket.Available == 0) || !socket.Connected);
			}
			catch (Exception)
			{
				return false;
			}
		}

		private const int BytesPerLong = 4; // 32 / 8
		private const int BitsPerByte = 8;

		//Credit: http://snipplr.com/view/54476/
		/// <summary>
		/// Sets the keep-alive interval for the socket.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="time">Time between two keep alive "pings".</param>
		/// <param name="interval">Time between two keep alive "pings" when first one fails.</param>
		/// <returns>If the keep alive infos were succefully modified.</returns>
		public static bool SetKeepAlive(this Socket socket, ulong time, ulong interval)
		{
			try
			{
				// Array to hold input values.
				var input = new[]
				{
					(time == 0 || interval == 0) ? 0UL : 1UL, // on or off
					time,
					interval
				};

				// Pack input into byte struct.
				byte[] inValue = new byte[3 * BytesPerLong];
				for (int i = 0; i < input.Length; i++)
				{
					inValue[i * BytesPerLong + 3] = (byte)(input[i] >> ((BytesPerLong - 1) * BitsPerByte) & 0xff);
					inValue[i * BytesPerLong + 2] = (byte)(input[i] >> ((BytesPerLong - 2) * BitsPerByte) & 0xff);
					inValue[i * BytesPerLong + 1] = (byte)(input[i] >> ((BytesPerLong - 3) * BitsPerByte) & 0xff);
					inValue[i * BytesPerLong + 0] = (byte)(input[i] >> ((BytesPerLong - 4) * BitsPerByte) & 0xff);
				}

				// Create bytestruct for result (bytes pending on server socket).
				byte[] outValue = BitConverter.GetBytes(0);

				// Write SIO_VALS to Socket IOControl.
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
				socket.IOControl(IOControlCode.KeepAliveValues, inValue, outValue);
			}
			catch (SocketException e)
			{
				SharpStarLogger.DefaultLogger.Error("Failed to set keep-alive: {0} {1}", e.ErrorCode, e);
				return false;
			}

			return true;
		}

	}
}