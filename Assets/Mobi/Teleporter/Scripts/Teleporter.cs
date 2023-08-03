using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using Random = UnityEngine.Random;

namespace Mobi.Teleporter.Scripts
{
    public class Teleporter : UdonSharpBehaviour
    {

        [SerializeField, Tooltip("If checked, users teleport to destinations in order listed. If unchecked, users are teleported randomly.")]
        private bool _sequential = true;

        [SerializeField, Tooltip("Where to transport anyone who interacts with the object this script is on.")]
        private Transform[] _destinations;

        [UdonSynced]
        private sbyte _destinationIndex = -1;

        public void Start()
        {
            _log("Starting script");
            
            if (_destinations == null || _destinations.Length == 0)
            {
                _log("No destinations set");
            }
        }

        public override void Interact()
        {
            if (_destinations != null && _destinations.Length > 0)
            {
                _changeDestinationIndex();
                _teleportToDestination();
            }
        }
        
        private void _log(string message)
        {
            Debug.Log("[Reflections Teleport] " + message);
        }

        private void _changeDestinationIndex()
        {
            // Make the new user own this object so they send the synced destination index out
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            // If a random order is desired, store the current index and create a random index
            var desiredIndex = _destinationIndex;
            if ( ! _sequential)
            {
                desiredIndex = Convert.ToSByte(Random.Range(0, _destinations.Length));
            }

            // If a sequential order is desired OR our random index chooses the last chosen index, add one
            if (_sequential || desiredIndex == _destinationIndex)
            {
                desiredIndex += 1;
            }

            // Roll back to the first destination index if we've exceeded bounds
            if (desiredIndex >= _destinations.Length)
            {
                desiredIndex = 0;
            }
            
            // And finally, set our synced variable
            _destinationIndex = desiredIndex;
            RequestSerialization();
        }

        private void _teleportToDestination()
        {

            // If there is a null destination in the provided list, do nothing
            var destination = _destinations[_destinationIndex];
            if (destination == null)
            {
                return;
            }

            // Send the user out to the destination
            Networking.LocalPlayer.TeleportTo(
                destination.position,
                destination.rotation,
                VRC_SceneDescriptor.SpawnOrientation.Default,
                false
            );
        }
    }
}

