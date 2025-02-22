﻿using Cove.Server.Plugins;
using Cove.Server;
using Cove.Server.Chalk;
using System.Text.Json;

// Change the namespace and class name!
namespace PersistentChalk
{
    public class PersistentChalk : CovePlugin
    {
        public PersistentChalk(CoveServer server) : base(server) { }
        private string currentDir = Directory.GetCurrentDirectory();

        public override void onInit()
        {
            base.onInit();

            // check if there is a chalk.bin file in the current directory
            if (File.Exists(Path.Combine(currentDir, "chalk.json")))
            {
                byte[] chalkData = File.ReadAllBytes(Path.Combine(currentDir, "chalk.bin"));
                Log("Chalk data file found. Loading chalk data...");
                loadChalk(chalkData);
            } else
            {
                // log that the chalk data file does not exist
                Log("Cannot find chalk data file.");
            }
        }

        public long lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // now
        public override void onUpdate()
        {
            base.onUpdate();
            // auto save the chalk data every 5 minutes if a player is online
            if (ParentServer.AllPlayers.Count > 0 && DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastUpdate > 300)
            {
                saveChalk();
                lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Log("Saving Chalk Data!");
            }
        }

        public void loadChalk(byte[] chalkData)
        {
            // deserialize the chalk data
            List<ChalkCanvas> chalk = JsonSerializer.Deserialize<List<ChalkCanvas>>(chalkData);
            if (chalk != null)
            {
                // set the chalk data to the server's chalk data
                ParentServer.chalkCanvas = chalk;
                Log("Restored Chalk Data");
            } else
            {
                Log("Failed to restore chalk data, chalk file is corrupt");
            }
        }

        public void saveChalk()
        {
            // get the canvas data
            List<ChalkCanvas> chalkData = ParentServer.chalkCanvas;

            // use the json formatter to serialize the chalk data
            string json = JsonSerializer.Serialize(chalkData);

            // write the json string to a file
            File.WriteAllText(Path.Combine(currentDir, "chalk.json"), json);
        }
    }
}