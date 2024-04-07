using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if PLUGIN
using Reptile;
#endif

namespace CarJack.Common
{
    [RequireComponent(typeof(AudioSource))]
    public class ScrapeAudio : MonoBehaviour
    {
        public float SpeedMultiplier = 0.1f;
        public float MinimumSpeed = 5f;
        public float LerpSpeed = 10f;
        private AudioSource _audioSource;
        private DrivableCar _car;
        private float _targetVolume = 0f;
        private void Awake()
        {
            _car = GetComponentInParent<DrivableCar>();
            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = 0f;
        }

        public void OnScrape(Collision other)
        {
            if (other.gameObject.name == "rocket ball")
                return;
            var impactVel = _car.Rigidbody.velocity;
            if (other.rigidbody != null)
            {
                impactVel += other.rigidbody.velocity;
            }
            var normal = other.contacts[0].normal;
            var velocityAway = (impactVel - Vector3.Project(_car.Rigidbody.velocity, normal)).magnitude;
            if (velocityAway > MinimumSpeed)
            {
                velocityAway -= MinimumSpeed;
                velocityAway = Mathf.Min(0.5f, velocityAway * SpeedMultiplier);
                _targetVolume = velocityAway;
            }
        }

        private void FixedUpdate()
        {
            _targetVolume = 0f;
        }

        private void Update()
        {
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;
#endif
            _audioSource.volume = Mathf.Lerp(_audioSource.volume, _targetVolume, LerpSpeed * Time.deltaTime);
        }
    }
}
