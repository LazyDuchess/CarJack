using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public class CarPassengerSeat : CarSeat
    {
        public int SeatIndex = 0;

#if PLUGIN
        protected override void Awake()
        {
            base.Awake();
            var trigger = GetComponentInChildren<BoxCollider>();
            if (trigger == null) return;
            var interactable = trigger.gameObject.AddComponent<PassengerSeatInteractable>();
            interactable.Initialize(this);
        }
#endif
    }
}
