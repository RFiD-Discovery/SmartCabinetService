using MQTTnet.Client;
using MQTTnet;
using SmartCabinetService.Repository;
using System;
using System.Collections.Generic;
using Topshelf.Logging;
using SmartCabinetService.Properties;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using SmartCabinetService.Classes;
using System.Linq;
using SmartCabinetService.Constants;
using System.Data;
using System.Diagnostics;

namespace SmartCabinetService
{
    class SmartCabinetService
    {
        private static readonly LogWriter _log = HostLogger.Get<SmartCabinetService>();
        private DiscoveryRepository _repository;
        private List<web_search> _assets;
        private IMqttClient _mqttClient;
        private int _cabinetLocationId;
        private string _brokerAddress;
        private int _port;
        private string _clientId;
        private string _topic;

        public async Task<bool> StartAsync()
        {
            try
            {
                _brokerAddress = Settings.Default.BrokerAdress;
                _port = Convert.ToInt32(Settings.Default.Port);
                _clientId = Settings.Default.ClientId;
                _topic = Settings.Default.Topic;
                _cabinetLocationId = Convert.ToInt32(Settings.Default.CabinetLocationId);

                LoadAssets();
                await SetupMQTTBrokerAsync();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            return true;
        }

        private async Task SetupMQTTBrokerAsync()
        {
            try
            {
                // Create an MQTT client factory and options
                var factory = new MqttFactory();
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_brokerAddress, _port) // Replace with your MQTT broker's address
                    .WithClientId(_clientId)            // Choose a unique client ID
                    .Build();

                _log.Info($"MQTT client created - Address: {_brokerAddress} | Port: {_port} | Client Id: {_clientId}");

                // Create an MQTT client instance
                _mqttClient = factory.CreateMqttClient();
                _mqttClient.ApplicationMessageReceivedAsync += HandleApplicationMessageReceivedAsync;
                _mqttClient.DisconnectedAsync += MqttClientOnDisconnectedAsync;

                // Connect
                await ConnectToMQTTClientAsync(options);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        private async Task ConnectToMQTTClientAsync(MqttClientOptions options)
        {
            try
            {
                while (!_mqttClient.IsConnected)
                {
                    await _mqttClient.ConnectAsync(options, CancellationToken.None);

                    if (_mqttClient.IsConnected)
                    {
                        // Subscribe to a topic

                        var subscribingOptions = new MqttClientSubscribeOptionsBuilder()
                            .WithTopicFilter(_topic)
                            .Build();

                        await _mqttClient.SubscribeAsync(subscribingOptions);
                        _log.Info($"Subcribed to: {_topic}");

                        _log.Info($"Connected to MQTT client");
                    }
                    else
                    {
                        _log.Info($"Failed to connect to MQTT client");

                        Thread.Sleep(5000);

                        await ConnectToMQTTClientAsync(options);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        private async Task MqttClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            try
            {
                // Use the current options as the new options.
                await ConnectToMQTTClientAsync(_mqttClient.Options);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        private Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            try
            {

                var payload = arg.ApplicationMessage.ConvertPayloadToString();

                if (!string.IsNullOrEmpty(payload))
                {
                    var tagEvents = JsonConvert.DeserializeObject<TagEvents>(payload);

                    if (tagEvents.tag_reads != null)
                    {
                        _log.Info($"{tagEvents.tag_reads.Count}");
                        if (tagEvents.tag_reads.Count > 0)
                        {
                            ProcessAssets(tagEvents.tag_reads);
                        }                       
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            return Task.CompletedTask;
        }

        private void ProcessAssets(List<TagEvents.TagRead> tagReads)
        {
            try
            {
                _repository = new DiscoveryRepository();

                // Find all assets that have been reported (found)
                var assetsFound = _assets.Where(asset => tagReads.Any(tagRead => tagRead.epc == asset.passive_tag_id)).ToList();
                _log.Info($"Assets found: {assetsFound.Count}");

                foreach (var asset in _assets) 
                {
                    // Default to unknown locaiton
                    var locationId = -1;

                    if (assetsFound.Contains(asset))
                    {
                        locationId = _cabinetLocationId;
                    }

                    // Parameter list: asset id, location id, timestamp, mobile device id, tag type, user id, rfid reader id
                    var args = new object[] { asset.rfid_asset_id, locationId, DateTimeOffset.Now, 0, 2, null, null };
                    var sql = StoredProcedures.AssetLocationUpate;

                    _repository.ExecuteSql(sql, args);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        private void LoadAssets()
        {
            try
            {
                _repository = new DiscoveryRepository();

                _assets = _repository.FindAll<web_search>(a => !string.IsNullOrEmpty(a.passive_tag_id));
                int targetLength = 24;
                char paddingChar = '0';
                foreach (var asset in _assets)
                {
                    asset.passive_tag_id = asset.passive_tag_id.PadLeft(targetLength, paddingChar);
                    
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public bool Stop()
        {
            _log.Info("RFiD Discovery Smart Cabinet Service Stopped");

            return true;
        }
    }
}
