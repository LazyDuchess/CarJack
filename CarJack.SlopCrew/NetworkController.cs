using CarJack.Common;
using CarJack.Common.WhipRemix;
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
        private const byte KickPassengersPacketVersion = 0;
        private const string KickPassengersPacketGUID = "CarJack-KickPassengers";
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
            CarController.OnPlayerExitingCar += SendKickPassengersPacket;
        }
        
        private void OnDestroy()
        {
            _api.OnCustomPacketReceived -= _api_OnCustomPacketReceived;
            CarController.OnPlayerExitingCar -= SendKickPassengersPacket;
        }

        private void _api_OnCustomPacketReceived(uint playerId, string guid, byte[] data)
        {
            var ms = new MemoryStream(data);
            var reader = new BinaryReader(ms);
            switch (guid)
            {
                case KickPassengersPacketGUID:
                    OnKickPassengersPacketReceived(playerId, reader);
                    break;
                case PlayerCarPacket.GUID:
                    OnPlayerCarDataPacketReceived(playerId, reader);
                    break;
                default:
                    break;
            }
            reader.Close();
        }

        private void SendKickPassengersPacket()
        {
            var currentCar = CarController.Instance.CurrentCar;
            if (currentCar == null) return;
            if (!currentCar.Driving) return;
            _api.SendCustomPacket(KickPassengersPacketGUID, [KickPassengersPacketVersion]);
        }

        private PlayerCarData GetPlayerForCar(DrivableCar car)
        {
            foreach(var player in _playerCarsById)
            {
                if (player.Value.Car == car) return player.Value;
            }
            return null;
        }

        private void OnKickPassengersPacketReceived(uint playerId, BinaryReader reader)
        {
            var version = reader.ReadByte();
            var carController = CarController.Instance;
            if (carController == null) return;
            var car = carController.CurrentCar;
            if (car == null) return;
            if (car.Driving) return;
            var playerCar = GetPlayerForCar(car);
            if (playerCar == null) return;
            if (playerCar.Seat != null) return;
            if (playerCar.PlayerID != playerId) return;
            carController.ExitCar();
        }

        private void OnPlayerCarDataPacketReceived(uint playerId, BinaryReader reader)
        {
            var packet = new PlayerCarPacket();
            packet.Deserialize(reader);
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

        public DrivableCar GetPlayersCar(uint playerId)
        {
            if (_api.PlayerIDExists(playerId) == false)
                return CarController.Instance.CurrentCar;
            if (!_playerCarsById.TryGetValue(playerId, out var playerCarData))
                return null;
            return playerCarData.Car;
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

        public uint GetDriver(DrivableCar car)
        {
            foreach(var playerCar in PlayerCars)
            {
                if (playerCar.Car == car)
                    return playerCar.PlayerID;
            }
            return uint.MaxValue;
        }

        private void Tick()
        {
            var packet = new PlayerCarPacket();
            if (CarController.Instance.CurrentCar != null)
            {
                var car = CarController.Instance.CurrentCar;

                if (CarController.Instance.CurrentSeat == null)
                    packet.CarInternalName = car.InternalName;

                packet.Position = car.Rigidbody.position;
                packet.Rotation = car.Rigidbody.rotation;

                packet.Velocity = car.Rigidbody.velocity;
                packet.AngularVelocity = car.Rigidbody.angularVelocity;

                packet.ThrottleAxis = car.ThrottleAxis;
                packet.SteerAxis = car.SteerAxis;
                packet.HornHeld = car.HornHeld;

                if (CarController.Instance.CurrentSeat != null)
                {
                    packet.PassengerSeat = CarController.Instance.CurrentSeat.SeatIndex;
                    packet.DriverPlayerID = GetDriver(car);
                }

                packet.DoorsLocked = PlayerData.Instance.DoorsLocked;

                var recolorable = car.GetComponent<RecolorableCar>();

                if (recolorable != null && recolorable.CurrentRecolor != null)
                    packet.RecolorGUID = recolorable.CurrentRecolor.Properties.RecolorGUID;
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
                if (car.Seat != null) continue;
                var interpolatedPos = Vector3.Lerp(car.Car.Rigidbody.position, car.LastPacket.Position, Lerp * Time.deltaTime);
                var interpolatedRot = Quaternion.Lerp(car.Car.Rigidbody.rotation, car.LastPacket.Rotation, Lerp * Time.deltaTime);
                var dist = (car.Car.Rigidbody.position - car.LastPacket.Position).magnitude;
                if (dist >= LerpMaxDistance)
                {
                    interpolatedPos = car.LastPacket.Position;
                    interpolatedRot = car.LastPacket.Rotation;
                    car.Car.transform.position = interpolatedPos;
                    car.Car.transform.rotation = interpolatedRot;
                }
                else
                    car.Car.Rigidbody.MovePosition(interpolatedPos);
                    car.Car.Rigidbody.MoveRotation(interpolatedRot);
            }
        }

        private bool TickCar(PlayerCarData playerCarData)
        {
            var missingCar = false;
            var keep = true;
            if (playerCarData.LastPacket.PassengerSeat != -1)
                playerCarData.LastPacket.CarInternalName = "carjack.bluecar";

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
                    if (playerCarData.Seat == null)
                        Destroy(playerCarData.Car.gameObject);
                    else
                    {
                        if (playerCarData.Seat.Player == player)
                            playerCarData.Seat.ExitSeat();
                    }
                    playerCarData.Seat = null;
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
                    car = result.Prefab.GetComponent<DrivableCar>().InternalName;
                else
                    missingCar = true;

                var currentCar = playerCarData.Car;

                if (playerCarData.LastPacket.PassengerSeat != -1)
                {
                    var pasCar = GetPlayersCar(playerCarData.LastPacket.DriverPlayerID);
                    currentCar = pasCar;
                    if (pasCar != null)
                    {
                        var targetSeat = pasCar.GetPassengerSeat(playerCarData.LastPacket.PassengerSeat);
                        if (targetSeat != null)
                        {
                            player.transform.position = targetSeat.transform.position;
                            if (targetSeat != playerCarData.Seat)
                            {
                                var oldSeat = playerCarData.Seat;
                                if (oldSeat != null)
                                {
                                    if (oldSeat.Player == player)
                                        oldSeat.ExitSeat();
                                }

                                player.characterVisual.gameObject.SetActive(false);
                                player.CompletelyStop();
                                player.DisablePlayer();
                                var playersPolo = player.transform.Find("Mascot_Polo_street(Clone)");
                                if (playersPolo != null)
                                {
                                    playerCarData.Polo = playersPolo.gameObject;
                                    playersPolo.gameObject.SetActive(false);
                                }

                                playerCarData.Seat = targetSeat;
                                targetSeat.PutInSeat(player);
                            }
                        }
                    }
                }
                else
                {
                    if (playerCarData.Seat != null)
                    {
                        if (playerCarData.Seat.Player == player)
                            playerCarData.Seat.ExitSeat();
                        playerCarData.Seat = null;
                    }
                    if (currentCar == null || currentCar.InternalName != car)
                    {

                        if (currentCar != null)
                        {
                            Destroy(currentCar.gameObject);
                        }
                        var carGO = Instantiate(CarDatabase.CarByInternalName[car].Prefab);
                        carGO.transform.position = playerCarData.LastPacket.Position;
                        carGO.transform.rotation = playerCarData.LastPacket.Rotation;
                        currentCar = carGO.GetComponent<DrivableCar>();
                        currentCar.Initialize();
                        currentCar.EnterCar(player);
                        var playerId = playerCarData.PlayerID;
                        currentCar.OnHandleInput += () =>
                        {
                            if (_playerCarsById.TryGetValue(playerId, out var result))
                            {
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
                            player.CompletelyStop();
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
                        currentCar.DoorsLocked = missingCar ? true : playerCarData.LastPacket.DoorsLocked;

                        var recolorable = currentCar.GetComponent<RecolorableCar>();
                        if (recolorable != null)
                        {
                            Recolor recolor = null;
                            if (!string.IsNullOrEmpty(playerCarData.LastPacket.RecolorGUID))
                            {
                                if (RecolorManager.RecolorsByGUID.TryGetValue(playerCarData.LastPacket.RecolorGUID, out var recResult))
                                {
                                    if (recResult.Properties.CarInternalName == currentCar.InternalName)
                                        recolor = recResult;
                                }
                            }

                            if (recolorable.CurrentRecolor != recolor)
                            {
                                if (recolor == null)
                                    recolorable.ApplyDefaultColor();
                                else
                                    recolorable.ApplyRecolor(recolor);
                            }
                        }
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
                }
                playerCarData.Car = currentCar;
            }
            return keep;
        }
    }
}
