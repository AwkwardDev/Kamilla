﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using Kamilla.Network.Logging;
using Kamilla.Network.Parsing;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;
using Kamilla.WPF;

namespace NetworkLogViewer
{
    internal sealed class ViewerImplementation : NetworkLogViewerBase
    {
        MainWindow m_window;
        PacketAddedEventHandler m_packetAddedHandler;
        Protocol m_currentProtocol;
        NetworkLog m_currentLog;
        internal readonly ViewerItemCollection m_items;
        WindowInteropHelper m_interopHelper;

        internal ViewerImplementation(MainWindow window)
        {
            m_window = window;
            m_interopHelper = new WindowInteropHelper(window);

            m_items = new ViewerItemCollection();
            m_items.ItemQueried += (o, e) =>
            {
                if (this.ItemQueried != null)
                    this.ItemQueried(o, e);
            };

            m_packetAddedHandler = (o, e) =>
            {
                var item = new ViewerItem(this, (NetworkLog)o, e.Packet, m_items.Count);
                m_items.Add(item);

                if (this.ItemAdded != null)
                    this.ItemAdded(this, new ViewerItemEventArgs(item));
            };
        }

        public override void UpdateItem(ViewerItem item)
        {
            if (item == null)
                throw new ArgumentNullException();

            if (item.Viewer != this)
                throw new ArgumentException();

            m_items.Update(item);
        }

        protected override void InternalNotifyParsingDone(PacketParser parser)
        {
            if (this.ItemParsingDone != null)
                this.ItemParsingDone(this, new ViewerItemEventArgs(parser.Item));
        }

        internal void SetProtocol(Protocol value)
        {
            if (m_currentProtocol == value)
                return;

            m_window.ThreadSafe(_ =>
            {
                var old = m_currentProtocol;
                if (old != null)
                    old.Unload();

                m_currentProtocol = value;
                m_currentProtocol.Load(this);

                if (this.ProtocolChanged != null)
                    this.ProtocolChanged(this, new ProtocolChangedEventArgs(old, value));
            });
        }

        internal void SetLog(NetworkLog value)
        {
            if (m_currentLog == value)
                return;

            m_window.ThreadSafe(_ =>
            {
                var old = m_currentLog;
                if (old != null)
                    old.PacketAdded -= m_packetAddedHandler;

                m_currentLog = value;
                if (value != null)
                    value.PacketAdded += m_packetAddedHandler;

                if (this.NetworkLogChanged != null)
                    this.NetworkLogChanged(this, new NetworkLogChangedEventArgs(old, value));
            });
        }

        internal void OnStyleChanged(Style oldStyle, Style newStyle)
        {
            if (this.StyleChanged != null)
                this.StyleChanged(this, EventArgs.Empty);
        }

        internal void CloseFile()
        {
            m_items.Clear();
            this.SetLog(null);
        }

        #region Overrides
        /// <summary>
        /// Retrieves an object that contains style information. This value can be null.
        /// </summary>
        public override object Style { get { return m_window.Style; } }

        /// <summary>
        /// Occurs when <see cref="NetworkLogViewer.MainWindow.Style"/> property changes.
        /// </summary>
        public override event EventHandler StyleChanged;

        /// <summary>
        /// Gets the collection of items currently loaded.
        /// </summary>
        public override IEnumerable<ViewerItem> Items { get { return m_items; } }

        /// <summary>
        /// Gets or sets the current <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public override Protocol CurrentProtocol { get { return m_currentProtocol; } }

        /// <summary>
        /// Gets the currently loaded <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        public override NetworkLog CurrentLog { get { return m_currentLog; } }

        /// <summary>
        /// Gets the handle of the window.
        /// </summary>
        public override IntPtr WindowHandle { get { return m_interopHelper.Handle; } }

        /// <summary>
        /// Occurs when <see href="NetworkLogViewer.MainWindow.CurrentProtocol"/> changes.
        /// </summary>
        public override event ProtocolChangedEventHandler ProtocolChanged;

        /// <summary>
        /// Occurs when the <see href="NetworkLogViewer.MainWindow.CurrentLog"/> property changes.
        /// </summary>
        public override event NetworkLogChangedEventHandler NetworkLogChanged;

        /// <summary>
        /// Occurs when data of a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is queried.
        /// </summary>
        public override event ViewerItemEventHandler ItemQueried;

        /// <summary>
        /// Occurs when a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is added.
        /// </summary>
        public override event ViewerItemEventHandler ItemAdded;

        /// <summary>
        /// Occurs when interpreting of contents of a
        /// <see cref="Kamilla.Network.Viewing.ViewerItem"/> is finished.
        /// </summary>
        public override event ViewerItemEventHandler ItemParsingDone;
        #endregion
    }
}
