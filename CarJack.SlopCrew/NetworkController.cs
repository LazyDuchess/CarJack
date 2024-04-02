using CarJack.Common;
using Reptile;
using SlopCrew.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.SlopCrew
{
    // Network cars!
    public class NetworkController : MonoBehaviour
    {
        public List<PlayerCarData> PlayerCars;
        public static NetworkController Instance { get; private set; }
        private const float LerpMaxDistance = 10f;
        private const float Lerp = 10f;
        private const float TickRate = 0.1f;
        private Dictionary<uint, PlayerCarData> _playerCarsById;
        private ISlopCrewAPI _api;
        private float _currentTick = TickRate;
        public static void Initialize()
        {
            StageManager.OnStageInitialized += StageManager_OnStageInitialized;
        }

        private static void StageManager_OnStageInitialized()
        {
            Create();
        }

        private static NetworkController Create()
        {
            var gameObject = new GameObject("CarJack Network Controller");
            var controller = gameObject.AddComponent<NetworkController>();
            return controller;
        }

        private void Awake()
        {
            Instance = this;
            PlayerCars = new();
            _playerCarsById = new();
            _api = APIManager.API;
            _api.OnCustomPacketReceived += _api_OnCustomPacketReceived;
        }
        
        private void OnDestroy()
        {
            _api.OnCustomPacketReceived -= _api_OnCustomPacketReceived;
        }

        private void _api_OnCustomPacketReceived(uint playerId, string guid, byte[] data)
        {
            if (guid != PlayerCarPacket.GUID)
                return;
            var packet = new PlayerCarPacket();
            var ms = new MemoryStream(data);
            var reader = new BinaryReader(ms);
            packet.Deserialize(reader);
            reader.Close();
            if (!_playerCarsById.TryGetValue(playerId, out var playerCarData))
            {
                playerCarData = new PlayerCarData();
                playerCarData.PlayerID = playerId;
                PlayerCars.Add(playerCarData);
                _playerCarsById[playerId] = playerCarData;
            }
            playerCarData.LastPacket = packet;
        }

        public bool PlayerHasCar(uint playerId)
        {
            if (!_playerCarsById.TryGetValue(playerId, out var playerCarData))
                return false;
            if (playerCarData.Car != null)
                return true;
            return false;
        }

        private void FixedUpdate()
        {
            if (!_api.Connected) return;
            _currentTick -= Time.deltaTime;
            if (_currentTick <= 0f)
            {
                _currentTick = Time.deltaTime;
                Tick();
            }
        }

        private void Tick()
        {
            var packet = new PlayerCarPacket();
            if (CarController.Instance.CurrentCar != null)
            {
                var car = CarController.Instance.CurrentCar;

                packet.CarInternalName = car.InternalName;

                packet.Position = car.Rigidbody.position;
                packet.Rotation = car.Rigidbody.rotation;

                packet.Velocity = car.Rigidbody.velocity;
                packet.AngularVelocity = car.Rigidbody.angularVelocity;

                packet.ThrottleAxis = car.ThrottleAxis;
                packet.SteerAxis = car.SteerAxis;
                packet.HornHeld = car.HornHeld;
            }
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);
            packet.Serialize(writer);
            writer.Flush();
            _api.SendCustomPacket(PlayerCarPacket.GUID, ms.ToArray());
            writer.Close();

            var newList = new List<PlayerCarData>();
            var newDict = new Dictionary<uint, PlayerCarData>();
            for(var i = 0; i < PlayerCars.Count; i++)
            {
                var keep = TickCar(PlayerCars[i]);
                if (keep)
                {
                    newList.Add(PlayerCars[i]);
                    newDict[PlayerCars[i].PlayerID] = PlayerCars[i];
                }
            }
            PlayerCars = newList;
            _playerCarsById = newDict;
        }

        private void Update()
        {
            if (Core.Instance.IsCorePaused) return;
            foreach (var car in PlayerCars)
            {
                if (car.Car == null) continue;
                var interpolatedPos = Vector3.Lerp(car.Car.Rigidbody.position, car.LastPacket.Position, Lerp * Time.deltaTime);
                var interpolatedRot = Quaternion.Lerp(car.Car.Rigidbody.rotation, car.LastPacket.Rotation, Lerp * Time.deltaTime);
                var dist = (car.Car.Rigidbody.position - car.LastPacket.Position).magnitude;
                if (dist >= LerpMaxDistance)
                {
                    interpolatedPos = car.LastPacket.Position;
                    interpolatedRot = car.LastPacket.Rotation;
                }
                car.Car.Rigidbody.MovePosition(interpolatedPos);
                car.Car.Rigidbody.MoveRotation(interpolatedRot);
            }
        }

        private bool TickCar(PlayerCarData playerCarData)
        {
            var keep = true;
            if (_api.PlayerIDExists(playerCarData.PlayerID) == false)
            {
                playerCarData.LastPacket.CarInternalName = "";
                keep = false;
            }
            var player = Utility.GetPlayer(playerCarData.PlayerID);
            if (playerCarData.LastPacket.CarInternalName == "")
            {
                if (player != null)
                {
                    player.characterVisual.gameObject.SetActive(true);
                    player.EnablePlayer();
                }
                if (playerCarData.Car != null)
                {
                    Destroy(playerCarData.Car.gameObject);
                    playerCarData.Car = null;
                }
                if (playerCarData.Polo != null)
                {
                    playerCarData.Polo.SetActive(true);
                }
            }
            else
            {
                var car = "carjack.bluecar";
                if (CarDatabase.CarByInternalName.TryGetValue(playerCarData.LastPacket.CarInternalName, out var result))
                    car = result.GetComponent<DrivableCar>().InternalName;

                var currentCar = playerCarData.Car;
                if (currentCar == null || currentCar.InternalName != car)
                {
                    
                    if (currentCar != null)
                    {
                        Destroy(currentCar.gameObject);
                    }
                    var carGO = Instantiate(CarDatabase.CarByInternalName[car]);
                    carGO.transform.position = playerCarData.LastPacket.Position;
                    carGO.transform.rotation = playerCarData.LastPacket.Rotation;
                    currentCar = carGO.GetComponent<DrivableCar>();
                    currentCar.Initialize();
                    currentCar.EnterCar(player);
                    var playerId = playerCarData.PlayerID;
                    currentCar.OnHandleInput += () =>
                    {
                        if (_playerCarsById.TryGetValue(playerId, out var result)){
                            currentCar.ThrottleAxis = result.LastPacket.ThrottleAxis;
                            currentCar.SteerAxis = result.LastPacket.SteerAxis;
                            currentCar.HornHeld = result.LastPacket.HornHeld;
                        }
                    };
                }

                if (currentCar != null)
                {
                    if (player != null)
                    {
                        player.characterVisual.gameObject.SetActive(false);
                        player.transform.position = currentCar.transform.position;
                        player.DisablePlayer();
                        var playersPolo = player.transform.Find("Mascot_Polo_street(Clone)");
                        if (playersPolo != null)
                        {
                            playerCarData.Polo = playersPolo.gameObject;
                            playersPolo.gameObject.SetActive(false);
                        }
                    }
                    currentCar.Rigidbody.velocity = playerCarData.LastPacket.Velocity;
                    currentCar.Rigidbody.angularVelocity = playerCarData.LastPacket.AngularVelocity;
                }
                else
                {
                    if (player != null)
                    {
                        player.characterVisual.gameObject.SetActive(true);
                        player.EnablePlayer();
                        if (playerCarData.Polo != null)
                            playerCarData.Polo.SetActive(true);
                    }
                }

                playerCarData.Car = currentCar;
            }
            return keep;
        }
    }
}
