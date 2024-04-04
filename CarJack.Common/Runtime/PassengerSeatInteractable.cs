#if PLUGIN
using CommonAPI;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CarJack.Common
{
    public class PassengerSeatInteractable : CustomInteractable
    {
        private CarPassengerSeat _seat;
        private void Awake()
        {
            Core.OnAlwaysUpdate += CoreOnAlwaysUpdate;
        }

        private void CoreOnAlwaysUpdate()
        {
            var player = WorldHandler.instance.GetCurrentPlayer();
            if (player == null) return;
            var test = Test(player);
            if (test != gameObject.activeSelf)
                gameObject.SetActive(test);
        }

        private void OnDestroy()
        {
            Core.OnAlwaysUpdate -= CoreOnAlwaysUpdate;
        }

        public void Initialize(CarPassengerSeat seat)
        {
            _seat = seat;
            Icon = InteractableIcon.Talk;
        }
        public override bool Test(Player player)
        {
            return _seat.Player == null && _seat.Car.DoorsLocked == false;
        }

        public override void Interact(Player player)
        {
            CarController.Instance.EnterCarAsPassenger(_seat.Car, _seat.SeatIndex);
        }
    }
}
#endif