﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;
using Org.InCommon.InCert.Engine.Importables;
using Org.InCommon.InCert.Engine.Logging;
using log4net;

namespace Org.InCommon.InCert.Engine.AdvancedMenu
{
    class AdvancedMenuManager : IAdvancedMenuManager
    {
       
        private static readonly ILog Log = Logger.Create();
        public Dictionary<string, IAdvancedMenuItem> Items { get; private set; }

        public AdvancedMenuManager()
        {
            Items = new Dictionary<string, IAdvancedMenuItem>();
            DefaultTitle = "Advanced Support Tools";
            DefaultDescription = "This is a place-holder description.";
            RunButtonText = "Run";
            CloseButtonText = "Close";
            HelpButtonText = "Help";
        }

        public bool ImportItemsFromXml(XElement node)
        {
            try
            {
                if (node == null)
                {
                    Log.Warn("empty xml document passed to ImportBranchesFromXml; cannot import task branches");
                    return false;
                }

                var itemsNode = node.Element("AdvancedMenuItems");
                if (itemsNode == null)
                    return false;

                foreach (var itemNode in itemsNode.Elements())
                {
                    var item = AbstractImportable.GetInstanceFromNode<AdvancedMenuItem>(itemNode);
                    if (item == null || !item.Initialized())
                        continue;

                    Items[item.Branch.ToLowerInvariant()] = item;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.WarnFormat("An exception occurred while importing advanced menu items from xml: {0}", e.Message);
                return false;
            }
        }

        public bool ImportItems(List<AdvancedMenuItem> items)
        {
            try
            {
                if (items == null)
                    return true;

                if (!items.Any())
                    return true;

                foreach (var item in items.Where(item => item.Initialized()))
                    Items[item.Branch.ToLowerInvariant()] = item;

                return true;
            }
            catch (Exception e)
            {
                Log.WarnFormat("An exception occurred while importing advanced menu items from list: {0}", e.Message);
                return false;
            }

        }
        
        public void Initialize()
        {
            
        }

        public string DefaultTitle { get; set; }
        public string DefaultDescription { get; set; }
        public string HelpTopic { get; set; }
        public string WindowTitle { get; set; }
        public double InitialLeftOffset { get; set; }
        public double InitialTopOffset { get; set; }
        public Brush DialogForeground { get; set; }
        public Brush ContainerForeground { get; set; }
        public Brush TopBannerForeground { get; set; }
        public Brush DialogBackground { get; set; }
        public Brush ContainerBackground { get; set; }
        public Brush TopBannerBackground { get; set; }
        public string HelpButtonText { get; set; }
        public string RunButtonText { get; set; }
        public string CloseButtonText { get; set; }
        public string RunButtonImageKey { get; set; }
        public string RunButtonMouseOverImageKey { get; set; }
        public string HelpButtonImageKey { get; set; }
        public string HelpButtonMouseOverImageKey { get; set; }
        public string CloseButtonImageKey { get; set; }
        public string CloseButtonMouseOverImageKey { get; set; }
    }
}
