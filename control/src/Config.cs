using System;
using System.IO;
using System.Json;
using System.Linq;
using System.Collections.Generic;
using DSController.Exception;


namespace DSController
{
    class Config
    {
        /// <summary>
        ///     Bound device to control.
        /// </summary>
        public string Device { get; protected set; }

        /// <summary>
        ///     Controller code to adjust device pin mapping
        /// </summary>
        public string ControllerCode { get; protected set; }
        
        /// <summary>
        /// Button keycodes mapping, refer to http://wiki.libsdl.org/SDL_Keycode for the list.
        /// The DS mapping is A,B,X,Y,L,R,Start,Select,Up,Down,Left,Right,Power.
        /// </summary>
        public Dictionary<string,string> ButtonMapping { get; protected set; }

        public Config()
        {
            this.Device = "";
            this.ControllerCode = "";
            this.ButtonMapping = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Loads the configuration from the given JSON file path and instanciates a new Config object from it.
        /// </summary>
        /// <param name="filePath">The JSON file path.</param>
        /// <returns>The new Config instance.</returns>
        public static Config LoadFromFile(string filePath)
        {
            string configContents;

            if(!File.Exists(filePath)) {
                throw new ConfigurationException("Configuration file not found in path: " + filePath);
            }

            
            try {
                configContents = File.ReadAllText(filePath);
                return Config.LoadFromJson(configContents);
            } catch(FormatException) {
                throw new ConfigurationException("Configuration is not a valid JSON file.");
            }
        }

        /// <summary>
        ///     Loads the configuration from the given JSON and instanciates a new Config object from it.
        /// </summary>
        /// <param name="json">The JSON content to load the config from.</param>
        /// <returns>The new Config instance.</returns>
        public static Config LoadFromJson(string json)
        {
            Config config = new Config();
            JsonValue readJson;
            JsonObject configData;

            try {
                readJson = JsonValue.Parse(json);

                if(readJson.JsonType != JsonType.Object) {
                    throw new ConfigurationException("Configuration invalid.");
                }

                configData = (JsonObject) readJson;
                config.Device = configData["device"];
                config.ControllerCode = configData["controllerCode"];
                
                foreach(KeyValuePair<string, JsonValue> entry in configData["buttonMapping"]) {
                    if(Enum.GetNames(typeof(NDSButton)).Contains(entry.Key)) {
                        if(entry.Value.JsonType == JsonType.String) {
                            config.ButtonMapping.Add(entry.Key, entry.Value);
                        } else {
                            throw new ConfigurationException("Configuration error: data at " + entry.Key + " button should be a string.");
                        }
                    } else {
                        throw new ConfigurationException("Configuration error: " + entry.Key + " button doesn't exist.");
                    }
                }

            } catch(FormatException) {
                throw new ConfigurationException("Configuration is not a valid JSON file.");
            } catch(ArgumentException) {
                throw new ConfigurationException("Configuration is not a valid JSON file.");
            }

            return config;
        }

        /// <summary>
        ///     Generates a skeleton for the configuration file.
        /// </summary>
        /// <returns>The new skeleton.</returns>
        public static string GenerateSkeleton()
        {
            Config emptyConfig = new Config();
            return emptyConfig.ToJson().ToString();
        }

        /// <summary>
        ///     Generates a JSON representation of the config object.
        /// </summary>
        /// <returns>The JSON representation as a JsonObject</returns>
        public JsonObject ToJson()
        {
            JsonObject outputJson = new JsonObject();
            JsonObject buttonMapping = new JsonObject();

            // Add the present buttons in the output element
            foreach(KeyValuePair<string, string> button in this.ButtonMapping) {
                buttonMapping.Add(button.Key, button.Value);
            }

            // Add the missing buttons to the output element
            foreach(string name in Enum.GetNames(typeof(NDSButton))) {
                if(!buttonMapping.ContainsKey(name)) {
                    buttonMapping.Add(name, "");
                }
            }

            outputJson.Add("device", this.Device);
            outputJson.Add("controllerCode", this.ControllerCode);
            outputJson.Add("buttonMapping", buttonMapping);

            return outputJson;
        }
    
        /// <summary>
        ///     Checks if the current configuration object is valid.
        /// </summary>
        /// <returns>True if the configuration object is valid, false if not.</returns>
        public bool IsValid()
        {
            if(this.Device.Length == 0 || this.ButtonMapping.Count == 0) {
                return false;
            }

            return true;
        }
    }
}