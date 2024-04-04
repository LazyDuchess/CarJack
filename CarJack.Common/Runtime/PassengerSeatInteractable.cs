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
        public void Initialize(CarPassengerSeat seat)
        {
            _seat = seat;
            Icon = InteractableIcon.Talk;
        }
        public override bool Test(Player player)
        {
            return _seat.Player == null;
        }

        public override void Interact(Player player)
        {
            CarController.Instance.EnterCarAsPassenger(_seat.Car, _seat.SeatIndex);
        }
    }
}
#endif