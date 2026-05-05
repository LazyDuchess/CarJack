using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Reptile;
using System.IO;
using CarJack.Common;
using BombRushMP.Plugin;

namespace CarJack.AllCityNetwork
{
    // Yes this sucks.
    // TLDR - There's a host, lowest player ID so everyones on the same page - picks a sub-host every tick, player closest to the ball
    // Sub-host sends ball data to everyone else, so the person whos closest to the ball on the hosts side owns the ball.
    public class BallController : MonoBehaviour
    {
        private const float LerpMaxDistance = 10f;
        private const float Lerp = 10f;
        private const string BallSubHostPacketGUID = "CarJack-Ball-SubHost";
        private const string BallHostPacketGUID = "CarJack-Ball-Host";
        private const string BallPacketGUID = "CarJack-Ball";
        private const float TickRate = 0.2f;
        private const string BallGameObjectName = "rocket ball";
        private GameObject _ball;
        private Rigidbody _ballRB;
        private float _currentTick = TickRate;
        private bool _host = false;
        private bool _subHost = false;
        private bool _hostFound = false;
        private bool _subHostFound = false;
        private Vector3 _receivedPosition;
        private Quaternion _receivedRotation;

        public static void Initialize()
        {
            StageManager.OnStageInitialized += StageManager_OnStageInitialized;
        }

        private void Awake()
        {
            ClientController.RegisterCustomPacketHandler(BallPacketGUID, OnBallPacketReceived);
            ClientController.RegisterCustomPacketHandler(BallHostPacketGUID, (ushort ply, byte[] data) => { OnBallHostPacketReceived(ply, data, false); });
            ClientController.RegisterCustomPacketHandler(BallSubHostPacketGUID, (ushort ply, byte[] data) => { OnBallHostPacketReceived(ply, data, true); });
        }

        private void PacketHandler()
        {

        }

        private void Update()
        {
            if (Core.Instance.IsCorePaused) return;
            if (!_hostFound) return;
            if (!_subHostFound) return;
            if (_subHost)
            {
                _receivedPosition = _ballRB.position;
                _receivedRotation = _ballRB.rotation;
                return;
            }
            var dist = (_ballRB.position - _receivedPosition).magnitude;
            if (dist >= LerpMaxDistance)
            {
                _ballRB.MovePosition(_receivedPosition);
                _ballRB.MoveRotation(_receivedRotation);
            }
            else
            {
                var interpolatedPos = Vector3.Lerp(_ballRB.position, _receivedPosition, Lerp * Time.deltaTime);
                var interpolatedRot = Quaternion.Lerp(_ballRB.rotation, _receivedRotation, Lerp * Time.deltaTime);
                _ballRB.MovePosition(interpolatedPos);
                _ballRB.MoveRotation(interpolatedRot);
            }
        }

        private void UpdateHost()
        {
            var cars = NetworkController.Instance.PlayerCars;
            var lowestDistance = float.MaxValue;
            var lowestDistancePlayer = ushort.MaxValue;
            var myDistance = float.MaxValue;
            var currentCar = CarController.Instance.CurrentCar;
            if (currentCar != null)
            {
                myDistance = (currentCar.Rigidbody.position - _ballRB.position).magnitude;
            }
            foreach(var playerCar in cars)
            {
                var dist = (playerCar.LastPacket.Position - _ballRB.position).magnitude;
                if (dist < lowestDistance)
                {
                    lowestDistance = dist;
                    lowestDistancePlayer = playerCar.PlayerID;
                }
            }
            if (lowestDistancePlayer == uint.MaxValue)
            {
                _subHost = true;
                _subHostFound = true;
                SendBallHostPacket(ushort.MaxValue, true);
            }
            else
            {
                if (myDistance < lowestDistance)
                {
                    _subHost = true;
                    _subHostFound = true;
                    SendBallHostPacket(ushort.MaxValue, true);
                }
                else
                {
                    _subHost = false;
                    _subHostFound = true;
                    SendBallHostPacket(lowestDistancePlayer, true);
                }
            }
        }

        private void FixedUpdate()
        {
            if (Core.Instance.IsCorePaused) return;
            if (!ClientController.Instance.Connected) return;
            _currentTick -= Time.deltaTime;
            if (_currentTick <= 0f)
            {
                _currentTick = TickRate;
                if (_subHost)
                    SendBallPacket();
                if (!_host)
                {
                    var players = ClientController.Instance.Players.Keys;
                    var lowestID = ushort.MaxValue;
                    foreach(var player in players)
                    {
                        if (player <= lowestID && NetworkController.Instance.PlayerHasCar(player))
                            lowestID = player;
                    }
                    if (lowestID != ushort.MaxValue)
                    {
                        SendBallHostPacket(lowestID, false);
                        _hostFound = true;
                    }
                    else
                    {
                        _hostFound = false;
                        _subHostFound = false;
                    }
                }
                else
                {
                    UpdateHost();
                }
            }
        }

