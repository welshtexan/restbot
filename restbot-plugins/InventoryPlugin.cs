/*--------------------------------------------------------------------------------
 LICENSE:
	 This file is part of the RESTBot Project.
 
	 RESTbot is free software; you can redistribute it and/or modify it under
	 the terms of the Affero General Public License Version 1 (March 2002)
 
	 RESTBot is distributed in the hope that it will be useful,
	 but WITHOUT ANY WARRANTY; without even the implied warranty of
	 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.	See the
	 Affero General Public License for more details.

	 You should have received a copy of the Affero General Public License
	 along with this program (see ./LICENSING) If this is missing, please 
	 contact alpha.zaius[at]gmail[dot]com and refer to 
	 <http://www.gnu.org/licenses/agpl.html> for now.
	 
	 Further changes by Gwyneth Llewelyn

 COPYRIGHT: 
	 RESTBot Codebase (c) 2007-2008 PLEIADES CONSULTING, INC
--------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System.Net;
using System.Threading;

// group functions; based on TestClient.exe code
namespace RESTBot
{
	// Set active group, using group UUID
	public class ListInventoryPlugin : StatefulPlugin
	{
		private UUID session;
		private Inventory Inventory;
		private InventoryManager Manager;
		
		public ListInventoryPlugin()
		{
			MethodName = "list_inventory";
		}
		
		public override void Initialize(RestBot bot)
		{
			session = bot.sessionid;
			DebugUtilities.WriteDebug(session + " " + MethodName + " startup");
		}
		
		public override string Process(RestBot b, Dictionary<string, string> Parameters)
		{
			UUID folderID;
			DebugUtilities.WriteDebug("LI - Entering folder key parser");
			try
			{
				bool check = false;
				if (Parameters.ContainsKey("key"))
				{
					DebugUtilities.WriteDebug("LI - Attempting to parse from POST");
					check = UUID.TryParse(Parameters["key"].ToString().Replace("_"," "), out folderID);
					DebugUtilities.WriteDebug("LI - Succesfully parsed POST");
				}
				else
				{
					folderID = UUID.Zero; // start with root folder
					check = true;
				}

				if (check)	// means that we have a correctly parsed key OR no key
							//  which is fine too (attempts root folder)
				{
					DebugUtilities.WriteDebug("List Inventory - Entering loop");
								
					Manager = b.Client.Inventory;
					Inventory = Manager.Store;
	
					StringBuilder response = new StringBuilder();
					
					InventoryFolder startFolder = new InventoryFolder(folderID);
					
					if (folderID == UUID.Zero)
						startFolder = Inventory.RootFolder;

					PrintFolder(b, startFolder, response);
				
					DebugUtilities.WriteDebug("List Inventory - Complete");
					
					return "<inventory>" + response + "</inventory>\n";
				}
				else 
				{
					return "<error>parsekey</error>";
				}
			}
			catch ( Exception e )
			{
				DebugUtilities.WriteError(e.Message);
				return "<error>" + e.Message + "</error>";
			}
		}

		private void PrintFolder(RestBot b, InventoryFolder f, StringBuilder result)
		{	  
			List<InventoryBase> contents = Manager.FolderContents(f.UUID, b.Client.Self.AgentID,
				true, true, InventorySortOrder.ByName, 3000);

			if (contents != null)
			{
				foreach (InventoryBase i in contents)
				{
					result.AppendFormat("<item><name>{0}</name><itemid>{1}</itemid></item>", i.Name, i.UUID);
					if (i is InventoryFolder)
					{
						InventoryFolder folder = (InventoryFolder)i;
						PrintFolder(b, folder, result);
					}
				}
			}
		}

	}
}