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
using Slop = SlopCrew;

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
        private const float TransformTickRate = 0.2f;
        private const float StateTickRate = 0.8f;
        private Dictionary<uint, PlayerCarData> _playerCarsById;
        private ISlopCrewAPI _api;
        private float _currentTransformTick = TransformTickRate;
        private float _currentStateTick = StateTickRate;
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
            guid = SlopCrewExtensions.GetPacketID(guid);
            var ms = new MemoryStream(data);
            var reader = new BinaryReader(ms);
            switch (guid)
            {
                case KickPassengersPacketGUID:
                    OnKickPassengersPacketReceived(playerId, reader);
                    break;
                case PlayerCarStatePacket.GUID:
                    OnPlayerCarStatePacketReceived(playerId, reader);
                    break;
                case PlayerCarTransformPacket.GUID:
                    OnPlayerCarTransformPacketReceived(playerId, reader);
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

        private void OnPlayerCarStatePacketReceived(uint playerId, BinaryReader reader)
        {
            var packet = new PlayerCarStatePacket();
            packet.Deserialize(reader);
            if (!_playerCarsById.TryGetValue(playerId, out var playerCarData))
            {
                playerCarData = new PlayerCarData();
                playerCarData.PlayerID = playerId;
                PlayerCars.Add(playerCarData);
                _playerCarsById[playerId] = playerCarData;
                playerCarData.StatePacket = new PlayerCarStatePacket();
                playerCarData.TransformPacket = new PlayerCarTransformPacket();
            }
            playerCarData.StatePacket = packet;
        }

        private void OnPlayerCarTransformPacketReceived(uint playerId, BinaryReader reader)
        {
            var packet = new PlayerCarTransformPacket();
            packet.Deserialize(reader);
            if (!_playerCarsById.TryGetValue(playerId, out var playerCarData))
            {
                playerCarData = new PlayerCarData();
                playerCarData.PlayerID = playerId;
                PlayerCars.Add(playerCarData);
                _playerCarsById[playerId] = playerCarData;
                playerCarData.StatePacket = new PlayerCarStatePacket();
                playerCarData.TransformPacket = new PlayerCarTransformPacket();
            }
            playerCarData.TransformPacket = packet;
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
            _currentTransformTick -= Time.deltaTime;
            _currentStateTick -= Time.deltaTime;
            if (_currentTransformTick <= 0f)
            {
                _currentTransformTick = TransformTickRate;
                TickTransform();
            }
            if (_currentStateTick <= 0f)
            {
                _currentStateTick = StateTickRate;
                TickState();
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

        private void TickState()
        {
            var packet = new PlayerCarStatePacket();
            if (CarController.Instance.CurrentCar != null)
            {
                var car = CarController.Instance.CurrentCar;

                if (CarController.Instance.CurrentSeat == null)
                    packet.CarInternalName = car.InternalName;

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
            SlopCrewExtensions.SendCustomPacket(PlayerCarStatePacket.GUID, ms.ToArray(), Slop.Common.SendFlags.Unreliable);
            writer.Close();
        }

        private void TickTransform()
        {
            var packet = new PlayerCarTransformPacket();
            if (CarController.Instance.CurrentCar != null)
            {
                var car = CarController.Instance.CurrentCar;

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
            SlopCrewExtensions.SendCustomPacket(PlayerCarTransformPacket.GUID, ms.ToArray(), Slop.Common.SendFlags.Unreliable);
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
                if (car.TransformPacket == null) continue;
                var interpolatedPos = Vector3.Lerp(car.Car.Rigidbody.position, car.TransformPacket.Position, Lerp * Time.deltaTime);
                var interpolatedRot = Quaternion.Lerp(car.Car.Rigidbody.rotation, car.TransformPacket.Rotation, Lerp * Time.deltaTime);
                var dist = (car.Car.Rigidbody.position - car.TransformPacket.Position).magnitude;
                if (dist >= LerpMaxDistance)
                {
                    interpolatedPos = car.TransformPacket.Position;
                    interpolatedRot = car.TransformPacket.Rotation;
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
            if (playerCarData.StatePacket.PassengerSeat != -1)
                playerCarData.StatePacket.CarInternalName = "carjack.bluecar";

            if (_api.PlayerIDExists(playerCarData.PlayerID) == false)
            {
                playerCarData.StatePacket.CarInternalName = "";
                keep = false;
            }
            var player = Utility.GetPlayer(playerCarData.PlayerID);
            if (playerCarData.StatePacket.CarInternalName == "")
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
                if (CarDatabase.CarByInternalName.TryGetValue(playerCarData.StatePacket.CarInternalName, out var result))
                    car = result.Prefab.GetComponent<DrivableCar>().InternalName;
                else
                    missingCar = true;

                var currentCar = playerCarData.Car;

                if (playerCarData.StatePacket.PassengerSeat != -1)
                {
                    var pasCar = GetPlayersCar(playerCarData.StatePacket.DriverPlayerID);
                    currentCar = pasCar;
                    if (pasCar != null)
                    {
                        var targetSeat = pasCar.GetPassengerSeat(playerCarData.StatePacket.PassengerSeat);
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
                        carGO.transform.position = playerCarData.TransformPacket.Position;
                        carGO.transform.rotation = playerCarData.TransformPacket.Rotation;
                        currentCar = carGO.GetComponent<DrivableCar>();
                        currentCar.Initialize();
                        currentCar.EnterCar(player);
                        var playerId = playerCarData.PlayerID;
                        currentCar.OnHandleInput += () =>
                        {
                            if (_playerCarsById.TryGetValue(playerId, out var result))
                            {
                                currentCar.ThrottleAxis = result.TransformPacket.ThrottleAxis;
                                currentCar.SteerAxis = result.TransformPacket.SteerAxis;
                                currentCar.HornHeld = result.TransformPacket.HornHeld;
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
                        currentCar.Rigidbody.velocity = playerCarData.TransformPacket.Velocity;
                        currentCar.Rigidbody.angularVelocity = playerCarData.TransformPacket.AngularVelocity;
                        currentCar.DoorsLocked = missingCar ? true : playerCarData.StatePacket.DoorsLocked;

                        var recolorable = currentCar.GetComponent<RecolorableCar>();
                        if (recolorable != null)
                        {
                            Recolor recolor = null;
                            if (!string.IsNullOrEmpty(playerCarData.StatePacket.RecolorGUID))
                            {
                                if (RecolorManager.RecolorsByGUID.TryGetValue(playerCarData.StatePacket.RecolorGUID, out var recResult))
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