        private void OnDestroy()
        {
            ClientController.UnregisterCustomPacketHandler(BallPacketGUID);
            ClientController.UnregisterCustomPacketHandler(BallHostPacketGUID);
            ClientController.UnregisterCustomPacketHandler(BallSubHostPacketGUID);
        }

        private void SendBallHostPacket(ushort playerID, bool subHost)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            //version
            writer.Write((byte)0);

            writer.Write(playerID);

            writer.Flush();
            if (subHost)
                ClientController.Instance.BroadcastCustomPacket(ms.ToArray(), BallSubHostPacketGUID);
            else
                ClientController.Instance.BroadcastCustomPacket(ms.ToArray(), BallHostPacketGUID);
            writer.Close();
        }

        private void SendBallPacket()
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            var pos = _ballRB.position;
            var rot = _ballRB.rotation;

            var vel = _ballRB.velocity;
            var avel = _ballRB.angularVelocity;

            //version
            writer.Write((byte)0);

            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);

            writer.Write(rot.x);
            writer.Write(rot.y);
            writer.Write(rot.z);
            writer.Write(rot.w);

            writer.Write(vel.x);
            writer.Write(vel.y);
            writer.Write(vel.z);

            writer.Write(avel.x);
            writer.Write(avel.y);
            writer.Write(avel.z);

            writer.Flush();
            ClientController.Instance.BroadcastCustomPacket(ms.ToArray(), BallPacketGUID, BombRushMP.Common.Networking.IMessage.SendModes.Unreliable);
            writer.Close();
        }

        private void OnBallHostPacketReceived(ushort playerid, byte[] data, bool subhost)
        {
            if (playerid == ClientController.Instance.LocalID) return;
            var ms = new MemoryStream(data);
            var reader = new BinaryReader(ms);
            var version = reader.ReadByte();
            var hostID = reader.ReadUInt16();
            reader.Close();
            if (ClientController.Instance.Players.ContainsKey(hostID) || hostID == ushort.MaxValue)
            {
                if (!subhost)
                {
                    _host = false;
                    _hostFound = true;
                }
                else
                {
                    _subHost = false;
                    _subHostFound = true;
                }
            }
            else
            {
                if (!subhost)
                {
                    _host = true;
                    _hostFound = true;
                }
                else
                {
                    _subHost = true;
                    _subHostFound = true;
                }
            }
        }

        private void OnBallPacketReceived(ushort playerid, byte[] data)
        {
            if (playerid == ClientController.Instance.LocalID) return;
            if (_subHost) return;
            var ms = new MemoryStream(data);
            var reader = new BinaryReader(ms);

            var version = reader.ReadByte();

            var posX = reader.ReadSingle();
            var posY = reader.ReadSingle();
            var posZ = reader.ReadSingle();

            var rotX = reader.ReadSingle();
            var rotY = reader.ReadSingle();
            var rotZ = reader.ReadSingle();
            var rotW = reader.ReadSingle();

            var velX = reader.ReadSingle();
            var velY = reader.ReadSingle();
            var velZ = reader.ReadSingle();

            var avelX = reader.ReadSingle();
            var avelY = reader.ReadSingle();
            var avelZ = reader.ReadSingle();

            _ballRB.velocity = new Vector3(velX, velY, velZ);
            _ballRB.angularVelocity = new Vector3(avelX, avelY, avelZ);
            _receivedPosition = new Vector3(posX, posY, posZ);
            _receivedRotation = new Quaternion(rotX, rotY, rotZ, rotW);
            //_ballRB.MovePosition(new Vector3(posX, posY, posZ));
            //_ballRB.MoveRotation(new Quaternion(rotX, rotY, rotZ, rotW));

            reader.Close();
        }

        private static void StageManager_OnStageInitialized()
        {
            var ball = GameObject.Find(BallGameObjectName);
            if (ball == null)
                return;
            Create(ball);
        }

        private static BallController Create(GameObject ball)
        {
            var gameObject = new GameObject("Ball Controller");
            var controller = gameObject.AddComponent<BallController>();
            controller.SetBall(ball);
            return controller;
        }

        private void SetBall(GameObject ball)
        {
            _ball = ball;
            _ballRB = ball.GetComponentInChildren<Rigidbody>();
            _receivedPosition = _ballRB.position;
            _receivedRotation = _ballRB.rotation;
        }
    }
}
