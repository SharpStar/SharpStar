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
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SharpStar.Lib.Server
{
    // Represents a collection of reusable SocketAsyncEventArgs objects.   
    public class SocketAsyncEventArgsPool : IDisposable
    {
        Stack<SocketAsyncEventArgs> m_pool;

        // Initializes the object pool to the specified size 
        // 
        // The "capacity" parameter is the maximum number of 
        // SocketAsyncEventArgs objects the pool can hold 
        public SocketAsyncEventArgsPool(int capacity)
        {
            m_pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        // Add a SocketAsyncEventArg instance to the pool 
        // 
        //The "item" parameter is the SocketAsyncEventArgs instance 
        // to add to the pool 
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }

        // Removes a SocketAsyncEventArgs instance from the pool 
        // and returns the object removed from the pool 
        public SocketAsyncEventArgs Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }

        // The number of SocketAsyncEventArgs instances in the pool 
        public int Count
        {
            get { return m_pool.Count; }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (m_pool.Count > 0)
                {
                    SocketAsyncEventArgs args = m_pool.Pop();

                    args.Dispose();
                }
            }

            m_pool = null;
        }

        ~SocketAsyncEventArgsPool()
        {
            Dispose(false);
        }

    }
}
